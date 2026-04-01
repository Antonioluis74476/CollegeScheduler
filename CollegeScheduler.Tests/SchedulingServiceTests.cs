using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Services;

public class SchedulingServiceTests
{
	private static ApplicationDbContext CreateDbContext(string dbName)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private static SchedulingService CreateService(ApplicationDbContext db)
	{
		var logger = new Mock<ILogger<SchedulingService>>();
		return new SchedulingService(db, logger.Object);
	}

	private static TimetableEvent CreateTimetableEvent(
		long id,
		int roomId,
		DateTime startUtc,
		DateTime endUtc)
	{
		return new TimetableEvent
		{
			TimetableEventId = id,
			TermId = 1,
			ModuleId = 1,
			RoomId = roomId,
			StartUtc = startUtc,
			EndUtc = endUtc,
			EventStatusId = 1,
			SessionType = "Lecture",
			CreatedByUserId = "test-user"
		};
	}

	private static Campus CreateCampus(int id = 1, string code = "C1", string name = "Campus 1")
	{
		return new Campus
		{
			CampusId = id,
			Code = code,
			Name = name
		};
	}

	private static Building CreateBuilding(int id = 1, int campusId = 1, string code = "B1", string name = "Building 1")
	{
		return new Building
		{
			BuildingId = id,
			CampusId = campusId,
			Code = code,
			Name = name
		};
	}

	private static Room CreateRoom(
		int id,
		int buildingId,
		int roomTypeId,
		string code,
		int capacity,
		Building? building = null)
	{
		return new Room
		{
			RoomId = id,
			BuildingId = buildingId,
			Building = building,
			RoomTypeId = roomTypeId,
			Code = code,
			Name = code,
			Capacity = capacity
		};
	}

