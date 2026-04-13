using System.Net;
using System.Net.Mail;
using CollegeScheduler.Messaging;
using MassTransit;

public sealed class SendEmailConsumer : IConsumer<SendEmailMessage>
{
	private readonly ILogger<SendEmailConsumer> _logger;

	public SendEmailConsumer(ILogger<SendEmailConsumer> logger)
	{
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<SendEmailMessage> context)
	{
		var msg = context.Message;

		try
		{
			var smtp = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
				EnableSsl = true
			};

			var mail = new MailMessage
			{
				From = new MailAddress("your-email@gmail.com"),
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