var builder = WebApplication.CreateBuilder(args);

builder.Services.
    AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)

        .Enrich.FromLogContext()
        .Enrich.WithSpan();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}


app.UseCoreMiddlewares(builder.Configuration);
app.MapControllers();
app.MapHub<BookOrbit.Infrastructure.Services.ChatServices.ChatHub>("/chat-hub");


app.Run();