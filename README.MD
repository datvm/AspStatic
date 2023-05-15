`AspStatic` is a simple library to generate static HTML files from ASP.NET Core websites. It simply sends requests to the pages and saves the result to a folder or zipped file that can then be served by any static file server.

# Why another Static Generator?

- You have the full power of ASP.NET Core (database, service, dependency injections for example) and file access at the _runtime_ (when the static files are generated). This means **you don't have to write custom or extra template or layout**, you write the usual ASP.NET website.

- Open abstractions for both `Grabber` and `Writer` so you can add your own implementations.

- A single code base for both a ASP.NET Website and a static website. This package is intended to be run once in your development machine and you can easily "unplug" it from your Release build if you want a dynamic version of your website.

# Installation

Install [`AspStatic` package from Nuget](https://www.nuget.org/packages/AspStatic):

```ps
dotnet add package AspStatic
```

# Usage

See full [demo project](https://github.com/datvm/AspStatic/tree/master/AppStatic.Demo).

1. Call `AddHttpContextAccessor`, `AddHttpClient` and `AddAspStatic` methods to add necessary services to the website's DI. Optionally call `AddAspStaticUrlGatherer` if you want to use it. See [URL Gatherer](#url-gatherer) below.

**Program.cs**
```cs
// ...
builder.Services
    // These 2 are required:
    .AddHttpContextAccessor()
    .AddHttpClient()

    // Optionally add URL Gatherer
    .AddAspStaticUrlGatherer()
    
    .AddAspStatic(options =>
    {
        // By default, AspStaticOptions already has two grabbers:
        // options.GrabWwwRootFiles();
        // options.GrabRazorPages();

        // You must call one Write method or else nothing will be written
        options.WriteToFolder(@"bin\aspstatic", true); // Write to a folder
        options.WriteToZip(@"bin\aspstatic.zip"); // Write to a Zip file

        // Grab some URLs that are not covered by the default grabbers
        // You can even use DI here to access database or other services
        options.GrabCustomUrls(async (ctx) =>
        {
            var services = ctx.RequestServices;

            var result = new List<string>()
            {
                "/AppStatic.Demo.styles.css",
            };

            // Get the product service to list all product pages
            var prodSrv = services.GetRequiredService<IProductService>();
            var prods = await prodSrv.GetAllAsync();

            result.AddRange(prods
                .Select(p => $"/products/{p.Id}"));

            return result;
        });
    })
```

2. Call `app.UseAspStatic();` to add the middleware to the pipeline. This should be called before any resources is served and therefore should be right after the `app.UseHttpsRedirection()` call. You can also call `app.UseAspStaticUrlGatherer()` if you use the URL Gatherer.

```cs
// ...
app.UseHttpsRedirection();

app.UseAspStatic();
app.UseAspStaticUrlGatherer();

app.UseStaticFiles();
// ...
```

> **Note**  
> If you plan to use this as your production website as well, put these code inbetween `#if !DEBUG` and `#endif` or use `app.Environment` to put the code into approriate environments only.

3. Run and access your web app. The first request will be hijacked by the `AspStatic` middleware. If everything runs smoothly, the output would be generated and you should see the response "ASP Static built."

# Grabbers

Grabbers `IGrabber` determine what URLs are requested and saved to the output. The library provides 3 grabbers by default:

- `WwwRootGrabber`: Grab all files in the `wwwroot` folder.

- `RazorPagesGrabber`: Grab all Razor Pages in the website. Only works for pages without parameters in their paths like `/products` or `/info` but _not_ `/products/{id}`..

- `CustomGrabber`: A grabber that you can customize to grab any URLs you want.

> **Note**  
> The default grabbers have an optional `requireSuccessfulResponse` option (**default: `true`**) that determines whether the response must be successful to be saved.

> **Note**  
> `IGrabber.GrabAsync(HttpContext context)` are called at runtime, providing great flexibility to what you can do because you have everything available to you.

The `AspStaticOptions` exposes the `List<IGrabber> Grabbers` so you can freely add or remove grabbers. You can also use the provided methods for quick access to those grabbers.

> **Note**  
> If you want to write your own Grabber, you may find the `BaseUrlGrabber` class useful. 

# Writers

Writers `IWriter` determine how the output is saved. The library provides 2 writers by default:

- `FsWriter`: Write to a folder on your File System. The optional `clearBeforeBuild` option (**default: `true`**) determines whether the folder is cleared before writing.

  - The `WriteToFolder()` method writes to the folder relative to the web project (`env.ContentRootPath`).
  
  - To write to an absolute folder, use `WriteToAbsoluteFolder` or the more generic `WriteToFolder(Func<HttpContext, string> folderFunc, bool clearBeforeBuild)` method.

> **Note**  
> A decision was made to write path like `/product/2` to `/product/2/index.html` _instead of_ `/product/2.html` for cases when the website contains paths like `/product/2/details` as well. Any path that the end part has a dot `.` (like static files, `/css/index.css`) are kept and no folder is made.

- `ZipWriter`: Write to a Zip file. The optional `relativeToContentRootPath` option (**default: `true`**) determines whether the provided path is relative to the web project (`env.ContentRootPath`).

## Implementing your own writer

You can implement your own writer by implementing the `IWriter` interface:

```cs
public interface IWriter
{
    Task InitializeAsync(HttpContext context);
    Task<Stream> GetOutputStreamAsync(string path);
    // ...
}
```

- `InitializeAsync` is called once before the first write. You can use this to create the output folder or file or get a relative folder resolved because an `HttpContext` instance is available to you.

- `GetOutputStreamAsync(string path)` asks you to provide a `Stream` so the library can write the content to it. `path` is the path part of the URL, for example `/css/app.css` or `/product/2`.

# URL Gatherer

The **URL Gatherer** is a separate component to the generator that simply logs all requests to the web app. Sometimes it may be difficult to gather all the URLs that you want to generate. For example, Blazor apps may request additional files from the framework.

> **Note**  
> The gatherer is simply a logger. Once you have the list, you have to manually put it to any use you want, for example feed it to `GrabCustomUrls`. You can write it to a file since `GrabCustomUrls` support `async` as well as all services you may need.

You need to call `builder.Services.AddAspStaticUrlGatherer()` and `app.UseAspStaticUrlGatherer()` to activate the URL Gatherer. This make an `IUrlGathererService` singleton instance available to your DI:

```cshtml
@inject AspStatic.IUrlGathererService Urls;

@* ... *@

@foreach (var item in Urls.GetGatheredUrls())
{
    <tr>
        <td>@(item.Url)</td>
        <td>@(item.StatusCode is null ? "N/A" : (int)item.StatusCode)</td>
    </tr>
}
```