using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Hubs;
using CollegeScheduler.Services;
using CollegeScheduler.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Services;

public class RequestServiceTests
{
	private static ApplicationDbContext CreateDbContext(string dbName)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private static RequestService CreateService(
		ApplicationDbContext db,
		Mock<ISchedulingService>? schedulingServiceMock = null,
		Mock<INotificationService>? notificationServiceMock = null)
	{
		schedulingServiceMock ??= new Mock<ISchedulingService>();
		notificationServiceMock ??= new Mock<INotificationService>();

		var hubContextMock = new Mock<IHubContext<TimetableHub>>();
		var hubClientsMock = new Mock<IHubClients>();
		var clientProxyMock = new Mock<IClientProxy>();

		hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);
		hubClientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
		hubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

		var hubLogger = new Mock<ILogger<TimetableHubNotifier>>();
		var notifier = new TimetableHubNotifier(hubContextMock.Object, hubLogger.Object);

		var requestLogger = new Mock<ILogger<RequestService>>();

		return new RequestService(
			db,
			schedulingServiceMock.Object,
			notificationServiceMock.Object,
			notifier,
			requestLogger.Object);
	}

	private static TimetableEvent CreateTimetableEvent(long id, int roomId = 1)
	{
		return new TimetableEvent
		{
			TimetableEventId = id,
			TermId = 1,
			ModuleId = 1,
			RoomId = roomId,
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1,
			SessionType = "Lecture",
			CreatedByUserId = "creator-user"
		};
	}

	private static Room CreateRoom(
		int roomId,
		int capacity = 30,
		bool isBookableByStudents = true,
		bool isActive = true)
	{
		return new Room
		{
			RoomId = roomId,
			BuildingId = 1,
			RoomTypeId = 1,
			Code = $"R{roomId}",
			Name = $"Room {roomId}",
			Capacity = capacity,
			IsBookableByStudents = isBookableByStudents,
			IsActive = isActive
		};
	}

	private static void SeedRequestLookups(ApplicationDbContext db)
	{
		db.RequestTypes.AddRange(
			new RequestType { RequestTypeId = 1, Name = "Reschedule" },
			new RequestType { RequestTypeId = 2, Name = "CancelClass" },
			new RequestType { RequestTypeId = 3, Name = "RoomBooking" }
		);

		db.RequestStatuses.AddRange(
			new RequestStatus { RequestStatusId = 1, Name = "Pending" },
			new RequestStatus { RequestStatusId = 2, Name = "Approved" },
			new RequestStatus { RequestStatusId = 3, Name = "Rejected" }
		);
	}

	[Fact]
	public async Task CreateScheduleChangeRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateScheduleChangeRequestAsync(
			requestedByUserId: "",
			timetableEventId: 1,
			proposedRoomId: 2,
			proposedStartUtc: DateTime.UtcNow,
			proposedEndUtc: DateTime.UtcNow.AddHours(1),
			reason: "Need to move class");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("RequestedByUserId is required.");
	}

	[Fact]
	public async Task CreateScheduleChangeRequestAsync_ShouldThrow_WhenReasonIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequestAsync_ShouldThrow_WhenReasonIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateScheduleChangeRequestAsync(
			requestedByUserId: "student-1",
			timetableEventId: 1,
			proposedRoomId: 2,
			proposedStartUtc: DateTime.UtcNow,
			proposedEndUtc: DateTime.UtcNow.AddHours(1),
			reason: "");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Reason is required.");
	}

	[Fact]
	public async Task CreateScheduleChangeRequestAsync_ShouldThrow_WhenEventDoesNotExist()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequestAsync_ShouldThrow_WhenEventDoesNotExist));
		SeedRequestLookups(db);
		await db.SaveChangesAsync();

		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateScheduleChangeRequestAsync(
			requestedByUserId: "student-1",
			timetableEventId: 999,
			proposedRoomId: 2,
			proposedStartUtc: DateTime.UtcNow,
			proposedEndUtc: DateTime.UtcNow.AddHours(1),
			reason: "Need to move class");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("TimetableEvent 999 not found.");
	}

	[Fact]
	public async Task CreateScheduleChangeRequestAsync_ShouldCreateRequestAndDetail_WhenValid()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequestAsync_ShouldCreateRequestAndDetail_WhenValid));

		SeedRequestLookups(db);
		db.TimetableEvents.Add(CreateTimetableEvent(10));
		await db.SaveChangesAsync();

		var service = CreateService(db);

		var proposedStart = new DateTime(2026, 4, 2, 9, 0, 0, DateTimeKind.Utc);
		var proposedEnd = proposedStart.AddHours(1);

		var requestId = await service.CreateScheduleChangeRequestAsync(
			requestedByUserId: "student-1",
			timetableEventId: 10,
			proposedRoomId: 5,
			proposedStartUtc: proposedStart,
			proposedEndUtc: proposedEnd,
			reason: "Need to move class");

		requestId.Should().BeGreaterThan(0);

		var request = await db.Requests.FirstAsync(x => x.RequestId == requestId);
		request.RequestedByUserId.Should().Be("student-1");
		request.Notes.Should().Be("Need to move class");

		var detail = await db.RequestScheduleChanges.FirstAsync(x => x.RequestId == requestId);
		detail.TimetableEventId.Should().Be(10);
		detail.ProposedRoomId.Should().Be(5);
		detail.ProposedStartUtc.Should().Be(proposedStart);
		detail.ProposedEndUtc.Should().Be(proposedEnd);
		detail.Reason.Should().Be("Need to move class");
	}

	[Fact]
	public async Task CreateCancelClassRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateCancelClassRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateCancelClassRequestAsync(
			requestedByUserId: "",
			timetableEventId: 1,
			reason: "Lecturer unavailable");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("RequestedByUserId is required.");
	}

	[Fact]
	public async Task CreateCancelClassRequestAsync_ShouldThrow_WhenReasonIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateCancelClassRequestAsync_ShouldThrow_WhenReasonIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateCancelClassRequestAsync(
			requestedByUserId: "lecturer-1",
			timetableEventId: 1,
			reason: "");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Reason is required.");
	}

	[Fact]
	public async Task CreateCancelClassRequestAsync_ShouldThrow_WhenEventDoesNotExist()
	{
		using var db = CreateDbContext(nameof(CreateCancelClassRequestAsync_ShouldThrow_WhenEventDoesNotExist));
		SeedRequestLookups(db);
		await db.SaveChangesAsync();

		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateCancelClassRequestAsync(
			requestedByUserId: "lecturer-1",
			timetableEventId: 999,
			reason: "Lecturer unavailable");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("TimetableEvent 999 not found.");
	}

	[Fact]
	public async Task CreateCancelClassRequestAsync_ShouldCreateRequestAndDetail_WhenValid()
	{
		using var db = CreateDbContext(nameof(CreateCancelClassRequestAsync_ShouldCreateRequestAndDetail_WhenValid));

		SeedRequestLookups(db);
		db.TimetableEvents.Add(CreateTimetableEvent(20));
		await db.SaveChangesAsync();

		var service = CreateService(db);

		var requestId = await service.CreateCancelClassRequestAsync(
			requestedByUserId: "lecturer-1",
			timetableEventId: 20,
			reason: "Lecturer unavailable");

		requestId.Should().BeGreaterThan(0);

		var request = await db.Requests.FirstAsync(x => x.RequestId == requestId);
		request.RequestedByUserId.Should().Be("lecturer-1");
		request.Notes.Should().Be("Lecturer unavailable");

		var detail = await db.RequestScheduleChanges.FirstAsync(x => x.RequestId == requestId);
		detail.TimetableEventId.Should().Be(20);
		detail.ProposedRoomId.Should().BeNull();
		detail.ProposedStartUtc.Should().BeNull();
		detail.ProposedEndUtc.Should().BeNull();
		detail.Reason.Should().Be("Lecturer unavailable");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenRequestedByUserIdIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "",
			roomId: 1,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "Study group",
			expectedAttendees: 5);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("RequestedByUserId is required.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenPurposeIsEmpty()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenPurposeIsEmpty));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "",
			expectedAttendees: 5);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Purpose is required.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenEndIsBeforeOrEqualToStart()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenEndIsBeforeOrEqualToStart));
		var service = CreateService(db);

		var start = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: start,
			endUtc: start,
			purpose: "Study group",
			expectedAttendees: 5);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("EndUtc must be greater than StartUtc.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenExpectedAttendeesLessThanOne()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenExpectedAttendeesLessThanOne));
		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "Study group",
			expectedAttendees: 0);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("ExpectedAttendees must be at least 1.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenRoomNotFound()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenRoomNotFound));
		SeedRequestLookups(db);
		await db.SaveChangesAsync();

		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 999,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "Study group",
			expectedAttendees: 5);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Room 999 not found.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenRoomIsNotBookableByStudents()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenRoomIsNotBookableByStudents));

		SeedRequestLookups(db);
		db.Rooms.Add(CreateRoom(roomId: 1, isBookableByStudents: false));
		await db.SaveChangesAsync();

		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "Study group",
			expectedAttendees: 5);

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("This room is not bookable by students.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldThrow_WhenExpectedAttendeesExceedCapacity()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldThrow_WhenExpectedAttendeesExceedCapacity));

		SeedRequestLookups(db);
		db.Rooms.Add(CreateRoom(roomId: 1, capacity: 10, isBookableByStudents: true));
		await db.SaveChangesAsync();

		var service = CreateService(db);

		Func<Task> act = async () => await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: DateTime.UtcNow,
			endUtc: DateTime.UtcNow.AddHours(1),
			purpose: "Study group",
			expectedAttendees: 20);

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Expected attendees exceed room capacity.");
	}

	[Fact]
	public async Task CreateRoomBookingRequestAsync_ShouldCreateRequestAndDetail_WhenValid()
	{
		using var db = CreateDbContext(nameof(CreateRoomBookingRequestAsync_ShouldCreateRequestAndDetail_WhenValid));

		SeedRequestLookups(db);
		db.Rooms.Add(CreateRoom(roomId: 1, capacity: 30, isBookableByStudents: true));
		await db.SaveChangesAsync();

		var service = CreateService(db);

		var start = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
		var end = start.AddHours(2);

		var requestId = await service.CreateRoomBookingRequestAsync(
			requestedByUserId: "student-1",
			roomId: 1,
			startUtc: start,
			endUtc: end,
			purpose: "Study group",
			expectedAttendees: 12);

		requestId.Should().BeGreaterThan(0);

		var request = await db.Requests.FirstAsync(x => x.RequestId == requestId);
		request.RequestedByUserId.Should().Be("student-1");
		request.Notes.Should().Be("Study group");

		var detail = await db.RequestRoomBookings.FirstAsync(x => x.RequestId == requestId);
		detail.RoomId.Should().Be(1);
		detail.StartUtc.Should().Be(start);
		detail.EndUtc.Should().Be(end);
		detail.Purpose.Should().Be("Study group");
		detail.ExpectedAttendees.Should().Be(12);
	}

	[Fact]
	public async Task DecideAsync_ShouldThrow_WhenDecisionIsInvalid()
	{
		using var db = CreateDbContext(nameof(DecideAsync_ShouldThrow_WhenDecisionIsInvalid));
		var service = CreateService(db);

		Func<Task> act = async () => await service.DecideAsync(
			requestId: 1,
			decidedByUserId: "admin-1",
			decision: "Maybe",
			comment: null);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Decision must be 'Approved' or 'Rejected'.");
	}

	[Fact]
	public async Task DecideAsync_ShouldThrow_WhenRequestNotFound()
	{
		using var db = CreateDbContext(nameof(DecideAsync_ShouldThrow_WhenRequestNotFound));
		var service = CreateService(db);

		Func<Task> act = async () => await service.DecideAsync(
			requestId: 999,
			decidedByUserId: "admin-1",
			decision: "Approved",
			comment: null);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Request 999 not found.");
	}

	[Fact]
	public async Task DecideAsync_ShouldReturnUnsuccessfulResult_WhenRequestAlreadyApproved()
	{
		using var db = CreateDbContext(nameof(DecideAsync_ShouldReturnUnsuccessfulResult_WhenRequestAlreadyApproved));

		SeedRequestLookups(db);

		db.Requests.Add(new Request
		{
			RequestId = 100,
			RequestTypeId = 1,
			RequestStatusId = 2,
			RequestedByUserId = "student-1",
			Title = "Request"
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.DecideAsync(
			requestId: 100,
			decidedByUserId: "admin-1",
			decision: "Approved",
			comment: "Already handled");

		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Request has already been approved.");
	}

	[Fact]
	public async Task DecideAsync_ShouldReturnUnsuccessfulResult_WhenRequestAlreadyRejected()
	{
		using var db = CreateDbContext(nameof(DecideAsync_ShouldReturnUnsuccessfulResult_WhenRequestAlreadyRejected));

		SeedRequestLookups(db);

		db.Requests.Add(new Request
		{
			RequestId = 101,
			RequestTypeId = 1,
			RequestStatusId = 3,
			RequestedByUserId = "student-1",
			Title = "Request"
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.DecideAsync(
			requestId: 101,
			decidedByUserId: "admin-1",
			decision: "Rejected",
			comment: "Already handled");

		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Request has already been rejected.");
	}
}