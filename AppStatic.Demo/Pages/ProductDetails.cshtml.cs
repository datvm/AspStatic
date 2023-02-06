using AppStatic.Demo.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppStatic.Demo.Pages;

public class ProductDetailsModel : PageModel
{

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Product Product { get; private set; } = null!;

    readonly IProductService productSrv;
    public ProductDetailsModel(IProductService productSrv)
    {
        this.productSrv = productSrv;
    }

    public async Task OnGetAsync()
    {
        Product = await productSrv.GetAsync(Id);
    }
    
}
