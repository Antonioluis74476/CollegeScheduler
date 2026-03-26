using System;

namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentModuleEnrollmentDto
	{
		public int StudentId { get; set; }
		public int ModuleId { get; set; }
		public int TermId { get; set; }

		public string EnrollmentType { get; set; } = "";
		public int? AttendWithCohortId { get; set; }
		public string Status { get; set; } = "";
		public DateTime EnrolledAtUtc { get; set; }
	}
}