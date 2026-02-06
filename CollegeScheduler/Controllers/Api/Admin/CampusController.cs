using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Controllers.Api.Admin
{
	[ApiController]
	[Route("api/v1/admin/campuses")]
	[Authorize(Roles = "Admin")]
	public class CampusController : ControllerBase
	{
		private readonly ApplicationDbContext _db;

		public CampusController(ApplicationDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<ActionResult<PagedResult<CampusDto>>> GetAll([FromQuery] CampusQuery q)
		{
			var query = _db.Campuses.AsNoTracking();

			// Filtering
			if (!string.IsNullOrWhiteSpace(q.Search))
			{
				var s = q.Search.Trim();
				query = query.Where(c =>
					c.Name.Contains(s) ||
					c.Code.Contains(s) ||
					(c.City != null && c.City.Contains(s)) ||
					(c.Address != null && c.Address.Contains(s)));
			}

			if (!string.IsNullOrWhiteSpace(q.Code))
			{
				var code = q.Code.Trim();
				query = query.Where(c => c.Code == code);
			}

			if (!string.IsNullOrWhiteSpace(q.City))
			{
				var city = q.City.Trim();
				query = query.Where(c => c.City != null && c.City.Contains(city));
			}

			if (q.IsActive.HasValue)
			{
				query = query.Where(c => c.IsActive == q.IsActive.Value);
			}

			// Sorting (safe)
			var sortBy = (q.SortBy ?? "name").Trim().ToLowerInvariant();
			var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

			query = sortBy switch
			{
				"code" => desc ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
				"city" => desc ? query.OrderByDescending(c => c.City) : query.OrderBy(c => c.City),
				_ => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
			};

			// Paging (guardrails)
			var page = q.Page < 1 ? 1 : q.Page;
			var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
			if (pageSize > 100) pageSize = 100;

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
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
				Items = items,
				TotalCount = totalCount,
				Page = page,
				PageSize = pageSize
			});
		}


		[HttpGet("{id:int}")]
		public async Task<ActionResult<CampusDto>> GetById(int id)
		{
			var item = await _db.Campuses
				.AsNoTracking()
				.Where(c => c.CampusId == id)
				.Select(c => new CampusDto
				{
					CampusId = c.CampusId,
					Code = c.Code,
					Name = c.Name,
					Address = c.Address,
					City = c.City,
					IsActive = c.IsActive
				})
				.FirstOrDefaultAsync();

			return item is null ? NotFound() : Ok(item);
		}

		[HttpPost]
		public async Task<ActionResult<CampusDto>> Create([FromBody] CampusCreateDto dto)
		{
			var campus = new Campus
			{
				Code = dto.Code.Trim(),
				Name = dto.Name.Trim(),
				Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
				City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
				IsActive = true
			};

			_db.Campuses.Add(campus);

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Conflict("Could not save Campus. A campus with the same Code may already exist.");
			}

			var result = new CampusDto
			{
				CampusId = campus.CampusId,
				Code = campus.Code,
				Name = campus.Name,
				Address = campus.Address,
				City = campus.City,
				IsActive = campus.IsActive
			};

			return CreatedAtAction(nameof(GetById), new { id = campus.CampusId }, result);
		}


		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] CampusUpdateDto dto)
		{
			var campus = await _db.Campuses.FirstOrDefaultAsync(c => c.CampusId == id);
			if (campus is null) return NotFound();

			campus.Code = dto.Code.Trim();
			campus.Name = dto.Name.Trim();
			campus.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
			campus.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
			campus.IsActive = dto.IsActive;

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Conflict("Could not update Campus. A campus with the same Code may already exist.");
			}

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var campus = await _db.Campuses.FindAsync(id);
			if (campus is null) return NotFound();

			_db.Campuses.Remove(campus);
			await _db.SaveChangesAsync();

			return NoContent();
		}
	}
}
