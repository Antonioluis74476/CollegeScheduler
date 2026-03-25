using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Common;
using CollegeScheduler.Data.Entities.Profiles;

namespace CollegeScheduler.Data.Entities.Membership
{
	public sealed class StudentCohortMembership : AuditableEntity
	{
		// Composite PK
		public int StudentId { get; set; }
		public int CohortId { get; set; }
		public int AcademicYearId { get; set; }

		// Data
		public string MembershipType { get; set; } = ""; // Primary/RepeatYear/Visiting
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }

		// Navigations
		public StudentProfile Student { get; set; } = null!;
		public Cohort Cohort { get; set; } = null!;
		public AcademicYear AcademicYear { get; set; } = null!;
	}
}