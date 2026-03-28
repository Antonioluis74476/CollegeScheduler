using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Route("api/v1/admin/scheduling")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminSchedulingController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly ISchedulingService _schedulingService;
	private readonly IRequestService _requestService;

	public AdminSchedulingController(
		ApplicationDbContext db,
		ISchedulingService schedulingService,
		IRequestService requestService)
	{
		_db = db;
		_schedulingService = schedulingService;
		_requestService = requestService;
	}

	private string CurrentUserId =>
		User.FindFirstValue(ClaimTypes.NameIdentifier)
		?? throw new UnauthorizedAccessException("Missing user id claim.");

	[HttpGet("rooms/available")]
	public async Task<IActionResult> FindAvailableRooms(
		[FromQuery] DateTime startUtc,
		[FromQuery] DateTime endUtc,
		[FromQuery] int? minCapacity,
		[FromQuery] int? roomTypeId,
		[FromQuery] int? buildingId,
		[FromQuery] int? campusId,
		[FromQuery] List<int>? featureIds)
	{
		try
		{
			var rooms = await _schedulingService.FindAvailableRoomsAsync(new RoomSearchQuery
			{
				StartUtc = startUtc,
				EndUtc = endUtc,
				MinCapacity = minCapacity,
				RoomTypeId = roomTypeId,
				BuildingId = buildingId,
				CampusId = campusId,
				RequiredFeatureIds = featureIds
			});

			return Ok(rooms);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("check-clashes")]
	public async Task<IActionResult> CheckClashes([FromBody] ClashCheckRequest dto)
	{
		try
		{
			var result = await _schedulingService.CheckClashesAsync(
				dto.ExcludeEventId,
				dto.RoomId,
				dto.StartUtc,
				dto.EndUtc,
				dto.CohortIds,
				dto.LecturerIds);

			if (!result.HasClash)
			{
				return Ok(new
				{
					hasClash = false,
					message = "No clashes detected."
				});
			}

			return Conflict(new
			{
				hasClash = true,
				roomClash = result.RoomClash,
				cohortClashes = result.CohortClashes,
				lecturerClashes = result.LecturerClashes
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("recurring-events")]
	public async Task<IActionResult> CreateRecurringEvents([FromBody] RecurringEventCreateDto dto)
	{
		try
		{
			var events = await _schedulingService.GenerateRecurringEventsAsync(dto, CurrentUserId);

			if (events.Count == 0)
			{
				return BadRequest("No recurring events could be created. All weeks may have clashes or term dates may not match.");
			}

			_db.TimetableEvents.AddRange(events);
			await _db.SaveChangesAsync();

			foreach (var ev in events)
			{
				foreach (var cohortId in dto.CohortIds.Distinct())
				{
					_db.EventCohorts.Add(new EventCohort
					{
						TimetableEventId = ev.TimetableEventId,
						CohortId = cohortId
					});
				}

				foreach (var lecturerId in dto.LecturerIds.Distinct())
				{
					_db.EventLecturers.Add(new EventLecturer
					{
						TimetableEventId = ev.TimetableEventId,
						LecturerId = lecturerId
					});
				}
			}

			await _db.SaveChangesAsync();

			return Ok(new
			{
				createdCount = events.Count,
				recurrenceGroupId = events.First().RecurrenceGroupId,
				eventIds = events.Select(e => e.TimetableEventId).ToList()
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet("requests/pending")]
	public async Task<IActionResult> GetPendingRequests()
	{
		var pendingRequests = await _db.Requests
			.AsNoTracking()
			.Where(r => r.RequestStatus.Name == "Pending")
			.OrderBy(r => r.CreatedAtUtc)
			.Select(r => new
			{
				r.RequestId,
				r.Title,
				r.Notes,
				RequestType = r.RequestType.Name,
				RequestStatus = r.RequestStatus.Name,
				r.RequestedByUserId,
				r.CreatedAtUtc,

				ScheduleChangeDetail = _db.RequestScheduleChanges
					.Where(sc => sc.RequestId == r.RequestId)
					.Select(sc => new
					{
						sc.TimetableEventId,
						sc.ProposedRoomId,
						sc.ProposedStartUtc,
						sc.ProposedEndUtc,
						sc.Reason
					})
					.FirstOrDefault(),

				RoomBookingDetail = _db.RequestRoomBookings
					.Where(rb => rb.RequestId == r.RequestId)
					.Select(rb => new
					{
						rb.RoomId,
						rb.StartUtc,
						rb.EndUtc,
						rb.Purpose,
						rb.ExpectedAttendees
					})
					.FirstOrDefault()
			})
			.ToListAsync();

		return Ok(pendingRequests);
	}

	[HttpPost("requests/{id:long}/decide")]
	public async Task<IActionResult> DecideRequest(long id, [FromBody] DecideRequestDto dto)
	{
		try
		{
			var result = await _requestService.DecideAsync(
				requestId: id,
				decidedByUserId: CurrentUserId,
				decision: dto.Decision,
				comment: dto.Comment);

			return result.IsSuccess ? Ok(result) : Conflict(result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(ex.Message);
		}
	}
}