using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Services;

/// <summary>
/// Core scheduling logic:
/// - room clash detection
/// - cohort clash detection
/// - lecturer clash detection
/// - available room search
/// - recurring weekly event generation
/// </summary>
public sealed class SchedulingService : ISchedulingService
{
	private readonly ApplicationDbContext _db;
	private readonly ILogger<SchedulingService> _logger;

	public SchedulingService(ApplicationDbContext db, ILogger<SchedulingService> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task<ClashResult> CheckClashesAsync(
		long? excludeEventId,
		int roomId,
		DateTime startUtc,
		DateTime endUtc,
		IEnumerable<int> cohortIds,
		IEnumerable<int> lecturerIds)
	{
		if (endUtc <= startUtc)
			throw new ArgumentException("EndUtc must be greater than StartUtc.");

		var result = new ClashResult();

		// Room clash
		var roomClash = await _db.TimetableEvents
			.AsNoTracking()
			.Where(e =>
				e.RoomId == roomId &&
				e.StartUtc < endUtc &&
				e.EndUtc > startUtc &&
				(!excludeEventId.HasValue || e.TimetableEventId != excludeEventId.Value))
			.Select(e => new
			{
				e.TimetableEventId,
				e.StartUtc,
				e.EndUtc
			})
			.FirstOrDefaultAsync();

		if (roomClash is not null)
		{
			result.RoomClash = new ClashDetail(
				roomClash.TimetableEventId,
				$"Room already booked from {roomClash.StartUtc:yyyy-MM-dd HH:mm} to {roomClash.EndUtc:yyyy-MM-dd HH:mm} UTC.");
		}

		// Room unavailability clash
		var roomUnavailable = await _db.RoomUnavailabilities
			.AsNoTracking()
			.Where(u =>
				u.RoomId == roomId &&
				u.StartUtc < endUtc &&
				u.EndUtc > startUtc)
			.Select(u => new
			{
				u.RoomUnavailabilityId,
				u.StartUtc,
				u.EndUtc
			})
			.FirstOrDefaultAsync();

		if (roomUnavailable is not null && result.RoomClash is null)
		{
			result.RoomClash = new ClashDetail(
				roomUnavailable.RoomUnavailabilityId,
				$"Room unavailable from {roomUnavailable.StartUtc:yyyy-MM-dd HH:mm} to {roomUnavailable.EndUtc:yyyy-MM-dd HH:mm} UTC.");
		}

		// Cohort clashes
		var cohortIdList = cohortIds.Distinct().ToList();
		if (cohortIdList.Count > 0)
		{
			var cohortClashes = await _db.EventCohorts
				.AsNoTracking()
				.Where(ec =>
					cohortIdList.Contains(ec.CohortId) &&
					ec.TimetableEvent.StartUtc < endUtc &&
					ec.TimetableEvent.EndUtc > startUtc &&
					(!excludeEventId.HasValue || ec.TimetableEventId != excludeEventId.Value))
				.Select(ec => new
				{
					ec.CohortId,
					ec.TimetableEventId,
					ec.TimetableEvent.StartUtc,
					ec.TimetableEvent.EndUtc
				})
				.ToListAsync();

			foreach (var clash in cohortClashes)
			{
				result.CohortClashes.Add(new ClashDetail(
					clash.TimetableEventId,
					$"Cohort {clash.CohortId} already has an event from {clash.StartUtc:yyyy-MM-dd HH:mm} to {clash.EndUtc:yyyy-MM-dd HH:mm} UTC."));
			}
		}

		// Lecturer clashes
		var lecturerIdList = lecturerIds.Distinct().ToList();
		if (lecturerIdList.Count > 0)
		{
			var lecturerClashes = await _db.EventLecturers
				.AsNoTracking()
				.Where(el =>
					lecturerIdList.Contains(el.LecturerId) &&
					el.TimetableEvent.StartUtc < endUtc &&
					el.TimetableEvent.EndUtc > startUtc &&
					(!excludeEventId.HasValue || el.TimetableEventId != excludeEventId.Value))
				.Select(el => new
				{
					el.LecturerId,
					el.TimetableEventId,
					el.TimetableEvent.StartUtc,
					el.TimetableEvent.EndUtc
				})
				.ToListAsync();

			foreach (var clash in lecturerClashes)
			{
				result.LecturerClashes.Add(new ClashDetail(
					clash.TimetableEventId,
					$"Lecturer {clash.LecturerId} already has an event from {clash.StartUtc:yyyy-MM-dd HH:mm} to {clash.EndUtc:yyyy-MM-dd HH:mm} UTC."));
			}
		}

		if (result.HasClash)
		{
			_logger.LogWarning(
				"Clash detected. RoomId={RoomId}, Start={StartUtc}, End={EndUtc}, RoomClash={HasRoomClash}, CohortClashes={CohortCount}, LecturerClashes={LecturerCount}",
				roomId,
				startUtc,
				endUtc,
				result.RoomClash is not null,
				result.CohortClashes.Count,
				result.LecturerClashes.Count);
		}

		return result;
	}

	public async Task<List<AvailableRoomDto>> FindAvailableRoomsAsync(RoomSearchQuery query)
	{
		if (query.EndUtc <= query.StartUtc)
			throw new ArgumentException("EndUtc must be greater than StartUtc.");

		var roomsQuery = _db.Rooms
			.AsNoTracking()
			.Where(r => r.IsActive);

		if (query.MinCapacity.HasValue)
			roomsQuery = roomsQuery.Where(r => r.Capacity >= query.MinCapacity.Value);

		if (query.RoomTypeId.HasValue)
			roomsQuery = roomsQuery.Where(r => r.RoomTypeId == query.RoomTypeId.Value);

		if (query.BuildingId.HasValue)
			roomsQuery = roomsQuery.Where(r => r.BuildingId == query.BuildingId.Value);

		if (query.CampusId.HasValue)
			roomsQuery = roomsQuery.Where(r => r.Building!.CampusId == query.CampusId.Value);

		if (query.RequiredFeatureIds is { Count: > 0 })
		{
			foreach (var featureId in query.RequiredFeatureIds.Distinct())
			{
				roomsQuery = roomsQuery.Where(r =>
					r.RoomFeatures.Any(rf => rf.FeatureId == featureId));
			}
		}

		var candidateRooms = await roomsQuery
			.Select(r => new
			{
				r.RoomId,
				r.Code,
				r.Name,
				r.Capacity,
				BuildingName = r.Building!.Name,
				CampusName = r.Building!.Campus!.Name
			})
			.ToListAsync();

		var bookedRoomIds = await _db.TimetableEvents
			.AsNoTracking()
			.Where(e =>
				e.StartUtc < query.EndUtc &&
				e.EndUtc > query.StartUtc)
			.Select(e => e.RoomId)
			.Distinct()
			.ToListAsync();

		var unavailableRoomIds = await _db.RoomUnavailabilities
			.AsNoTracking()
			.Where(u =>
				u.StartUtc < query.EndUtc &&
				u.EndUtc > query.StartUtc)
			.Select(u => u.RoomId)
			.Distinct()
			.ToListAsync();

		var excludedRoomIds = bookedRoomIds
			.Union(unavailableRoomIds)
			.ToHashSet();

		var availableRooms = candidateRooms
			.Where(r => !excludedRoomIds.Contains(r.RoomId))
			.OrderBy(r => r.Capacity)
			.ThenBy(r => r.Code)
			.Select(r => new AvailableRoomDto
			{
				RoomId = r.RoomId,
				Code = r.Code,
				Name = r.Name,
				Capacity = r.Capacity,
				BuildingName = r.BuildingName,
				CampusName = r.CampusName
			})
			.ToList();

		_logger.LogInformation(
			"Available room search completed. Start={StartUtc}, End={EndUtc}, Candidates={CandidateCount}, Available={AvailableCount}",
			query.StartUtc,
			query.EndUtc,
			candidateRooms.Count,
			availableRooms.Count);

		return availableRooms;
	}

	public async Task<List<TimetableEvent>> GenerateRecurringEventsAsync(
		RecurringEventCreateDto dto,
		string createdByUserId)
	{
		var term = await _db.Terms.FindAsync(dto.TermId);
		if (term is null)
			throw new ArgumentException($"Term {dto.TermId} not found.");

		if (dto.FirstOccurrenceEndUtc <= dto.FirstOccurrenceStartUtc)
			throw new ArgumentException("FirstOccurrenceEndUtc must be greater than FirstOccurrenceStartUtc.");

		var recurrenceGroupId = Guid.NewGuid();
		var events = new List<TimetableEvent>();

		var duration = dto.FirstOccurrenceEndUtc - dto.FirstOccurrenceStartUtc;
		var currentStart = dto.FirstOccurrenceStartUtc;

		// FIX: compare DateTime to DateTime instead of DateOnly to DateTime
		while (currentStart.Date <= term.EndDate.Date)
		{
			var currentEnd = currentStart.Add(duration);

			if (dto.ExcludeDates?.Contains(DateOnly.FromDateTime(currentStart)) == true)
			{
				currentStart = currentStart.AddDays(7);
				continue;
			}

			var clash = await CheckClashesAsync(
				excludeEventId: null,
				roomId: dto.RoomId,
				startUtc: currentStart,
				endUtc: currentEnd,
				cohortIds: dto.CohortIds,
				lecturerIds: dto.LecturerIds);

			if (!clash.HasClash)
			{
				events.Add(new TimetableEvent
				{
					TermId = dto.TermId,
					ModuleId = dto.ModuleId,
					RoomId = dto.RoomId,
					StartUtc = currentStart,
					EndUtc = currentEnd,
					EventStatusId = dto.EventStatusId,
					SessionType = dto.SessionType,
					RecurrenceGroupId = recurrenceGroupId,
					Notes = dto.Notes,
					CreatedByUserId = createdByUserId
				});
			}
			else
			{
				_logger.LogWarning(
					"Recurring event skipped due to clash. Start={StartUtc}, End={EndUtc}, RoomId={RoomId}",
					currentStart,
					currentEnd,
					dto.RoomId);
			}

			currentStart = currentStart.AddDays(7);
		}

		return events;
	}
}