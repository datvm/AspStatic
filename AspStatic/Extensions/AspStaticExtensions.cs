namespace Microsoft.Extensions.DependencyInjection
{
    public static class AspStaticExtensions
    {

        public static IServiceCollection AddAspStatic(this IServiceCollection services) =>
            AddAspStatic(services, null);

        public static IServiceCollection AddAspStatic(this IServiceCollection services, Action<AspStaticOptions>? configure)
        {
            if (configure is not null)
            {
                services.Configure(configure);
            }

            return services
                .AddScoped<IAspStaticService, AspStaticService>();
        }

        public static IServiceCollection AddAspStaticUrlGatherer(this IServiceCollection services) =>
            services.AddSingleton<IUrlGathererService, UrlGathererService>();

    }
}

namespace Microsoft.Extensions.Hosting
{
    public static class AspStaticExtensions
    {
        public static IApplicationBuilder UseAspStatic(this IApplicationBuilder app)
            => app.UseMiddleware<AspStaticBuildMiddleware>();

        public static IApplicationBuilder UseAspStaticUrlGatherer(this IApplicationBuilder app)
        {
            if (app.ApplicationServices.GetService<IUrlGathererService>() is null)
            {
                throw new InvalidOperationException($"No {nameof(IUrlGathererService)} instance found in the DI. " +
                    $"Call {nameof(DependencyInjection.AspStaticExtensions.AddAspStaticUrlGatherer)} method to register one.");
            }

            return app.UseMiddleware<AspStaticUrlGathererMiddleware>();
        }
    }
}