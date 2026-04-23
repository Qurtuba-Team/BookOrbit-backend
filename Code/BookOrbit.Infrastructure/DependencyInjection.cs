

using BookOrbit.Application.Common.Constants;
using BookOrbit.Application.Common.Interfaces.ImageServices;
using BookOrbit.Infrastructure.Services.ImageServices;

namespace BookOrbit.Infrastructure;
static public class DependencyInjection
{
    static public IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        return services
            .AddDbContext(configuration)
            .AddIdentity()
            .AddInfrastructureServices()
            .AddPolicies();
    }
    static private IServiceCollection AddDbContext(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());


        return services;
    }
    static private IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services
        .AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 4;


            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                " " +
                "ءآأؤإئابةتثجحخدذرزسشصضطظعغفقكلمنهوىي";

        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
    static private IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ITokenProvider, TokenProvider>();
        services.AddTransient<IMaskingService, MaskingService>();
        services.AddTransient<IAppCache, AppCache>();
        services.AddScoped<AppDbContextInitialiser>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IStudentQueryService, StudentQueryService>();
        services.AddTransient<IEmailConfirmationService, EmailConfirmationService>();
        services.AddTransient<IRouteService, RouteService>();
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IEmailFormatService, EmailFormatService>();
        services.AddTransient<IBookImageService, BookImageService>();
        services.AddTransient<IStudentImageService, StudentImageService>();
        return services;
    }
    static private IServiceCollection AddPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler,ActiveUserHandler>();
        services.AddScoped<IAuthorizationHandler,AdminOnlyHandler>();
        services.AddScoped<IAuthorizationHandler,StudentOwnerShipHandler>();
        services.AddScoped<IAuthorizationHandler,RegisteredUserHandler>();
        services.AddScoped<IAuthorizationHandler,RegisteredUserOwnershipHandler>();
        services.AddScoped<IAuthorizationHandler,StudentOnlyHandler>();
        services.AddScoped<IAuthorizationHandler, ActiveStudentHandler>();

        services.AddAuthorizationBuilder()

            .AddPolicy(PoliciesNames.ActiveUserPolicy, policy =>
            policy.Requirements.Add(new ActiveUserRequirement()))

            .AddPolicy(PoliciesNames.AdminOnlyPolicy, policy => {
            policy.Requirements.Add(new ActiveUserRequirement());
            policy.Requirements.Add(new AdminOnlyRequirement());
            })

            .AddPolicy(PoliciesNames.StudentOwnershipPolicy,policy =>
            {
                policy.Requirements.Add(new RegisteredUserRequirement());
                policy.Requirements.Add(new StudentOwnerShipRequirement());
            })

            .AddPolicy(PoliciesNames.RegisteredUserPolicy,policy =>
            {
                policy.Requirements.Add(new RegisteredUserRequirement());
            })

            .AddPolicy(PoliciesNames.RegisteredUserOwnershipPolicy,policy =>
            {
                policy.Requirements.Add(new RegisteredUserRequirement());
                policy.Requirements.Add(new RegisteredUserOwnershipRequirement());
            })

            .AddPolicy(PoliciesNames.StudentOnlyPolicy,policy =>
            {
                policy.Requirements.Add(new RegisteredUserRequirement());
                policy.Requirements.Add(new StudentOnlyRequirement());
            })


            .AddPolicy(PoliciesNames.ActiveStudentPolicy,policy =>
            {
                policy.Requirements.Add(new RegisteredUserRequirement());
                policy.Requirements.Add(new ActiveStudentRequirement());

            })
            ;

        return services;
    }

}
