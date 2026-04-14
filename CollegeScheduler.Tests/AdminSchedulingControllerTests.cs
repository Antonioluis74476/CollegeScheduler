using System.Security.Claims;
using CollegeScheduler.Controllers.Api.Admin;
using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Hubs;
using CollegeScheduler.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Controllers;

public class AdminSchedulingControllerTests
{
	private static ApplicationDbContext CreateDbContext(string dbName)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private static TimetableHubNotifier CreateHubNotifier()
	{
		var hubContextMock = new Mock<IHubContext<TimetableHub>>();
		var hubClientsMock = new Mock<IHubClients>();
		var clientProxyMock = new Mock<IClientProxy>();
		var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<TimetableHubNotifier>>();

		hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);
		hubClientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
		hubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

		return new TimetableHubNotifier(hubContextMock.Object, loggerMock.Object);
	}

	private static AdminSchedulingController CreateController(
		ApplicationDbContext db,
		Mock<ISchedulingService>? schedulingMock = null,
		Mock<IRequestService>? requestMock = null,
		Mock<INotificationService>? notificationMock = null,
		string userId = "admin-1")
	{
		schedulingMock ??= new Mock<ISchedulingService>();
		requestMock ??= new Mock<IRequestService>();
		notificationMock ??= new Mock<INotificationService>();

		var controller = new AdminSchedulingController(
			db,
			schedulingMock.Object,
			requestMock.Object,
			notificationMock.Object,
			CreateHubNotifier());

		var user = new ClaimsPrincipal(new ClaimsIdentity(
			new[]
			{
				new Claim(ClaimTypes.NameIdentifier, userId)
			},
			"TestAuth"));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = user
			}
		};

		return controller;
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
			CreatedByUserId = "creator-1"
		};
	}

	[Fact]
	public async Task FindAvailableRooms_ShouldReturnOk_WhenServiceSucceeds()
	{
		using var db = CreateDbContext(nameof(FindAvailableRooms_ShouldReturnOk_WhenServiceSucceeds));
		var schedulingMock = new Mock<ISchedulingService>();

		schedulingMock
			.Setup(x => x.FindAvailableRoomsAsync(It.IsAny<RoomSearchQuery>()))
			.ReturnsAsync(new List<AvailableRoomDto>
			{
				new()
				{
					RoomId = 1,
					Code = "B201",
					Name = "Room B201",
					Capacity = 30,
					BuildingName = "Main",
					CampusName = "Campus"
				}
			});

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.FindAvailableRooms(
			startUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			minCapacity: null,
			roomTypeId: null,
			buildingId: null,
			campusId: null,
			featureIds: null);

		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task FindAvailableRooms_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
	{
		using var db = CreateDbContext(nameof(FindAvailableRooms_ShouldReturnBadRequest_WhenServiceThrowsArgumentException));
		var schedulingMock = new Mock<ISchedulingService>();

		schedulingMock
			.Setup(x => x.FindAvailableRoomsAsync(It.IsAny<RoomSearchQuery>()))
			.ThrowsAsync(new ArgumentException("EndUtc must be greater than StartUtc."));

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.FindAvailableRooms(
			startUtc: new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			minCapacity: null,
			roomTypeId: null,
			buildingId: null,
			campusId: null,
			featureIds: null);

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task CheckClashes_ShouldReturnOk_WhenNoClash()
	{
		using var db = CreateDbContext(nameof(CheckClashes_ShouldReturnOk_WhenNoClash));
		var schedulingMock = new Mock<ISchedulingService>();

		schedulingMock
			.Setup(x => x.CheckClashesAsync(
				It.IsAny<long?>(),
				It.IsAny<int>(),
				It.IsAny<DateTime>(),
				It.IsAny<DateTime>(),
				It.IsAny<IEnumerable<int>>(),
				It.IsAny<IEnumerable<int>>()))
			.ReturnsAsync(new ClashResult());

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.CheckClashes(new ClashCheckRequest
		{
			RoomId = 1,
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc)
		});

		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task CheckClashes_ShouldReturnConflict_WhenClashExists()
	{
		using var db = CreateDbContext(nameof(CheckClashes_ShouldReturnConflict_WhenClashExists));
		var schedulingMock = new Mock<ISchedulingService>();

		var clashResult = new ClashResult
		{
			RoomClash = new ClashDetail(99, "Room clash")
		};

		schedulingMock
			.Setup(x => x.CheckClashesAsync(
				It.IsAny<long?>(),
				It.IsAny<int>(),
				It.IsAny<DateTime>(),
				It.IsAny<DateTime>(),
				It.IsAny<IEnumerable<int>>(),
				It.IsAny<IEnumerable<int>>()))
			.ReturnsAsync(clashResult);

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.CheckClashes(new ClashCheckRequest
		{
			RoomId = 1,
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc)
		});

		result.Should().BeOfType<ConflictObjectResult>();
	}

	[Fact]
	public async Task CheckClashes_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
	{
		using var db = CreateDbContext(nameof(CheckClashes_ShouldReturnBadRequest_WhenServiceThrowsArgumentException));
		var schedulingMock = new Mock<ISchedulingService>();

		schedulingMock
			.Setup(x => x.CheckClashesAsync(
				It.IsAny<long?>(),
				It.IsAny<int>(),
				It.IsAny<DateTime>(),
				It.IsAny<DateTime>(),
				It.IsAny<IEnumerable<int>>(),
				It.IsAny<IEnumerable<int>>()))
			.ThrowsAsync(new ArgumentException("EndUtc must be greater than StartUtc."));

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.CheckClashes(new ClashCheckRequest
		{
			RoomId = 1,
			StartUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)
		});

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task CreateRecurringEvents_ShouldReturnBadRequest_WhenNoEventsCreated()
	{
		using var db = CreateDbContext(nameof(CreateRecurringEvents_ShouldReturnBadRequest_WhenNoEventsCreated));
		var schedulingMock = new Mock<ISchedulingService>();

		schedulingMock
			.Setup(x => x.GenerateRecurringEventsAsync(It.IsAny<RecurringEventCreateDto>(), It.IsAny<string>()))
			.ReturnsAsync(new List<TimetableEvent>());

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.CreateRecurringEvents(new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			FirstOccurrenceEndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1
		});

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task CreateRecurringEvents_ShouldReturnOk_WhenEventsCreated()
	{
		using var db = CreateDbContext(nameof(CreateRecurringEvents_ShouldReturnOk_WhenEventsCreated));
		var schedulingMock = new Mock<ISchedulingService>();

		var generatedEvents = new List<TimetableEvent>
		{
			CreateTimetableEvent(1),
			CreateTimetableEvent(2)
		};

		schedulingMock
			.Setup(x => x.GenerateRecurringEventsAsync(It.IsAny<RecurringEventCreateDto>(), It.IsAny<string>()))
			.ReturnsAsync(generatedEvents);

		var controller = CreateController(db, schedulingMock: schedulingMock);

		var result = await controller.CreateRecurringEvents(new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			FirstOccurrenceEndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1,
			CohortIds = new List<int> { 10, 20 },
			LecturerIds = new List<int> { 30 }
		});

		result.Should().BeOfType<OkObjectResult>();
		db.TimetableEvents.Count().Should().Be(2);
		db.EventCohorts.Count().Should().Be(4);
		db.EventLecturers.Count().Should().Be(2);
	}

	[Fact]
	public async Task DecideRequest_ShouldReturnOk_WhenDecisionSucceeds()
	{
		using var db = CreateDbContext(nameof(DecideRequest_ShouldReturnOk_WhenDecisionSucceeds));
		var requestMock = new Mock<IRequestService>();

		requestMock
			.Setup(x => x.DecideAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new DecisionResultDto
			{
				IsSuccess = true,
				Message = "Request approved successfully."
			});

		var controller = CreateController(db, requestMock: requestMock);

		var result = await controller.DecideRequest(10, new DecideRequestDto
		{
			Decision = "Approved",
			Comment = "Looks good"
		});

		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task DecideRequest_ShouldReturnConflict_WhenDecisionResultIsUnsuccessful()
	{
		using var db = CreateDbContext(nameof(DecideRequest_ShouldReturnConflict_WhenDecisionResultIsUnsuccessful));
		var requestMock = new Mock<IRequestService>();

		requestMock
			.Setup(x => x.DecideAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(new DecisionResultDto
			{
				IsSuccess = false,
				Message = "Request has already been approved."
			});

		var controller = CreateController(db, requestMock: requestMock);

		var result = await controller.DecideRequest(10, new DecideRequestDto
		{
			Decision = "Approved"
		});

		result.Should().BeOfType<ConflictObjectResult>();
	}

	[Fact]
	public async Task DecideRequest_ShouldReturnBadRequest_WhenRequestServiceThrowsArgumentException()
	{
		using var db = CreateDbContext(nameof(DecideRequest_ShouldReturnBadRequest_WhenRequestServiceThrowsArgumentException));
		var requestMock = new Mock<IRequestService>();

		requestMock
			.Setup(x => x.DecideAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
			.ThrowsAsync(new ArgumentException("Decision must be 'Approved' or 'Rejected'."));

		var controller = CreateController(db, requestMock: requestMock);

		var result = await controller.DecideRequest(10, new DecideRequestDto
		{
			Decision = "Invalid"
		});

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task DecideRequest_ShouldReturnConflict_WhenRequestServiceThrowsInvalidOperationException()
	{
		using var db = CreateDbContext(nameof(DecideRequest_ShouldReturnConflict_WhenRequestServiceThrowsInvalidOperationException));
		var requestMock = new Mock<IRequestService>();

		requestMock
			.Setup(x => x.DecideAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
			.ThrowsAsync(new InvalidOperationException("Approved schedule change cannot be applied because it creates a clash."));

		var controller = CreateController(db, requestMock: requestMock);

		var result = await controller.DecideRequest(10, new DecideRequestDto
		{
			Decision = "Approved"
		});

		result.Should().BeOfType<ConflictObjectResult>();
	}

	[Fact]
	public async Task RescheduleEvent_ShouldReturnBadRequest_WhenEndIsBeforeOrEqualToStart()
	{
		using var db = CreateDbContext(nameof(RescheduleEvent_ShouldReturnBadRequest_WhenEndIsBeforeOrEqualToStart));
		var controller = CreateController(db);

		var start = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

		var result = await controller.RescheduleEvent(1, new AdminEventRescheduleDto
		{
			RoomId = 2,
			StartUtc = start,
			EndUtc = start,
			Reason = "Move class"
		});

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task RescheduleEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
	{
		using var db = CreateDbContext(nameof(RescheduleEvent_ShouldReturnNotFound_WhenEventDoesNotExist));
		var controller = CreateController(db);

		var result = await controller.RescheduleEvent(999, new AdminEventRescheduleDto
		{
			RoomId = 2,
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			Reason = "Move class"
		});

		result.Should().BeOfType<NotFoundObjectResult>();
	}

	[Fact]
	public async Task CancelEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
	{
		using var db = CreateDbContext(nameof(CancelEvent_ShouldReturnNotFound_WhenEventDoesNotExist));
		var controller = CreateController(db);

		var result = await controller.CancelEvent(999, new AdminCancelEventDto
		{
			Reason = "Cancelled"
		});

		result.Should().BeOfType<NotFoundObjectResult>();
	}
}