using Microsoft.AspNetCore.SignalR;

namespace CollegeScheduler.Hubs;

public sealed class TimetableHubNotifier
{
	private readonly IHubContext<TimetableHub> _hub;
	private readonly ILogger<TimetableHubNotifier> _logger;

	public TimetableHubNotifier(
		IHubContext<TimetableHub> hub,
		ILogger<TimetableHubNotifier> logger)
	{
		_hub = hub;
		_logger = logger;
	}

	public async Task PushRequestDecisionAsync(string requesterUserId, long requestId, string decision, string? comment)
	{
		await _hub.Clients.User(requesterUserId).SendAsync("RequestDecision", new
		{
			requestId,
			decision,
			comment
		});

		_logger.LogInformation(
			"SignalR RequestDecision pushed. RequestId={RequestId}, UserId={UserId}, Decision={Decision}",
			requestId,
			requesterUserId,
			decision);
	}

	public async Task PushEventChangedAsync(
		long timetableEventId,
		IEnumerable<int> cohortIds,
		IEnumerable<string> lecturerUserIds,
		DateTime oldStartUtc,
		DateTime newStartUtc)
	{
		var payload = new
		{
			timetableEventId,
			oldStartUtc,
			newStartUtc
		};

		foreach (var cohortId in cohortIds.Distinct())
		{
			await _hub.Clients.Group($"cohort-{cohortId}")
				.SendAsync("EventChanged", payload);
		}

		foreach (var userId in lecturerUserIds.Distinct())
		{
			await _hub.Clients.Group($"lecturer-{userId}")
				.SendAsync("EventChanged", payload);
		}

		await _hub.Clients.Group("admin").SendAsync("EventChanged", payload);

		_logger.LogInformation(
			"SignalR EventChanged pushed. TimetableEventId={TimetableEventId}",
			timetableEventId);
	}

	public async Task PushEventCancelledAsync(
		long timetableEventId,
		IEnumerable<int> cohortIds,
		IEnumerable<string> lecturerUserIds,
		string reason)
	{
		var payload = new
		{
			timetableEventId,
			reason
		};

		foreach (var cohortId in cohortIds.Distinct())
		{
			await _hub.Clients.Group($"cohort-{cohortId}")
				.SendAsync("EventCancelled", payload);
		}

		foreach (var userId in lecturerUserIds.Distinct())
		{
			await _hub.Clients.Group($"lecturer-{userId}")
				.SendAsync("EventCancelled", payload);
		}

		await _hub.Clients.Group("admin").SendAsync("EventCancelled", payload);

		_logger.LogInformation(
			"SignalR EventCancelled pushed. TimetableEventId={TimetableEventId}",
			timetableEventId);
	}
}