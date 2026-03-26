using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class AcademicYear : AuditableEntity
	{
		[Key]
		public int AcademicYearId { get; set; }

		[Required, StringLength(20)]
		public string Label { get; set; } = ""; // e.g. 2025/2026

		[Required]
		public DateTime StartDate { get; set; } // store as date in SQL

		[Required]
		public DateTime EndDate { get; set; } // store as date in SQL

		public bool IsCurrent { get; set; } = false;
	}
}
