using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Profiles;

namespace CollegeScheduler.Data.Entities.Academic
{
	public sealed class ModuleLecturer : AuditableEntity
	{
		// Composite PK
		public int ModuleId { get; set; }
		public int LecturerId { get; set; }
		public int TermId { get; set; }

		public string Role { get; set; } = "Lead"; // Lead/Assistant/Substitute
		public DateTime AssignedAtUtc { get; set; }

		public Module Module { get; set; } = null!;
		public LecturerProfile Lecturer { get; set; } = null!;
		public Term Term { get; set; } = null!;
	}
}