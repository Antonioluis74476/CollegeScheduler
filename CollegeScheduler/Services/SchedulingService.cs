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
				e.EventStatus.Name != "Cancelled" &&
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
					ec.TimetableEvent.EventStatus.Name != "Cancelled" &&
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
					el.TimetableEvent.EventStatus.Name != "Cancelled" &&
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
				e.EndUtc > query.StartUtc &&
				e.EventStatus.Name != "Cancelled")
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

    public async Task<List<RecurringAvailableRoomDto>> FindRecurringAvailableRoomsAsync(
    RecurringRoomSearchQuery query)
    {
        if (query.DurationMinutes <= 0)
            throw new ArgumentException("DurationMinutes must be greater than zero.");

        if (query.NumberOfWeeks <= 0)
            throw new ArgumentException("NumberOfWeeks must be greater than zero.");

        if (query.NumberOfWeeks > 52)
            throw new ArgumentException("NumberOfWeeks cannot be greater than 52.");

        if (query.MinimumCapacity < 0)
            throw new ArgumentException("MinimumCapacity cannot be negative.");

        var occurrences = new List<(DateTime StartUtc, DateTime EndUtc)>();

        var selectedDays = query.DaysOfWeek.Any()
			? query.DaysOfWeek.Distinct().OrderBy(d => d).ToList()
			: new List<DayOfWeek> { query.StartDate.DayOfWeek };

        foreach (var day in selectedDays)
        {
            var firstOccurrence = query.StartDate;

            while (firstOccurrence.DayOfWeek != day)
            {
                firstOccurrence = firstOccurrence.AddDays(1);
            }

            for (var week = 0; week < query.NumberOfWeeks; week++)
            {
                var occurrenceDate = firstOccurrence.AddDays(week * 7);

                var localStart = DateTime.SpecifyKind(
                    occurrenceDate.ToDateTime(query.StartTime),
                    DateTimeKind.Local);

                var startUtc = localStart.ToUniversalTime();

                var endUtc = startUtc.AddMinutes(query.DurationMinutes);

                occurrences.Add((startUtc, endUtc));
            }
        }

        occurrences = occurrences
            .OrderBy(o => o.StartUtc)
            .Distinct()
            .ToList();

        var roomsQuery = _db.Rooms
            .AsNoTracking()
            .Where(r =>
                r.IsActive &&
                r.Capacity >= query.MinimumCapacity);

        if (query.RoomTypeId.HasValue)
        {
            roomsQuery = roomsQuery.Where(r =>
                r.RoomTypeId == query.RoomTypeId.Value);
        }

        if (query.BuildingId.HasValue)
        {
            roomsQuery = roomsQuery.Where(r =>
                r.BuildingId == query.BuildingId.Value);
        }

        if (query.CampusId.HasValue)
        {
            roomsQuery = roomsQuery.Where(r =>
                r.Building!.CampusId == query.CampusId.Value);
        }

        if (query.RequiredFeatureIds is { Count: > 0 })
        {
            foreach (var featureId in query.RequiredFeatureIds.Distinct())
            {
                roomsQuery = roomsQuery.Where(r =>
                    r.RoomFeatures.Any(rf =>
                        rf.FeatureId == featureId));
            }
        }

        var candidateRooms = await roomsQuery
            .Select(r => new
            {
                r.RoomId,
                RoomCode = r.Code,
                RoomName = r.Name,
                r.Capacity,
                BuildingName = r.Building!.Name,
                CampusName = r.Building.Campus!.Name,
                RoomTypeName = r.RoomType!.Name,

                Features = r.RoomFeatures
                    .Select(rf => rf.Feature!.Name)
                    .ToList()
            })
            .ToListAsync();

        if (candidateRooms.Count == 0)
            return new List<RecurringAvailableRoomDto>();

        var candidateRoomIds = candidateRooms
            .Select(r => r.RoomId)
            .ToList();

        var searchStartUtc = occurrences.Min(o => o.StartUtc);
        var searchEndUtc = occurrences.Max(o => o.EndUtc);

        var bookings = await _db.TimetableEvents
            .AsNoTracking()
            .Where(e =>
                candidateRoomIds.Contains(e.RoomId) &&
                e.StartUtc < searchEndUtc &&
                e.EndUtc > searchStartUtc &&
                e.EventStatus.Name != "Cancelled")
            .Select(e => new
            {
                e.RoomId,
                e.StartUtc,
                e.EndUtc
            })
            .ToListAsync();

        var roomUnavailabilities = await _db.RoomUnavailabilities
            .AsNoTracking()
            .Where(u =>
                candidateRoomIds.Contains(u.RoomId) &&
                u.StartUtc < searchEndUtc &&
                u.EndUtc > searchStartUtc)
            .Select(u => new
            {
                u.RoomId,
                u.StartUtc,
                u.EndUtc
            })
            .ToListAsync();

        var results = new List<RecurringAvailableRoomDto>();

        foreach (var room in candidateRooms)
        {
            var unavailableOccurrences = new List<UnavailableOccurrenceDto>();

            foreach (var occurrence in occurrences)
            {
                var hasBookingClash = bookings.Any(b =>
                    b.RoomId == room.RoomId &&
                    b.StartUtc < occurrence.EndUtc &&
                    b.EndUtc > occurrence.StartUtc);

                var hasUnavailabilityClash = roomUnavailabilities.Any(u =>
                    u.RoomId == room.RoomId &&
                    u.StartUtc < occurrence.EndUtc &&
                    u.EndUtc > occurrence.StartUtc);

                if (hasBookingClash || hasUnavailabilityClash)
                {
                    unavailableOccurrences.Add(new UnavailableOccurrenceDto
                    {
                        StartUtc = occurrence.StartUtc,
                        EndUtc = occurrence.EndUtc,
                        Reason = hasBookingClash
                            ? "Already booked"
                            : "Room unavailable"
                    });
                }
            }

            var availableOccurrences =
                occurrences.Count - unavailableOccurrences.Count;

            results.Add(new RecurringAvailableRoomDto
            {
                RoomId = room.RoomId,
                RoomCode = room.RoomCode,
                RoomName = room.RoomName,
                BuildingName = room.BuildingName,
                CampusName = room.CampusName,
                Capacity = room.Capacity,
                RoomType = room.RoomTypeName,
                Features = room.Features,
                AvailableOccurrences = availableOccurrences,
                TotalOccurrences = occurrences.Count,
                UnavailableOccurrences = unavailableOccurrences
            });
        }

        _logger.LogInformation(
            "Recurring room search completed. Start={StartDate}, Weeks={NumberOfWeeks}, DurationMinutes={DurationMinutes}, Candidates={CandidateCount}, Available={AvailableCount}",
            query.StartDate,
            query.NumberOfWeeks,
            query.DurationMinutes,
            candidateRooms.Count,
            results.Count);

        return results
            .OrderBy(r => r.Capacity)
            .ThenBy(r => r.RoomCode)
            .ToList();
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
	public async Task<List<TimetableEvent>> GetRecurringEventSeriesAsync(Guid recurrenceGroupId)
	{
		return await _db.TimetableEvents
			.AsNoTracking()
			.Where(te => te.RecurrenceGroupId == recurrenceGroupId)
			.OrderBy(te => te.StartUtc)
			.ToListAsync();
	}

	public async Task<RecurringEventUpdateResultDto> UpdateRecurringEventsAsync(
		Guid recurrenceGroupId, UpdateRecurringEventDto dto, string updatedByUserId)
	{
		var events = await _db.TimetableEvents
			.Include(te => te.EventCohorts)
			.Include(te => te.EventLecturers)
			.Where(te => te.RecurrenceGroupId == recurrenceGroupId)
			.OrderBy(te => te.StartUtc)
			.ToListAsync();

		if (events.Count == 0)
			throw new ArgumentException($"No recurring events found for group {recurrenceGroupId}.");

		List<TimetableEvent> targetEvents = dto.Scope switch
		{
			RecurringEventUpdateScope.All => events,

			RecurringEventUpdateScope.ThisOnly =>
				events.Where(e => e.TimetableEventId == dto.AnchorEventId).ToList(),

			RecurringEventUpdateScope.ThisAndFollowing =>
				events.Where(e => e.StartUtc >= (events.FirstOrDefault(a => a.TimetableEventId == dto.AnchorEventId)
					?? throw new ArgumentException($"AnchorEventId {dto.AnchorEventId} not found in this recurrence group.")).StartUtc)
					.ToList(),

			_ => throw new ArgumentException("Unknown scope.")
		};

		if (targetEvents.Count == 0)
			throw new ArgumentException($"AnchorEventId {dto.AnchorEventId} not found in this recurrence group.");

		var proposedTimes = new Dictionary<long, (DateTime NewStart, DateTime NewEnd)>();
		foreach (var ev in targetEvents)
		{
			var newStart = ev.StartUtc;
			var newEnd = ev.EndUtc;

			if (dto.NewStartTime.HasValue && dto.NewEndTime.HasValue)
			{
				var date = DateOnly.FromDateTime(ev.StartUtc);
				newStart = date.ToDateTime(TimeOnly.FromTimeSpan(dto.NewStartTime.Value));
				newEnd = date.ToDateTime(TimeOnly.FromTimeSpan(dto.NewEndTime.Value));
			}

			proposedTimes[ev.TimetableEventId] = (newStart, newEnd);
		}

		var clashes = new List<OccurrenceClashDto>();
		foreach (var ev in targetEvents)
		{
			var (newStart, newEnd) = proposedTimes[ev.TimetableEventId];
			var cohortIdsForCheck = dto.CohortIds ?? ev.EventCohorts.Select(ec => ec.CohortId).ToList();
			var lecturerIdsForCheck = dto.LecturerIds ?? ev.EventLecturers.Select(el => el.LecturerId).ToList();

			var clashResult = await CheckClashesAsync(
				excludeEventId: ev.TimetableEventId,
				roomId: dto.RoomId ?? ev.RoomId,
				startUtc: newStart,
				endUtc: newEnd,
				cohortIds: cohortIdsForCheck,
				lecturerIds: lecturerIdsForCheck);

			if (clashResult.HasClash)
				clashes.Add(new OccurrenceClashDto { TimetableEventId = ev.TimetableEventId, Clash = clashResult });
		}

		if (clashes.Count > 0)
		{
			return new RecurringEventUpdateResultDto
			{
				Success = false,
				UpdatedCount = 0,
				EventIds = new List<long>(),
				Clashes = clashes
			};
		}

		foreach (var ev in targetEvents)
		{
			var (newStart, newEnd) = proposedTimes[ev.TimetableEventId];
			var oldRoomId = ev.RoomId;
			var oldStartUtc = ev.StartUtc;
			var oldEndUtc = ev.EndUtc;

			if (dto.RoomId.HasValue) ev.RoomId = dto.RoomId.Value;
			ev.StartUtc = newStart;
			ev.EndUtc = newEnd;
			if (!string.IsNullOrWhiteSpace(dto.SessionType)) ev.SessionType = dto.SessionType;
			if (dto.Notes is not null) ev.Notes = dto.Notes;
			ev.UpdatedAtUtc = DateTime.UtcNow;

			if (dto.CohortIds is not null)
			{
				_db.EventCohorts.RemoveRange(ev.EventCohorts);
				foreach (var cohortId in dto.CohortIds.Distinct())
					_db.EventCohorts.Add(new EventCohort { TimetableEventId = ev.TimetableEventId, CohortId = cohortId });
			}

			if (dto.LecturerIds is not null)
			{
				_db.EventLecturers.RemoveRange(ev.EventLecturers);
				foreach (var lecturerId in dto.LecturerIds.Distinct())
					_db.EventLecturers.Add(new EventLecturer { TimetableEventId = ev.TimetableEventId, LecturerId = lecturerId });
			}

			_db.TimetableEventChanges.Add(new TimetableEventChange
			{
				TimetableEventId = ev.TimetableEventId,
				ChangeType = "RecurringUpdate",
				OldRoomId = oldRoomId,
				NewRoomId = ev.RoomId,
				OldStartUtc = oldStartUtc,
				NewStartUtc = ev.StartUtc,
				OldEndUtc = oldEndUtc,
				NewEndUtc = ev.EndUtc,
				Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Recurring series updated by admin." : dto.Reason,
				ChangedByUserId = updatedByUserId,
				ChangedAtUtc = DateTime.UtcNow,
				NotificationSent = false
			});
		}

		await _db.SaveChangesAsync();

		return new RecurringEventUpdateResultDto
		{
			Success = true,
			UpdatedCount = targetEvents.Count,
			EventIds = targetEvents.Select(e => e.TimetableEventId).ToList()
		};
	}

	public async Task<RecurringEventUpdateResultDto> CancelRecurringEventsAsync(
		Guid recurrenceGroupId, CancelRecurringEventDto dto, string cancelledByUserId)
	{
		var events = await _db.TimetableEvents
			.Where(te => te.RecurrenceGroupId == recurrenceGroupId)
			.OrderBy(te => te.StartUtc)
			.ToListAsync();

		if (events.Count == 0)
			throw new ArgumentException($"No recurring events found for group {recurrenceGroupId}.");

		List<TimetableEvent> targetEvents = dto.Scope switch
		{
			RecurringEventUpdateScope.All => events,

			RecurringEventUpdateScope.ThisOnly =>
				events.Where(e => e.TimetableEventId == dto.AnchorEventId).ToList(),

			RecurringEventUpdateScope.ThisAndFollowing =>
				events.Where(e => e.StartUtc >= (events.FirstOrDefault(a => a.TimetableEventId == dto.AnchorEventId)
					?? throw new ArgumentException($"AnchorEventId {dto.AnchorEventId} not found in this recurrence group.")).StartUtc)
					.ToList(),

			_ => throw new ArgumentException("Unknown scope.")
		};

		if (targetEvents.Count == 0)
			throw new ArgumentException($"AnchorEventId {dto.AnchorEventId} not found in this recurrence group.");

		var cancelledStatusId = await _db.EventStatuses
			.Where(x => x.Name == "Cancelled")
			.Select(x => x.EventStatusId)
			.FirstOrDefaultAsync();

		if (cancelledStatusId == 0)
			throw new InvalidOperationException("EventStatus 'Cancelled' not found.");

		foreach (var ev in targetEvents)
		{
			ev.EventStatusId = cancelledStatusId;
			ev.UpdatedAtUtc = DateTime.UtcNow;

			_db.TimetableEventChanges.Add(new TimetableEventChange
			{
				TimetableEventId = ev.TimetableEventId,
				ChangeType = "RecurringCancel",
				Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Recurring series cancelled by admin." : dto.Reason,
				ChangedByUserId = cancelledByUserId,
				ChangedAtUtc = DateTime.UtcNow,
				NotificationSent = false
			});
		}

		await _db.SaveChangesAsync();

		return new RecurringEventUpdateResultDto
		{
			Success = true,
			UpdatedCount = targetEvents.Count,
			EventIds = targetEvents.Select(e => e.TimetableEventId).ToList()
		};
	}

}

