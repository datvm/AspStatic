using AppStatic.Demo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Other services used by your site
builder.Services
    .AddScoped<IProductService, ProductService>();

builder.Services
    .AddHttpContextAccessor()
    .AddAspStatic(options =>
    {
        options.WriteToFolder(@"bin\aspstatic", true);
        options.WriteToZip(@"bin\aspstatic.zip");

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