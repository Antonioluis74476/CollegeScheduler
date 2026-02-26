namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentModuleEnrollmentQuery
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;

		public int? StudentId { get; set; }
		public int? ModuleId { get; set; }
		public int? TermId { get; set; }
		public int? AttendWithCohortId { get; set; }
		public string? Status { get; set; }
		public string? EnrollmentType { get; set; }
	}
}