using AppStatic.Demo.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppStatic.Demo.Pages;

partial class Products : PageModel
{

    public IEnumerable<Product> ProductsList { get; private set; } = null!;

    readonly IProductService productSrv;
    public Products(IProductService productSrv)
    {
        this.productSrv = productSrv;
    }

    public async Task OnGetAsync()
    {
        ProductsList = await productSrv.GetAllAsync();
    }

}
