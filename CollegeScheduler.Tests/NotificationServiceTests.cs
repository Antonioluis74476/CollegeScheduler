using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Messaging;
using CollegeScheduler.Services;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollegeScheduler.Tests.Services;

public class NotificationServiceTests
{
	private static ApplicationDbContext CreateDbContext(string dbName)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private static NotificationService CreateService(
		ApplicationDbContext db,
		Mock<IPublishEndpoint>? publishEndpointMock = null)
	{
		publishEndpointMock ??= new Mock<IPublishEndpoint>();
		var logger = new Mock<ILogger<NotificationService>>();

		return new NotificationService(
			db,
			publishEndpointMock.Object,
			logger.Object);
	}

	private static void SeedNotificationType(ApplicationDbContext db, string name = "EventChanged", int id = 1)
	{
		db.NotificationTypes.Add(new NotificationType
		{
			NotificationTypeId = id,
			Name = name
		});
	}

	private static void SeedUser(ApplicationDbContext db, string id, string? email)
	{
		db.Users.Add(new ApplicationUser
		{
			Id = id,
			UserName = email ?? $"{id}@test.local",
			Email = email
		});
	}

	[Fact]
	public async Task CreateAsync_ShouldThrow_WhenNotificationTypeDoesNotExist()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldThrow_WhenNotificationTypeDoesNotExist));
		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		Func<Task> act = async () => await service.CreateAsync(
			notificationTypeName: "MissingType",
			title: "Title",
			message: "Message",
			recipientUserIds: new[] { "user-1" });

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("NotificationType 'MissingType' not found.");
	}

	[Fact]
	public async Task CreateAsync_ShouldThrow_WhenNoValidRecipientsRemain()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldThrow_WhenNoValidRecipientsRemain));
		SeedNotificationType(db);
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		Func<Task> act = async () => await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Title",
			message: "Message",
			recipientUserIds: new[] { "", " ", "\t" });

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("At least one recipient is required.");
	}

	[Fact]
	public async Task CreateAsync_ShouldCreateNotification_WhenValid()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldCreateNotification_WhenValid));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		var notificationId = await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Class moved",
			message: "Your class has changed time.",
			recipientUserIds: new[] { "user-1" });

		notificationId.Should().BeGreaterThan(0);

		var notification = await db.Notifications.FirstAsync(x => x.NotificationId == notificationId);
		notification.Title.Should().Be("Class moved");
		notification.Message.Should().Be("Your class has changed time.");
		notification.NotificationTypeId.Should().Be(1);
	}

	[Fact]
	public async Task CreateAsync_ShouldCreateRecipientRows_ForDistinctValidRecipients()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldCreateRecipientRows_ForDistinctValidRecipients));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		SeedUser(db, "user-2", "user2@test.com");
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		var notificationId = await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Update",
			message: "Message",
			recipientUserIds: new[] { "user-1", "user-2", "user-1", "", " " });

		var recipients = await db.NotificationRecipients
			.Where(x => x.NotificationId == notificationId)
			.OrderBy(x => x.UserId)
			.ToListAsync();

		recipients.Should().HaveCount(2);
		recipients[0].UserId.Should().Be("user-1");
		recipients[1].UserId.Should().Be("user-2");
		recipients.All(x => x.DeliveryStatus == "Pending").Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_ShouldStoreRelatedIds_WhenProvided()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldStoreRelatedIds_WhenProvided));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		var notificationId = await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Update",
			message: "Message",
			recipientUserIds: new[] { "user-1" },
			relatedTimetableEventId: 123,
			relatedRequestId: 456);

		var notification = await db.Notifications.FirstAsync(x => x.NotificationId == notificationId);

		notification.RelatedTimetableEventId.Should().Be(123);
		notification.RelatedRequestId.Should().Be(456);
	}

	[Fact]
	public async Task CreateAsync_ShouldPublishOneEmailPerRecipientWithEmail()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldPublishOneEmailPerRecipientWithEmail));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		SeedUser(db, "user-2", "user2@test.com");
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Class moved",
			message: "Your class has changed time.",
			recipientUserIds: new[] { "user-1", "user-2" });

		publishEndpointMock.Verify(
			x => x.Publish(It.IsAny<SendEmailMessage>(), default),
			Times.Exactly(2));
	}

	[Fact]
	public async Task CreateAsync_ShouldNotPublishEmail_ForRecipientWithoutEmail()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldNotPublishEmail_ForRecipientWithoutEmail));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		SeedUser(db, "user-2", null);
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Class moved",
			message: "Your class has changed time.",
			recipientUserIds: new[] { "user-1", "user-2" });

		publishEndpointMock.Verify(
			x => x.Publish(It.IsAny<SendEmailMessage>(), default),
			Times.Once);
	}

	[Fact]
	public async Task CreateAsync_ShouldPublishEmailWithCorrectSubjectAndBody()
	{
		using var db = CreateDbContext(nameof(CreateAsync_ShouldPublishEmailWithCorrectSubjectAndBody));
		SeedNotificationType(db);
		SeedUser(db, "user-1", "user1@test.com");
		await db.SaveChangesAsync();

		var publishEndpointMock = new Mock<IPublishEndpoint>();
		var service = CreateService(db, publishEndpointMock);

		await service.CreateAsync(
			notificationTypeName: "EventChanged",
			title: "Important notice",
			message: "Room changed to B201.",
			recipientUserIds: new[] { "user-1" });

		publishEndpointMock.Verify(
			x => x.Publish(
				It.Is<SendEmailMessage>(m =>
					m.To == "user1@test.com" &&
					m.Subject == "Important notice" &&
					m.Body == "Room changed to B201."),
				default),
			Times.Once);
	}
}