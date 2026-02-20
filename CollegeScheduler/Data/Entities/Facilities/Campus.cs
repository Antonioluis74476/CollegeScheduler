using CollegeScheduler.Data.Entities.Common;

namespace CollegeScheduler.Data.Entities.Facilities;

public class Campus : AuditableEntity
{
	public int CampusId { get; set; }

	public string Code { get; set; } = "";
	public string Name { get; set; } = "";

	public string? Address { get; set; }
	public string? City { get; set; }

	public List<Building> Buildings { get; set; } = new();
}
