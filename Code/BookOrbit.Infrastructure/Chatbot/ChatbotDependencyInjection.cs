using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Infrastructure.Services.Chatbot;
using BookOrbit.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace BookOrbit.Infrastructure.Chatbot;

public static class ChatbotDependencyInjection
{
    private const string ChatbotSettingsSectionName = "ChatbotSettings";
    private const string OpenAiSettingsSectionName = "OpenAiSettings";

    public static IServiceCollection AddChatbot(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ChatbotSettings>(configuration.GetSection(ChatbotSettingsSectionName));
        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettingsSectionName));

        var openAiSettings = configuration
            .GetSection(OpenAiSettingsSectionName)
            .Get<OpenAiSettings>() ?? new OpenAiSettings();

        var chatbotSettings = configuration
            .GetSection(ChatbotSettingsSectionName)
            .Get<ChatbotSettings>() ?? new ChatbotSettings();

        // Register the plugin in the outer ASP.NET Core scope so that scoped dependencies
        // (ICurrentUser, ISender) resolve from the correct per-request scope.
        services.AddScoped<BookOrbitKernelPlugin>();

        // Register Semantic Kernel with OpenAI chat completion.
        // The Kernel is registered as Scoped so it is recreated per request.
        services.AddScoped<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: chatbotSettings.ModelId,
                apiKey: openAiSettings.ApiKey);

            var kernel = builder.Build();

            // Resolve plugin from the outer ASP.NET Core scope — guarantees the correct
            // ICurrentUser (and ISender) for the current HTTP request.
            var plugin = sp.GetRequiredService<BookOrbitKernelPlugin>();
            kernel.Plugins.AddFromObject(plugin, "BookOrbit");

            return kernel;
        });

        services.AddScoped<IChatbotService, SemanticKernelChatbotService>();
        services.AddScoped<IConversationHistoryService, DistributedCacheConversationHistoryService>();

        return services;
    }
}
