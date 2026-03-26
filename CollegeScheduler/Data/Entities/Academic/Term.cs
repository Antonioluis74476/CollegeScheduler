using CollegeScheduler.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Academic
{
	public class Term : AuditableEntity
	{
		[Key]
		public int TermId { get; set; }

		[Required]
		public int AcademicYearId { get; set; }
		public AcademicYear? AcademicYear { get; set; }

		[Required]
		public int TermNumber { get; set; } // 1,2,3

		[Required, StringLength(50)]
		public string Name { get; set; } = ""; // Autumn/Spring/Summer

		[Required]
		public DateTime StartDate { get; set; } // stored as date in SQL

		[Required]
		public DateTime EndDate { get; set; } // stored as date in SQL
	}
}
