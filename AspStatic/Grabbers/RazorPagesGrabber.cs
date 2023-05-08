using Microsoft.AspNetCore.Routing;

namespace AspStatic.Grabbers;

public class RazorPagesGrabber : BaseUrlGrabber
{
    const string SelfPath = "aspstatic";

    protected override async IAsyncEnumerable<Uri> GetUrls(HttpContext context)
    {
        await Task.CompletedTask;

        var endpointSource = context.RequestServices
            .GetRequiredService<EndpointDataSource>();

        foreach (var ep in endpointSource.Endpoints)
        {
            if (ep is not RouteEndpoint rEp) { continue; }

            var raw = rEp.RoutePattern.RawText;
            if (raw is null ||
                raw.Contains('{') ||
                raw.EndsWith("/index", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("index", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals(SelfPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (raw.Length == 0)
            {
                raw = "/index";
            }

            var path = raw.StartsWith('/') ? raw : '/' + raw;
            yield return new(path, UriKind.RelativeOrAbsolute);
        }
    }
}
