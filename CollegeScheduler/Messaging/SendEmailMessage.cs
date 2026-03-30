namespace CollegeScheduler.Messaging;

public sealed record SendEmailMessage(
	string To,
	string Subject,
	string Body);