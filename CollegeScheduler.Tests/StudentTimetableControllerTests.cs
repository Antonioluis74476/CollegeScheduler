using System.Security.Claims;
using CollegeScheduler.Controllers.Api.Student;
using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Entities.Membership;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Controllers;

public class StudentTimetableControllerTests
{
	private static ApplicationDbContext CreateDbContext(string dbName)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
	{
		var store = new Mock<IUserStore<ApplicationUser>>();

		return new Mock<UserManager<ApplicationUser>>(
			store.Object,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!);
	}

	private static StudentTimetableController CreateController(
		ApplicationDbContext db,
		Mock<IRequestService>? requestServiceMock = null,
		Mock<UserManager<ApplicationUser>>? userManagerMock = null,
		string userId = "student-user-1")
	{
		requestServiceMock ??= new Mock<IRequestService>();
		userManagerMock ??= CreateUserManagerMock();

		var controller = new StudentTimetableController(
			db,
			requestServiceMock.Object,
			userManagerMock.Object);

		var user = new ClaimsPrincipal(new ClaimsIdentity(
			new[]
			{
				new Claim(ClaimTypes.NameIdentifier, userId)
			},
			"TestAuth"));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = user
			}
		};

		return controller;
	}

	private static StudentProfile CreateStudentProfile(int studentId, string userId)
	{
		return new StudentProfile
		{
			StudentId = studentId,
			UserId = userId,
			StudentNumber = $"S{studentId:0000}",
			Name = "Test",
			LastName = "Student",
			Email = "student@test.com",
			Status = "Active"
		};
	}

	[Fact]
	public async Task GetMyTimetable_ShouldReturnNotFound_WhenNoStudentProfileExists()
	{
		using var db = CreateDbContext(nameof(GetMyTimetable_ShouldReturnNotFound_WhenNoStudentProfileExists));
		var controller = CreateController(db);

		var result = await controller.GetMyTimetable(null, null);

		result.Should().BeOfType<NotFoundObjectResult>();
	}

	[Fact]
	public async Task GetMyTimetable_ShouldReturnOkWithEmptyList_WhenStudentHasNoCohortMemberships()
	{
		using var db = CreateDbContext(nameof(GetMyTimetable_ShouldReturnOkWithEmptyList_WhenStudentHasNoCohortMemberships));

		db.StudentProfiles.Add(CreateStudentProfile(1, "student-user-1"));
		await db.SaveChangesAsync();

		var controller = CreateController(db);

		var result = await controller.GetMyTimetable(null, null);

		result.Should().BeOfType<OkObjectResult>();

		var ok = result as OkObjectResult;
		ok!.Value.Should().NotBeNull();
	}

	[Fact]
	public async Task MarkNotificationAsRead_ShouldReturnNotFound_WhenNotificationRecipientDoesNotExist()
	{
		using var db = CreateDbContext(nameof(MarkNotificationAsRead_ShouldReturnNotFound_WhenNotificationRecipientDoesNotExist));
		var controller = CreateController(db);

		var result = await controller.MarkNotificationAsRead(999);

		result.Should().BeOfType<NotFoundResult>();
	}

	[Fact]
	public async Task MarkNotificationAsRead_ShouldReturnNoContent_WhenNotificationRecipientExists()
	{
		using var db = CreateDbContext(nameof(MarkNotificationAsRead_ShouldReturnNoContent_WhenNotificationRecipientExists));

		db.NotificationTypes.Add(new NotificationType
		{
			NotificationTypeId = 1,
			Name = "EventChanged"
		});

		db.Notifications.Add(new Notification
		{
			NotificationId = 1,
			NotificationTypeId = 1,
			Title = "Test notification",
			Message = "Message"
		});

		db.NotificationRecipients.Add(new NotificationRecipient
		{
			NotificationId = 1,
			UserId = "student-user-1",
			DeliveryStatus = "Pending"
		});

		await db.SaveChangesAsync();

		var controller = CreateController(db);

		var result = await controller.MarkNotificationAsRead(1);

		result.Should().BeOfType<NoContentResult>();

		var row = await db.NotificationRecipients.FirstAsync(x =>
			x.NotificationId == 1 &&
			x.UserId == "student-user-1");

		row.ReadAtUtc.Should().NotBeNull();
	}

	[Fact]
	public async Task GetMyProfile_ShouldReturnNotFound_WhenProfileDoesNotExist()
	{
		using var db = CreateDbContext(nameof(GetMyProfile_ShouldReturnNotFound_WhenProfileDoesNotExist));
		var controller = CreateController(db);

		var result = await controller.GetMyProfile();

		result.Should().BeOfType<NotFoundObjectResult>();
	}

	[Fact]
	public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordsAreMissing()
	{
		using var db = CreateDbContext(nameof(ChangePassword_ShouldReturnBadRequest_WhenPasswordsAreMissing));
		var controller = CreateController(db);

		var result = await controller.ChangePassword(new ChangePasswordDto
		{
			CurrentPassword = "",
			NewPassword = ""
		});

		result.Should().BeOfType<BadRequestObjectResult>();
	}
}