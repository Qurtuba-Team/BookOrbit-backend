namespace BookOrbit.Api;
static public class DependencyInjection
{
    private const string AppSettingsSectionName = "AppSettings";
    private const string CacheSettingsSectionName = "CacheSettings";
    private const string EmailSettingsSectionName = "EmailSettings";
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSettings(configuration)
            .AddControllerWithJsonConfiguration()
            .AddIdentityInfrastructure()
            .AddCustomProblemDetails()
            .AddAuthentication(configuration)
            .AddAppRateLimiting()
            .AddConfiguredCors(configuration)
            .AddExceptionHandling()
            .AddAppOutputCaching(configuration)
            .AddHybridCaching(configuration)
            .AddCustomApiVersioning()
            .AddAppOpenTelemetry()
            .AddAppHealthChecks()
            .AddApiDocumentation()
            .AddPresentationServices();

        return services;
    }
    public static IServiceCollection AddSettings(this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection(AppSettingsSectionName));
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettingsSectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettingsSectionName));
        return services;
    }
    public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options => {
            
            options
            .JsonSerializerOptions
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        return services;
    }
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }
    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = (context) =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        });

        return services;
    }
    static private IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            };
        });

        return services;
    }
    static private IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            RateLimitHelper.AddSlidingPolicy(
                options,
                ApiConstants.NormalRateLimitingPolicyName,
                ApiConstants.NormalRateLimiteMaxRequests,
                ApiConstants.NormalRateLimitWindowSpanInMinutes,
                ApiConstants.NormalRateLimitSegmentPerWindow,
                ApiConstants.NormalRateLimitQueueLimit);

            RateLimitHelper.AddSlidingPolicy(
                options,
                ApiConstants.SensitiveRateLimmitingPolicyName,
                ApiConstants.SensistiveRateLimiteMaxRequests,
                ApiConstants.SensistiveRateLimitWindowSpanInMinutes,
                ApiConstants.SensistiveRateLimitSegmentPerWindow,
                ApiConstants.SensistiveRateLimitQueueLimit);

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
    static private IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.GetSection(AppSettingsSectionName).Get<AppSettings>()!;

        services.AddCors(options => options.AddPolicy(
            appSettings.CorsPolicyName,
            policy => policy
                .WithOrigins(appSettings.AllowedOrigins!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
    static public IServiceCollection AddAppOutputCaching(this IServiceCollection services , IConfiguration configuration)
    {
        var cacheSettings = configuration.GetSection(CacheSettingsSectionName).Get<CacheSettings>()!;

        services.AddOutputCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100 mb
            options.AddPolicy(ApiConstants.DefaultOutputCachePolicyName, policy =>
                policy.Expire(TimeSpan.FromSeconds(cacheSettings.OutputCachExpirationInSeconds)));
        });

        return services;
    }
    static private IServiceCollection AddHybridCaching(this IServiceCollection services,IConfiguration configuration)
    {
        var cacheSettings = configuration.GetSection(CacheSettingsSectionName).Get<CacheSettings>()!;

        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(cacheSettings.RemoteCachExpirationInMinutes), //Remote
            LocalCacheExpiration = TimeSpan.FromSeconds(cacheSettings.LocalCachExpirationInSeconds), // Local
        });

        return services;
    }
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;//Doesnt work with URL segment
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("bookorbit", serviceVersion: "1.0.0")
                .AddAttributes(new[]
                {
                new KeyValuePair<string, object>("environment", "dev")
                }))

            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new AlwaysOnSampler())  // dev only
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://jaeger:4317");
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter();
            });

        return services;
    }
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1"];

        foreach (var version in versions)
        {
            services.AddOpenApi(version, options =>
            {
                // Versioning config
                options.AddDocumentTransformer<VersionInfoTransformer>();

                // Security Scheme config
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
            });
        }

        return services;
    }
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddScoped<ImageHelper>();
        return services;
    }

    public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        var appSettings = configuration.GetSection(AppSettingsSectionName).Get<AppSettings>()!;

        app.UseMiddleware<RequestLogContextMiddleware>();

        app.UseHealthChecks("/health");

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.UseCors(appSettings.CorsPolicyName);

        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseOutputCache();

        return app;
    }

}