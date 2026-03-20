namespace My.Talli.Web.Configuration;

using Domain.Components.Tokens;
using Microsoft.Extensions.Options;
using Web.Services.Email;
using Web.Services.Tokens;

/// <summary>Configuration</summary>
public static class EmailConfiguration
{
    #region <Methods>

    public static void AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailService, AcsEmailService>();
        services.AddExceptionHandler<ExceptionEmailHandler>();

        services.Configure<UnsubscribeTokenSettings>(configuration.GetSection("UnsubscribeToken"));
        services.AddScoped<UnsubscribeTokenService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<UnsubscribeTokenSettings>>().Value;
            return new UnsubscribeTokenService(settings.SecretKey);
        });
    }

    #endregion
}
