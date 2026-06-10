using ERP.API.Configurations;

namespace ERP.API.Extensions
{
    public static class AddAppSettingsExtension
    {
        public static void AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtConfig>(configuration.GetSection(nameof(JwtConfig)));
            services.Configure<SmtpConfig>(configuration.GetSection(nameof(SmtpConfig)));
            services.Configure<OllamaConfig>(configuration.GetSection(nameof(OllamaConfig)));
            services.Configure<QdrantConfig>(configuration.GetSection(nameof(QdrantConfig)));
        }
    }
}
