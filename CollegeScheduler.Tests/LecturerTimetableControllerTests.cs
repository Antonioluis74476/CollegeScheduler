using System.Security.Claims;
using CollegeScheduler.Controllers.Api.Lecturer;
using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Controllers;

public class LecturerTimetableControllerTests
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

	private static LecturerTimetableController CreateController(
		ApplicationDbContext db,
		Mock<IRequestService>? requestServiceMock = null,
		Mock<UserManager<ApplicationUser>>? userManagerMock = null,
		string userId = "lecturer-user-1")
	{
		requestServiceMock ??= new Mock<IRequestService>();
		userManagerMock ??= CreateUserManagerMock();

		var controller = new LecturerTimetableController(
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

	private static LecturerProfile CreateLecturerProfile(int lecturerId, string userId)
	{
		return new LecturerProfile
		{
			LecturerId = lecturerId,
			UserId = userId,
			StaffNumber = $"L{lecturerId:0000}",
			Name = "Test",
			LastName = "Lecturer",
			Email = "lecturer@test.com"
		};
	}

	[Fact]
	public async Task GetMyTimetable_ShouldReturnNotFound_WhenNoLecturerProfileExists()
	{
		using var db = CreateDbContext(nameof(GetMyTimetable_ShouldReturnNotFound_WhenNoLecturerProfileExists));
		var controller = CreateController(db);

		var result = await controller.GetMyTimetable(null, null);

		result.Should().BeOfType<NotFoundObjectResult>();
	}

	[Fact]
	public async Task CreateScheduleChangeRequest_ShouldReturnNotFound_WhenNoLecturerProfileExists()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequest_ShouldReturnNotFound_WhenNoLecturerProfileExists));
		var requestServiceMock = new Mock<IRequestService>();
		var controller = CreateController(db, requestServiceMock);

		var result = await controller.CreateScheduleChangeRequest(new ScheduleChangeRequestCreateDto
		{
			TimetableEventId = 1,
			ProposedRoomId = 2,
			ProposedStartUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
			ProposedEndUtc = new DateTime(2026, 4, 1, 13, 0, 0, DateTimeKind.Utc),
			Reason = "Need to move class"
		});

		result.Should().BeOfType<NotFoundObjectResult>();
	}

	[Fact]
	public async Task CreateScheduleChangeRequest_ShouldReturnForbid_WhenLecturerIsNotAssignedToEvent()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequest_ShouldReturnForbid_WhenLecturerIsNotAssignedToEvent));

		db.LecturerProfiles.Add(CreateLecturerProfile(1, "lecturer-user-1"));
		await db.SaveChangesAsync();

		var requestServiceMock = new Mock<IRequestService>();
		var controller = CreateController(db, requestServiceMock);

		var result = await controller.CreateScheduleChangeRequest(new ScheduleChangeRequestCreateDto
		{
			TimetableEventId = 999,
			ProposedRoomId = 2,
			ProposedStartUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
			ProposedEndUtc = new DateTime(2026, 4, 1, 13, 0, 0, DateTimeKind.Utc),
			Reason = "Need to move class"
		});

		result.Should().BeOfType<ForbidResult>();
	}

	[Fact]
	public async Task CreateScheduleChangeRequest_ShouldReturnOk_WhenValid()
	{
		using var db = CreateDbContext(nameof(CreateScheduleChangeRequest_ShouldReturnOk_WhenValid));

		db.LecturerProfiles.Add(CreateLecturerProfile(1, "lecturer-user-1"));
		db.EventLecturers.Add(new EventLecturer
		{
			TimetableEventId = 100,
			LecturerId = 1
		});
		await db.SaveChangesAsync();

		var requestServiceMock = new Mock<IRequestService>();
		requestServiceMock
			.Setup(x => x.CreateScheduleChangeRequestAsync(
				"lecturer-user-1",
				100,
				2,
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				"Need to move class"))
			.ReturnsAsync(500);

		var controller = CreateController(db, requestServiceMock);

		var result = await controller.CreateScheduleChangeRequest(new ScheduleChangeRequestCreateDto
		{
			TimetableEventId = 100,
			ProposedRoomId = 2,
			ProposedStartUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
			ProposedEndUtc = new DateTime(2026, 4, 1, 13, 0, 0, DateTimeKind.Utc),
			Reason = "Need to move class"
		});

		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task CreateCancelClassRequest_ShouldReturnForbid_WhenLecturerIsNotAssignedToEvent()
	{
		using var db = CreateDbContext(nameof(CreateCancelClassRequest_ShouldReturnForbid_WhenLecturerIsNotAssignedToEvent));

		db.LecturerProfiles.Add(CreateLecturerProfile(1, "lecturer-user-1"));
		await db.SaveChangesAsync();

		var requestServiceMock = new Mock<IRequestService>();
		var controller = CreateController(db, requestServiceMock);

		var result = await controller.CreateCancelClassRequest(new CancelClassRequestCreateDto
		{
			TimetableEventId = 999,
			Reason = "Cannot attend"
		});

		result.Should().BeOfType<ForbidResult>();
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