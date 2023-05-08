namespace Microsoft.Extensions.DependencyInjection
{
    public static class AspStaticExtensions
    {

        public static IServiceCollection AddAspStatic(this IServiceCollection services)
            => AddAspStatic(services, null);

        public static IServiceCollection AddAspStatic(this IServiceCollection services, Action<AspStaticOptions>? configure)
        {
            if (configure is not null)
            {
                services.Configure(configure);
            }

            return services
                .AddScoped<IAspStaticService, AspStaticService>();
        }

    }
}

namespace Microsoft.Extensions.Hosting
{
    public static class AspStaticExtensions
    {
        public static IApplicationBuilder UseAspStatic(this IApplicationBuilder app)
            => app.UseMiddleware<AspStaticBuildMiddleware>();
    }
}