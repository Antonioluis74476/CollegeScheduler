using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Profiles;

namespace CollegeScheduler.Data.Entities.Membership
{
	public sealed class StudentModuleEnrollment : AuditableEntity
	{
		// Composite PK
		public int StudentId { get; set; }
		public int ModuleId { get; set; }
		public int TermId { get; set; }

		// Data
		public string EnrollmentType { get; set; } = ""; // Regular/Repeat/Resit/Additional
		public int? AttendWithCohortId { get; set; }
		public string Status { get; set; } = "Enrolled"; // Enrolled/Withdrawn/Completed/Failed
		public DateTime EnrolledAtUtc { get; set; }

		// Navigations
		public StudentProfile Student { get; set; } = null!;
		public Module Module { get; set; } = null!;
		public Term Term { get; set; } = null!;
		public Cohort? AttendWithCohort { get; set; }
	}
}