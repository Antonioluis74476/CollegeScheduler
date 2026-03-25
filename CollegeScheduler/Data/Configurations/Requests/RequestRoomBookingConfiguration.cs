using CollegeScheduler.Data.Entities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollegeScheduler.Data.Configurations.Requests
{
	public class RequestRoomBookingConfiguration : IEntityTypeConfiguration<RequestRoomBooking>
	{
		public void Configure(EntityTypeBuilder<RequestRoomBooking> b)
		{
			b.ToTable("RequestRoomBookings");

			b.HasKey(x => x.RequestId);

			b.Property(x => x.Purpose)
				.HasMaxLength(300)
				.IsRequired();

			b.Property(x => x.ExpectedAttendees)
				.HasDefaultValue(1);

			b.HasIndex(x => new { x.RoomId, x.StartUtc, x.EndUtc });

			b.ToTable(t => t.HasCheckConstraint("CK_RequestRoomBooking_EndUtc_StartUtc", "[EndUtc] > [StartUtc]"));

			b.HasOne(x => x.Request)
				.WithOne(x => x.RoomBookingDetail)
				.HasForeignKey<RequestRoomBooking>(x => x.RequestId)
				.OnDelete(DeleteBehavior.Cascade);

			b.HasOne(x => x.Room)
				.WithMany()
				.HasForeignKey(x => x.RoomId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}