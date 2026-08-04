using System.Net;
using System.Net.Mail;
using CollegeScheduler.Messaging;
using MassTransit;
using Microsoft.Extensions.Options;

public sealed class SendEmailConsumer : IConsumer<SendEmailMessage>
{
	private readonly ILogger<SendEmailConsumer> _logger;
	private readonly SmtpSettings _settings;

	public SendEmailConsumer(
		ILogger<SendEmailConsumer> logger,
		IOptions<SmtpSettings> settings)
	{
		_logger = logger;
		_settings = settings.Value;
	}

	public async Task Consume(ConsumeContext<SendEmailMessage> context)
	{
		var msg = context.Message;
		try
		{
			using var smtp = new SmtpClient(_settings.Host)
			{
				Port = _settings.Port,
				Credentials = new NetworkCredential(_settings.Username, _settings.Password),
				EnableSsl = true
			};

			using var mail = new MailMessage
			{
				From = new MailAddress(_settings.FromAddress, _settings.FromName),
				Subject = msg.Subject,
				Body = msg.Body,
				IsBodyHtml = false
			};
			mail.To.Add(msg.To);

			await smtp.SendMailAsync(mail);

			_logger.LogInformation(
				"Email sent successfully to {To}", msg.To);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Failed to send email to {To}", msg.To);
		}
	}
}