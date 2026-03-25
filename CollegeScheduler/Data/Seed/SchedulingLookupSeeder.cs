using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data.Seed;

public static class SchedulingLookupSeeder
{
	public static async Task SeedAsync(IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		if (!await db.EventStatuses.AnyAsync())
		{
			db.EventStatuses.AddRange(
				new EventStatus { Name = "Scheduled" },
				new EventStatus { Name = "Cancelled" },
				new EventStatus { Name = "Moved" },
				new EventStatus { Name = "Completed" }
			);
		}

		if (!await db.RequestTypes.AnyAsync())
		{
			db.RequestTypes.AddRange(
				new RequestType { Name = "RoomBooking" },
				new RequestType { Name = "Reschedule" },
				new RequestType { Name = "CancelClass" },
				new RequestType { Name = "ChangeRoom" }
			);
		}

		if (!await db.RequestStatuses.AnyAsync())
		{
			db.RequestStatuses.AddRange(
				new RequestStatus { Name = "Pending" },
				new RequestStatus { Name = "Approved" },
				new RequestStatus { Name = "Rejected" },
				new RequestStatus { Name = "Cancelled" }
			);
		}

		if (!await db.NotificationTypes.AnyAsync())
		{
			db.NotificationTypes.AddRange(
				new NotificationType { Name = "EventCreated" },
				new NotificationType { Name = "EventChanged" },
				new NotificationType { Name = "EventCancelled" },
				new NotificationType { Name = "RequestApproved" },
				new NotificationType { Name = "RequestRejected" }
			);
		}

		await db.SaveChangesAsync();
	}
}