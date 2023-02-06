using AppStatic.Demo.Services;
using AspStatic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Other services used by your site
builder.Services
    .AddScoped<IProductService, ProductService>();

builder.Services
    .AddAspStatic(config =>
    {
        config.GenerateNonSuccessfulPages = true;
        config.ClearOutput = true;

        config.CustomResources = async (services) =>
        {
            var result = new List<PageResource>
            {
                new PageResource("/AppStatic.Demo.styles.css", "AppStatic.Demo.styles.css"),
            };

            // Get the product service to list all product pages
            var prodSrv = services.GetRequiredService<IProductService>();
            var prods = await prodSrv.GetAllAsync();

            result.AddRange(prods.Select(p => new PageResource(
                $"/products/{p.Id}",
                $"products/{p.Id}.html"
            )));

            return result;
        };
    })
    .AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // For testing
    app.Use((ctx, next) =>
    {
        ctx.Response.Headers.Add("Cache-Control", "no-store");

        return next();
    });
}

app.UseHttpsRedirection();

app.UseAspStatic();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();