	private static Term CreateTerm(
		int id,
		DateTime startDate,
		DateTime endDate)
	{
		return new Term
		{
			TermId = id,
			AcademicYearId = 1,
			TermNumber = 1,
			Name = "Autumn",
			StartDate = startDate,
			EndDate = endDate
		};
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldThrow_WhenEndIsBeforeOrEqualToStart()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldThrow_WhenEndIsBeforeOrEqualToStart));
		var service = CreateService(db);

		var start = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
		var end = start;

		Func<Task> act = async () => await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 1,
			startUtc: start,
			endUtc: end,
			cohortIds: new List<int>(),
			lecturerIds: new List<int>());

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("EndUtc must be greater than StartUtc.");
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldReturnNoClash_WhenNothingConflicts()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldReturnNoClash_WhenNothingConflicts));
		var service = CreateService(db);

		var start = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
		var end = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);

		var result = await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 1,
			startUtc: start,
			endUtc: end,
			cohortIds: new List<int> { 100 },
			lecturerIds: new List<int> { 200 });

		result.Should().NotBeNull();
		result.HasClash.Should().BeFalse();
		result.RoomClash.Should().BeNull();
		result.CohortClashes.Should().BeEmpty();
		result.LecturerClashes.Should().BeEmpty();
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldDetectRoomClash_WhenExistingEventOverlaps()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldDetectRoomClash_WhenExistingEventOverlaps));

		db.TimetableEvents.Add(CreateTimetableEvent(
			id: 1,
			roomId: 10,
			startUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 10,
			startUtc: new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 1, 13, 0, 0, DateTimeKind.Utc),
			cohortIds: new List<int>(),
			lecturerIds: new List<int>());

		result.HasClash.Should().BeTrue();
		result.RoomClash.Should().NotBeNull();
		result.RoomClash!.ConflictingEventId.Should().Be(1);
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldDetectCohortClash_WhenCohortAlreadyHasOverlappingEvent()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldDetectCohortClash_WhenCohortAlreadyHasOverlappingEvent));

		db.TimetableEvents.Add(CreateTimetableEvent(
			id: 2,
			roomId: 20,
			startUtc: new DateTime(2026, 4, 2, 9, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc)));

		db.EventCohorts.Add(new EventCohort
		{
			TimetableEventId = 2,
			CohortId = 300
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 999,
			startUtc: new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
			cohortIds: new List<int> { 300 },
			lecturerIds: new List<int>());

		result.HasClash.Should().BeTrue();
		result.CohortClashes.Should().HaveCount(1);
		result.CohortClashes[0].ConflictingEventId.Should().Be(2);
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldDetectLecturerClash_WhenLecturerAlreadyHasOverlappingEvent()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldDetectLecturerClash_WhenLecturerAlreadyHasOverlappingEvent));

		db.TimetableEvents.Add(CreateTimetableEvent(
			id: 3,
			roomId: 30,
			startUtc: new DateTime(2026, 4, 3, 14, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 3, 16, 0, 0, DateTimeKind.Utc)));

		db.EventLecturers.Add(new EventLecturer
		{
			TimetableEventId = 3,
			LecturerId = 400
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 888,
			startUtc: new DateTime(2026, 4, 3, 15, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 3, 17, 0, 0, DateTimeKind.Utc),
			cohortIds: new List<int>(),
			lecturerIds: new List<int> { 400 });

		result.HasClash.Should().BeTrue();
		result.LecturerClashes.Should().HaveCount(1);
		result.LecturerClashes[0].ConflictingEventId.Should().Be(3);
	}

	[Fact]
	public async Task CheckClashesAsync_ShouldDetectRoomUnavailability_WhenRoomIsBlocked()
	{
		using var db = CreateDbContext(nameof(CheckClashesAsync_ShouldDetectRoomUnavailability_WhenRoomIsBlocked));

		db.RoomUnavailabilities.Add(new RoomUnavailability
		{
			RoomUnavailabilityId = 50,
			RoomId = 7,
			StartUtc = new DateTime(2026, 4, 4, 8, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 4, 12, 0, 0, DateTimeKind.Utc),
			UnavailabilityReasonTypeId = 1,
			CreatedByUserId = "test-user"
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var result = await service.CheckClashesAsync(
			excludeEventId: null,
			roomId: 7,
			startUtc: new DateTime(2026, 4, 4, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 4, 11, 0, 0, DateTimeKind.Utc),
			cohortIds: new List<int>(),
			lecturerIds: new List<int>());

		result.HasClash.Should().BeTrue();
		result.RoomClash.Should().NotBeNull();
		result.RoomClash!.ConflictingEventId.Should().Be(50);
	}

	[Fact]
	public async Task FindAvailableRoomsAsync_ShouldThrow_WhenEndEqualsStart()
	{
		using var db = CreateDbContext(nameof(FindAvailableRoomsAsync_ShouldThrow_WhenEndEqualsStart));
		var service = CreateService(db);

		var start = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

		var query = new RoomSearchQuery
		{
			StartUtc = start,
			EndUtc = start
		};

		Func<Task> act = async () => await service.FindAvailableRoomsAsync(query);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("EndUtc must be greater than StartUtc.");
	}

	[Fact]
	public async Task FindAvailableRoomsAsync_ShouldReturnAvailableRooms_WhenNoConflicts()
	{
		using var db = CreateDbContext(nameof(FindAvailableRoomsAsync_ShouldReturnAvailableRooms_WhenNoConflicts));

		var campus = CreateCampus();
		var building = CreateBuilding(campusId: campus.CampusId);
		building.Campus = campus;

		db.Rooms.Add(CreateRoom(
			id: 1,
			buildingId: building.BuildingId,
			roomTypeId: 1,
			code: "R1",
			capacity: 50,
			building: building));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var query = new RoomSearchQuery
		{
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
		};

		var result = await service.FindAvailableRoomsAsync(query);

		result.Should().HaveCount(1);
		result[0].Code.Should().Be("R1");
	}

	[Fact]
	public async Task FindAvailableRoomsAsync_ShouldExcludeBookedRooms()
	{
		using var db = CreateDbContext(nameof(FindAvailableRoomsAsync_ShouldExcludeBookedRooms));

		var campus = CreateCampus();
		var building = CreateBuilding(campusId: campus.CampusId);
		building.Campus = campus;

		db.Rooms.Add(CreateRoom(
			id: 1,
			buildingId: building.BuildingId,
			roomTypeId: 1,
			code: "R1",
			capacity: 50,
			building: building));

		db.TimetableEvents.Add(CreateTimetableEvent(
			id: 1,
			roomId: 1,
			startUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var query = new RoomSearchQuery
		{
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
		};

		var result = await service.FindAvailableRoomsAsync(query);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindAvailableRoomsAsync_ShouldExcludeUnavailableRooms()
	{
		using var db = CreateDbContext(nameof(FindAvailableRoomsAsync_ShouldExcludeUnavailableRooms));

		var campus = CreateCampus();
		var building = CreateBuilding(campusId: campus.CampusId);
		building.Campus = campus;

		db.Rooms.Add(CreateRoom(
			id: 1,
			buildingId: building.BuildingId,
			roomTypeId: 1,
			code: "R1",
			capacity: 50,
			building: building));

		db.RoomUnavailabilities.Add(new RoomUnavailability
		{
			RoomUnavailabilityId = 1,
			RoomId = 1,
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
			UnavailabilityReasonTypeId = 1,
			CreatedByUserId = "test-user"
		});

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var query = new RoomSearchQuery
		{
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
		};

		var result = await service.FindAvailableRoomsAsync(query);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindAvailableRoomsAsync_ShouldFilterByCapacity()
	{
		using var db = CreateDbContext(nameof(FindAvailableRoomsAsync_ShouldFilterByCapacity));

		var campus = CreateCampus();
		var building = CreateBuilding(campusId: campus.CampusId);
		building.Campus = campus;

		db.Rooms.AddRange(
			CreateRoom(
				id: 1,
				buildingId: building.BuildingId,
				roomTypeId: 1,
				code: "Small",
				capacity: 10,
				building: building),
			CreateRoom(
				id: 2,
				buildingId: building.BuildingId,
				roomTypeId: 1,
				code: "Big",
				capacity: 100,
				building: building));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var query = new RoomSearchQuery
		{
			StartUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
			EndUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
			MinCapacity = 50
		};

		var result = await service.FindAvailableRoomsAsync(query);

		result.Should().HaveCount(1);
		result[0].Code.Should().Be("Big");
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldThrow_WhenTermNotFound()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldThrow_WhenTermNotFound));
		var service = CreateService(db);

		var dto = new RecurringEventCreateDto
		{
			TermId = 999,
			RoomId = 1,
			ModuleId = 1,
			FirstOccurrenceStartUtc = DateTime.UtcNow,
			FirstOccurrenceEndUtc = DateTime.UtcNow.AddHours(1),
			EventStatusId = 1
		};

		Func<Task> act = async () => await service.GenerateRecurringEventsAsync(dto, "user");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Term 999 not found.");
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldThrow_WhenEndBeforeStart()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldThrow_WhenEndBeforeStart));

		db.Terms.Add(CreateTerm(
			id: 1,
			startDate: new DateTime(2026, 1, 1),
			endDate: new DateTime(2026, 2, 28)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var start = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc);

		var dto = new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = start,
			FirstOccurrenceEndUtc = start,
			EventStatusId = 1
		};

		Func<Task> act = async () => await service.GenerateRecurringEventsAsync(dto, "user");

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("FirstOccurrenceEndUtc must be greater than FirstOccurrenceStartUtc.");
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldCreateWeeklyEvents_UntilTermEnd()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldCreateWeeklyEvents_UntilTermEnd));

		db.Terms.Add(CreateTerm(
			id: 1,
			startDate: new DateTime(2026, 1, 1),
			endDate: new DateTime(2026, 1, 26)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var dto = new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
			FirstOccurrenceEndUtc = new DateTime(2026, 1, 5, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1,
			SessionType = "Lecture",
			CohortIds = new List<int>(),
			LecturerIds = new List<int>()
		};

		var result = await service.GenerateRecurringEventsAsync(dto, "creator-1");

		result.Should().HaveCount(4);
		result.All(e => e.CreatedByUserId == "creator-1").Should().BeTrue();
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldSkipExcludedDates()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldSkipExcludedDates));

		db.Terms.Add(CreateTerm(
			id: 1,
			startDate: new DateTime(2026, 1, 1),
			endDate: new DateTime(2026, 1, 26)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var firstStart = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc);

		var dto = new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = firstStart,
			FirstOccurrenceEndUtc = firstStart.AddHours(1),
			EventStatusId = 1,
			ExcludeDates = new HashSet<DateOnly>
			{
				DateOnly.FromDateTime(firstStart.AddDays(7))
			}
		};

		var result = await service.GenerateRecurringEventsAsync(dto, "creator-1");

		result.Should().HaveCount(3);
		result.Any(e => e.StartUtc.Date == firstStart.AddDays(7).Date).Should().BeFalse();
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldSkipOccurrencesWithClashes()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldSkipOccurrencesWithClashes));

		db.Terms.Add(CreateTerm(
			id: 1,
			startDate: new DateTime(2026, 1, 1),
			endDate: new DateTime(2026, 1, 26)));

		db.TimetableEvents.Add(CreateTimetableEvent(
			id: 99,
			roomId: 1,
			startUtc: new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc),
			endUtc: new DateTime(2026, 1, 12, 11, 0, 0, DateTimeKind.Utc)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var dto = new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
			FirstOccurrenceEndUtc = new DateTime(2026, 1, 5, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1
		};

		var result = await service.GenerateRecurringEventsAsync(dto, "creator-1");

		result.Should().HaveCount(3);
		result.Any(e => e.StartUtc == new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc)).Should().BeFalse();
	}

	[Fact]
	public async Task GenerateRecurringEventsAsync_ShouldAssignSameRecurrenceGroupId_ToAllEvents()
	{
		using var db = CreateDbContext(nameof(GenerateRecurringEventsAsync_ShouldAssignSameRecurrenceGroupId_ToAllEvents));

		db.Terms.Add(CreateTerm(
			id: 1,
			startDate: new DateTime(2026, 1, 1),
			endDate: new DateTime(2026, 1, 26)));

		await db.SaveChangesAsync();

		var service = CreateService(db);

		var dto = new RecurringEventCreateDto
		{
			TermId = 1,
			ModuleId = 1,
			RoomId = 1,
			FirstOccurrenceStartUtc = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
			FirstOccurrenceEndUtc = new DateTime(2026, 1, 5, 11, 0, 0, DateTimeKind.Utc),
			EventStatusId = 1
		};

		var result = await service.GenerateRecurringEventsAsync(dto, "creator-1");

		result.Should().NotBeEmpty();
		result.Select(e => e.RecurrenceGroupId).Distinct().Should().HaveCount(1);
		result.All(e => e.RecurrenceGroupId.HasValue).Should().BeTrue();
	}
}