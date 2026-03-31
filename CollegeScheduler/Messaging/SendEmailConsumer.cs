using MassTransit;

namespace CollegeScheduler.Messaging;

public sealed class SendEmailConsumer : IConsumer<SendEmailMessage>
{
	private readonly ILogger<SendEmailConsumer> _logger;

	public SendEmailConsumer(ILogger<SendEmailConsumer> logger)
	{
		_logger = logger;
	}

	public Task Consume(ConsumeContext<SendEmailMessage> context)
	{
		var msg = context.Message;

		_logger.LogInformation(
			"RabbitMQ email consumer received message. To={To}, Subject={Subject}",
			msg.To,
			msg.Subject);

		// MVP version:
		// just log receipt of the email job.
		// Later, you can replace this with real SMTP sending.

		return Task.CompletedTask;
	}
}