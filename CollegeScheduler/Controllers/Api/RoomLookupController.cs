using CollegeScheduler.Data;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using CollegeScheduler.DTOs.Facilities;
using CollegeScheduler.DTOs.Common;

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

    [HttpGet("campuses")]
    public async Task<ActionResult<PagedResult<CampusDto>>> GetCampuses()
    {
        var campuses = await _db.Campuses
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CampusDto
            {
                CampusId = c.CampusId,
                Code = c.Code,
                Name = c.Name,
                Address = c.Address,
                City = c.City,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(new PagedResult<CampusDto>
        {
            Items = campuses,
            Page = 1,
            PageSize = campuses.Count,
            TotalCount = campuses.Count
        });
    }

    [HttpGet("campuses/{campusId:int}/buildings")]
    public async Task<ActionResult<PagedResult<BuildingDto>>> GetBuildings(
        int campusId)
    {
        var buildings = await _db.Buildings
            .Where(b => b.CampusId == campusId && b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new BuildingDto
            {
                BuildingId = b.BuildingId,
                CampusId = b.CampusId,
                Name = b.Name,
                Code = b.Code,
                IsActive = b.IsActive
            })
            .ToListAsync();

        return Ok(new PagedResult<BuildingDto>
        {
            Items = buildings,
            Page = 1,
            PageSize = buildings.Count,
            TotalCount = buildings.Count
        });
    }


}