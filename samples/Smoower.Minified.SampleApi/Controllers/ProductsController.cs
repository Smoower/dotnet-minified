using FluentValidation;

namespace Smoower.Minified.SampleApi;

[Crud<Product, ProductIn, ProductOut>("api/products")]
public partial class ProductsController(AppDb db, IValidator<ProductIn> val)
{
    [HG]public Tr All()=>db.Products.nt().w(x=>x.Price>0).ob(x=>x.Name).s(x=>new ProductOut(x.Id,x.Name,x.Price)).okl();
}

public record ProductIn(string Name,decimal Price);
public record ProductOut(int Id,string Name,decimal Price);

public class ProductInValidator:MiniValidator<ProductIn>{
 public ProductInValidator(){
  req(x=>x.Name).max(100);
  rule(x=>x.Price).gt(0m);
 }
}
