namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentModuleEnrollmentUpdateDto
	{
		public string EnrollmentType { get; set; } = "";
		public int? AttendWithCohortId { get; set; }
		public string Status { get; set; } = "";
	}
}