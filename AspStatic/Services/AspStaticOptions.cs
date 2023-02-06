using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace AspStatic.Services;

public class AspStaticOptions
{

    public string OutputPath { get; set; } = @"bin\aspstatic";
    public bool ZipOutput { get; set; }
    public bool ClearOutput { get; set; }

    public bool CopyWwwroot { get; set; } = true;

    public bool GenerateRazorPages { get; set; } = true;
    public GenerateRazorPagesUrlHandling RazorPagesUrlHandling { get; set; } = GenerateRazorPagesUrlHandling.AlwaysUseIndexHtml;

    public bool GenerateNonSuccessfulPages { get; set; }

    public Func<IServiceProvider, Uri> BaseUriFunc { get; set; } = DefaultBaseUri;

    public Func<IServiceProvider, Task<IEnumerable<PageResource>>>? CustomResources { get; set; }

    public AspStaticOptions OutputToZip(string path)
    {
        OutputPath = path;
        ZipOutput = true;

        return this;
    }

    public AspStaticOptions DoNotUseDefaultRazorPages()
    {
        GenerateRazorPages = false;
        return this;
    }

    public AspStaticOptions UseDefaultRazorPages()
        => UseDefaultRazorPages(RazorPagesUrlHandling);

    public AspStaticOptions UseDefaultRazorPages(GenerateRazorPagesUrlHandling urlHandling)
    {
        GenerateRazorPages = true;
        RazorPagesUrlHandling = urlHandling;

        return this;
    }
    
    static Uri DefaultBaseUri(IServiceProvider services)
    {
        const string Error =
            "Cannot get the Base Uri. IServer wasn't found in the DI. " +
            "If you are not hosting on IIS or Kestrel, please set \"BaseUriFunc\" options.";

        var server = services.GetService<IServer>();
        var serverAddr = server?.Features.Get<IServerAddressesFeature>();
        var address = serverAddr?.Addresses.FirstOrDefault();

        return new Uri(address ?? throw new InvalidOperationException(Error));
    }

    public enum GenerateRazorPagesUrlHandling
    {
        AlwaysUseIndexHtml,
        UseFileNameHtml,
    }

}
