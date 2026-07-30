using CollegeScheduler.Data;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api;

[ApiController]
[Route("api/v1/rooms")]
[Authorize(Roles = "Student,Lecturer")]
public class RoomLookupController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    private readonly ApplicationDbContext _db;

    public RoomLookupController(
        ISchedulingService schedulingService,
        ApplicationDbContext db)
    {
        _schedulingService = schedulingService;
        _db = db;
    }


[HttpGet("available")]
    public async Task<ActionResult<List<AvailableRoomDto>>> GetAvailableRooms(
    [FromQuery] RoomSearchQuery query)
    {
        var rooms = await _schedulingService.FindAvailableRoomsAsync(query);

        if (User.IsInRole("Student"))
        {
            var allowedIds = await _db.Rooms
                .Where(r => r.IsBookableByStudents)
                .Select(r => r.RoomId)
                .ToListAsync();

            rooms = rooms
                .Where(r => allowedIds.Contains(r.RoomId))
                .ToList();
        }

        return Ok(rooms);
    }

    [HttpPost("recurring-available")]
    public async Task<ActionResult<List<RecurringAvailableRoomDto>>> GetRecurringAvailableRooms(
        [FromBody] RecurringRoomSearchQuery query)
    {
        var rooms = await _schedulingService.FindRecurringAvailableRoomsAsync(query);

        if (User.IsInRole("Student"))
        {
            var allowedIds = await _db.Rooms
                .Where(r => r.IsBookableByStudents)
                .Select(r => r.RoomId)
                .ToListAsync();

            rooms = rooms
                .Where(r => allowedIds.Contains(r.RoomId))
                .ToList();
        }

        return Ok(rooms);
    }


}