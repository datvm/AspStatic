using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text;
using static AspStatic.AspStaticOptions;

namespace AspStatic.Services;

public interface IAspStaticService
{
    Task BuildStaticAssets(IEnumerable<PageResource> resources, AspStaticOptions? o);
    Task BuildStaticAssets(IEnumerable<PageResource> resources);
    Task BuildStaticAssets(IEnumerable<PageResource> resources, AspStaticOptions? o, StringBuilder? report);
    Task<IEnumerable<PageResource>> GetScanRequestAsync(AspStaticOptions? options);
    Task<IEnumerable<PageResource>> GetScanRequestAsync();
}

class AspStaticService : IAspStaticService
{
    const string SelfPath = "aspstatic";

    readonly IOptions<AspStaticOptions> o;
    readonly IHostEnvironment env;
    readonly EndpointDataSource epSrc;
    readonly IServiceProvider services;
    readonly IHttpClientFactory httpFac;

    ZipArchive? currZipArchive;
    public AspStaticService(IOptions<AspStaticOptions> o, IHostEnvironment env, EndpointDataSource epSrc, IServiceProvider services, IHttpClientFactory httpFac)
    {
        this.o = o;
        this.env = env;
        this.epSrc = epSrc;
        this.services = services;
        this.httpFac = httpFac;
    }

    public Task<IEnumerable<PageResource>> GetScanRequestAsync()
        => GetScanRequestAsync(null);

    public async Task<IEnumerable<PageResource>> GetScanRequestAsync(AspStaticOptions? o)
    {
        o ??= this.o.Value;
        var host = o.BaseUriFunc(services);

        var resources = new List<PageResource>();

        if (o.GenerateRazorPages)
        {
            resources.AddRange(GetPagesPaths(host, o.RazorPagesUrlHandling));
        }

        if (o.CustomResources is not null)
        {
            var res = await o.CustomResources(services);
            resources.AddRange(res
                .Select(q => new PageResource(
                    new Uri(host, q.Url).AbsoluteUri,
                    q.FilePath)));
        }

        if (o.CopyWwwroot)
        {
            resources.AddRange(GetWwwRootPaths(host));
        }

        return resources;
    }

    public async Task BuildStaticAssets(IEnumerable<PageResource> resources)
        => await BuildStaticAssets(resources, null, null);

    public async Task BuildStaticAssets(IEnumerable<PageResource> resources, AspStaticOptions? o)
        => await BuildStaticAssets(resources, o, null);

    public async Task BuildStaticAssets(IEnumerable<PageResource> resources, AspStaticOptions? o, StringBuilder? report)
    {
        o ??= this.o.Value;

        if (o.ClearOutput)
        {
            if (!o.ZipOutput && Directory.Exists(o.OutputPath))
            {
                report?.AppendLine("Clearing output: " + o.OutputPath);

                Directory.Delete(o.OutputPath, true);
            }
        }

        if (o.ZipOutput)
        {
            // Don't add using here
            var file = File.Open(o.OutputPath, FileMode.Create);
            currZipArchive = new ZipArchive(file, ZipArchiveMode.Create); ;
        }

        var http = httpFac.CreateClient();

        foreach (var r in resources)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, r.Url);
            using var res = await http.SendAsync(req);

            report?.AppendLine($"{(int)res.StatusCode,3} - {r.Url}");

            if (!res.IsSuccessStatusCode &&
                !o.GenerateNonSuccessfulPages)
            {
                continue;
            }

            var content = await res.Content.ReadAsStringAsync();
            await PrintOutputAsync(r.FilePath, content, o);
        }

        currZipArchive?.Dispose();
        currZipArchive = null;
    }

    async Task PrintOutputAsync(string path, string content, AspStaticOptions o)
    {
        if (path[0] == '/') { path = path[1..]; }

        Stream outStream;
        if (currZipArchive is null)
        {
            var outPath = Path.Combine(o.OutputPath, path);
            var outFolder = Path.GetDirectoryName(outPath);
            if (outFolder is not null)
            {
                Directory.CreateDirectory(outFolder);
            }

            outStream = File.OpenWrite(
                Path.Combine(o.OutputPath, path));
        }
        else
        {
            var entry = currZipArchive.CreateEntry(path);
            outStream = entry.Open();
        }

        using (outStream)
        {
            using var writer = new StreamWriter(outStream);
            await writer.WriteAsync(content);
        }
    }

    IEnumerable<PageResource> GetWwwRootPaths(Uri? customHost)
    {
        var wwwroot = new DirectoryInfo(Path.Combine(env.ContentRootPath, "wwwroot"));
        if (!wwwroot.Exists)
        {
            yield break;
        }

        foreach (var file in GetFiles(wwwroot, "/"))
        {
            yield return file;
        }

        IEnumerable<PageResource> GetFiles(DirectoryInfo folder, string currPath)
        {
            var subFolders = folder.GetDirectories();
            foreach (var subFolder in subFolders)
            {
                foreach (var file in
                    GetFiles(subFolder, currPath + subFolder.Name + "/"))
                {
                    yield return file;
                }
            }

            var files = folder.GetFiles();
            foreach (var file in files)
            {
                var filePath = currPath + file.Name;

                yield return new(
                    GetUrl(customHost, filePath),
                    filePath);
            }
        }
    }

    IEnumerable<PageResource> GetPagesPaths(Uri? customHost, GenerateRazorPagesUrlHandling urlHandling)
    {
        foreach (var ep in epSrc.Endpoints)
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

            var filePath = path;
            if (filePath.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".html";
            }
            else
            {
                filePath = path + urlHandling switch
                {
                    GenerateRazorPagesUrlHandling.AlwaysUseIndexHtml => "/index.html",
                    GenerateRazorPagesUrlHandling.UseFileNameHtml => ".html",
                    _ => throw new NotImplementedException(),
                };
            }

            yield return new(
                GetUrl(customHost, path),
                filePath);
        }
    }

    static string GetUrl(Uri? baseUri, string path)
        => baseUri is null ?
            path :
            new Uri(baseUri, path).AbsoluteUri;

}

public record PageResource(string Url, string FilePath);