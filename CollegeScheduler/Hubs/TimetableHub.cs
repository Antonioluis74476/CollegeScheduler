using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CollegeScheduler.Hubs;

[Authorize]
public sealed class TimetableHub : Hub
{
	private readonly ILogger<TimetableHub> _logger;

	public TimetableHub(ILogger<TimetableHub> logger)
	{
		_logger = logger;
	}

	public override async Task OnConnectedAsync()
	{
		var userId = Context.UserIdentifier;

		_logger.LogInformation("SignalR connected. UserId={UserId}, ConnectionId={ConnectionId}",
			userId, Context.ConnectionId);

		if (Context.User?.IsInRole("Admin") == true)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
		}

		await base.OnConnectedAsync();
	}

	public async Task JoinCohortGroup(int cohortId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, $"cohort-{cohortId}");

		_logger.LogInformation(
			"Connection {ConnectionId} joined cohort group {GroupName}",
			Context.ConnectionId,
			$"cohort-{cohortId}");
	}

	public async Task LeaveCohortGroup(int cohortId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cohort-{cohortId}");
	}

	public async Task JoinLecturerGroup(string userId)
	{
		if (Context.UserIdentifier != userId)
		{
			_logger.LogWarning(
				"User {ActualUserId} tried to join lecturer group for {RequestedUserId}",
				Context.UserIdentifier,
				userId);

			return;
		}

		await Groups.AddToGroupAsync(Context.ConnectionId, $"lecturer-{userId}");

		_logger.LogInformation(
			"Connection {ConnectionId} joined lecturer group {GroupName}",
			Context.ConnectionId,
			$"lecturer-{userId}");
	}
}