namespace BookOrbit.Application.Features.Chatbot;

public static class ChatbotApplicationErrors
{
    public static readonly Error ChatbotUnavailable = Error.Failure(
        "Chatbot.Unavailable",
        "The chatbot service is temporarily unavailable. Please try again later.");

    public static readonly Error UserNotFound = Error.Failure(
        "Chatbot.UserNotFound",
        "Your account could not be found. Please log in again.");
}
