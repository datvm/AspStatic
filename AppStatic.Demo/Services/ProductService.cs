namespace AppStatic.Demo.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> GetAsync(int id);
}

public class ProductService : IProductService
{
    static readonly List<Product> Products = new()
    {
        new(1, "awesome-name", "Awesome Name", "Lorem ipsum dolor sit amet, consectetur adipiscing elit"),
        new(2, "mediocre-name", "Mediocre Name", "Aliquam non enim finibus, vehicula tortor vitae, aliquet odio"),
        new(3, "terrible-name", "Terrible Name", "Cras tempus ullamcorper sem, at scelerisque purus vulputate consectetur"),
    };

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        await Task.CompletedTask;
        return Products;
    }

    public async Task<Product> GetAsync(int id)
    {
        await Task.CompletedTask;

        return Products.First(q => q.Id == id);
    }

}

public record Product(int Id, string Slug, string Name, string Details);