using ECommerceAPI.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN} , {StaticRole.ADMIN}")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        IRepositroy<Product> productRepo;
        IRepositroy<Categroy> categoryRepo;
        IRepositroy<Brand> brandRepo;
        IRepositroy<ProductColor> productColorRepo;
        IRepositroy<ProductSubImage> subImageRepo;
        public ProductsController(ApplicationDBContext _context, IRepositroy<Product> _productRepo, IRepositroy<ProductSubImage> _subImageRepo, IRepositroy<Brand> _brandRepo, IRepositroy<ProductColor> _productColorRepo, IRepositroy<Categroy> _categoryRepo)
        {
            context = _context;
            productRepo = _productRepo;
            subImageRepo = _subImageRepo;
            brandRepo = _brandRepo;
            productColorRepo = _productColorRepo;
            categoryRepo = _categoryRepo;
        }
        [HttpPost("GetAll/{page}")]
        public async Task<IActionResult> GetAll(FilterProductRequest filterProduct, CancellationToken cancellationToken, int page = 1)
        {
            #region filter section
            var product = await productRepo.GetAsync(includes: [p => p.Categroy, p => p.Brand], cancellationToken: cancellationToken);
            if (filterProduct.name is not null)
                product = await productRepo.GetAsync(p => p.Name.Contains(filterProduct.name), cancellationToken: cancellationToken);
            if (filterProduct.minPrice is not null)
                product = await productRepo.GetAsync(p => p.Price - (p.Price * (p.Discount / 100)) >= filterProduct.minPrice, cancellationToken: cancellationToken);
            if (filterProduct.maxPrice is not null)
                product = await productRepo.GetAsync(p => p.Price - (p.Price * (p.Discount / 100)) <= filterProduct.maxPrice, cancellationToken: cancellationToken);
            if (filterProduct.categoryId is not null)
                product = await productRepo.GetAsync(p => p.CategroyId == filterProduct.categoryId, cancellationToken: cancellationToken);
            if (filterProduct.brandId is not null)
                product = await productRepo.GetAsync(p => p.BrandId == filterProduct.brandId, cancellationToken: cancellationToken);
            if (filterProduct.lessQuantity is not null)
                product = await productRepo.GetAsync(p => p.Quantity <= 100, cancellationToken: cancellationToken);
            #endregion

            // filter response
            FilterProductResponse filterProductResponse = new(filterProduct.name, filterProduct.minPrice, filterProduct.maxPrice, filterProduct.categoryId, filterProduct.brandId, filterProduct.lessQuantity);
            //pagination response
            PaginationResponse paginationResponse = new PaginationResponse();
            paginationResponse.TotalPage = Math.Ceiling(product.Count() / 8.0);
            paginationResponse.CurrentPage = page;
            product = product.Skip((page - 1) * 8).Take(8);
            // return result
            return Ok(new
            {
                Product = product,
                PaginationResponse = paginationResponse,
                FilterProductResponse = filterProductResponse
            });
        }
        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var product = await productRepo.GetOneAsync(b => b.Id == id , includes: [p=>p.Categroy , p => p.Brand], cancellationToken: cancellationToken);
            if (product is null)
                return NotFound();
            var prductSubImgs = await subImageRepo.GetAsync(p => p.ProductId == id, cancellationToken: cancellationToken);
            var productColors = await productColorRepo.GetAsync(pc => pc.ProductId == id, cancellationToken: cancellationToken);

            return Ok(new ProductResponse
            {
                product = product,
                ProductSubImages = prductSubImgs,
                productColors = productColors
            });
        }
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateProductRequest createProductRequest, CancellationToken cancellationToken)
        {
            Product product = createProductRequest.Adapt<Product>();
            #region add product with main image
            if (createProductRequest.Img is not null && createProductRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createProductRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    createProductRequest.Img.CopyTo(stream);
                }
                product.MainImage = fileName;
            }
            await productRepo.AddAsync(product, cancellationToken: cancellationToken);
            await productRepo.CommitAsync();
            #endregion

            #region add sub imges
            if (createProductRequest.SubImgs is not null && createProductRequest.SubImgs.Count > 0)
            {
                foreach (var simg in createProductRequest.SubImgs)
                {
                    if (simg.Length > 0)
                    {
                        var imgName = Guid.NewGuid().ToString() + Path.GetExtension(simg.FileName);
                        var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", imgName);
                        using (var stream = System.IO.File.Create(imgPath))
                        {
                            simg.CopyTo(stream);
                        }
                        await subImageRepo.AddAsync(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = product.Id
                        }, cancellationToken: cancellationToken);
                    }
                }
                await subImageRepo.CommitAsync();
            }
            #endregion

            #region add colors
            if (createProductRequest.Colors is not null && createProductRequest.Colors.Length > 0)
            {
                foreach (var color in createProductRequest.Colors)
                {
                    await productColorRepo.AddAsync(new ProductColor
                    {
                        Color = color,
                        ProductId = product.Id
                    }, cancellationToken);
                }
                await productColorRepo.CommitAsync();
            }
            #endregion

            return Created(nameof(GetOne), new { id = product.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id , UpdateProductRequest updateProductRequest, CancellationToken cancellationToken)
        {
            #region edit main imge
            var oldProduct = await productRepo.GetOneAsync(p => p.Id == id, cancellationToken: cancellationToken);
            if(oldProduct is null)
                return NotFound();
            if (updateProductRequest.Img is not null && updateProductRequest.Img.Length > 0)
            {
                var imgName = Guid.NewGuid().ToString() + Path.GetExtension(updateProductRequest.Img.FileName);
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", imgName);
                using (var stream = System.IO.File.Create(imgPath))
                {
                    updateProductRequest.Img.CopyTo(stream);
                }
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", oldProduct.MainImage);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                oldProduct.MainImage = imgName;
            }
            #endregion


            #region update items name , status.....etc
            oldProduct.Name = updateProductRequest.Name;
            oldProduct.Description = updateProductRequest.Description;
            oldProduct.Price = updateProductRequest.Price;
            oldProduct.Status = updateProductRequest.Status;
            oldProduct.Quantity = updateProductRequest.Quantity;
            oldProduct.Discount = updateProductRequest.Discount;
            oldProduct.BrandId = updateProductRequest.BrandId;
            oldProduct.CategroyId = updateProductRequest.CategroyId;
            await productRepo.CommitAsync();
            #endregion

            #region edit sub imges
            if (updateProductRequest.SubImgs is not null && updateProductRequest.SubImgs.Count > 0)
            {
                var editSubImgs = await subImageRepo.GetAsync(p => p.ProductId == id, cancellationToken: cancellationToken);
                foreach (var oldSubImg in editSubImgs)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", oldSubImg.Imge);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                    subImageRepo.Delete(oldSubImg);
                }
                await subImageRepo.CommitAsync();
                foreach (var simg in updateProductRequest.SubImgs)
                {
                    if (simg.Length > 0)
                    {
                        var imgName = Guid.NewGuid().ToString() + Path.GetExtension(simg.FileName);
                        var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", imgName);
                        using (var stream = System.IO.File.Create(imgPath))
                        {
                            simg.CopyTo(stream);
                        }
                        await subImageRepo.AddAsync(new ProductSubImage
                        {
                            Imge = imgName,
                            ProductId = oldProduct.Id
                        }, cancellationToken);
                    }
                }
                await subImageRepo.CommitAsync();
            }
            #endregion

            #region edit colors
            if (updateProductRequest.Colors is not null && updateProductRequest.Colors.Length > 0)
            {
                var oldColors = await productColorRepo.GetAsync(pc => pc.ProductId == id, cancellationToken: cancellationToken);
                foreach (var oldColor in oldColors)
                    productColorRepo.Delete(oldColor);
                await productColorRepo.CommitAsync(cancellationToken);
                foreach (var color in updateProductRequest.Colors)
                {
                    await productColorRepo.AddAsync(new ProductColor
                    {
                        Color = color,
                        ProductId = oldProduct.Id
                    }, cancellationToken);
                }
                await productColorRepo.CommitAsync(cancellationToken);
            }
            #endregion

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var product = await productRepo.GetOneAsync(b => b.Id == id, cancellationToken: cancellationToken);
            if (product is null)
                return NotFound();
            var subimgs = await subImageRepo.GetAsync(p => p.ProductId == product.Id, cancellationToken: cancellationToken);
            var productColors = await productColorRepo.GetAsync(p => p.ProductId == id, cancellationToken: cancellationToken);
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", product.MainImage);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            if (subimgs is not null)
            {
                foreach (var subimg in subimgs)
                {
                    var oldSubPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\ProductImg", subimg.Imge);
                    if (System.IO.File.Exists(oldSubPath))
                        System.IO.File.Delete(oldSubPath);
                    subImageRepo.Delete(subimg);
                }
                await subImageRepo.CommitAsync(cancellationToken);
            }
            if (productColors is not null)
            {
                foreach (var color in productColors)
                    productColorRepo.Delete(color);
                await productColorRepo.CommitAsync(cancellationToken);
            }
            productRepo.Delete(product);
            await productRepo.CommitAsync(cancellationToken);
            return NoContent();
        }
        [HttpDelete("{productId}/{img}")]
        public async Task<IActionResult> DeleteSubImg(int productId, string img, CancellationToken cancellationToken)
        {
            //get subimg from db
            var subimg = await subImageRepo.GetOneAsync(p => p.ProductId == productId && p.Imge == img, cancellationToken: cancellationToken);
            //get old path of subimg
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\ProductImg", img);
            if (subimg is null)
                return NotFound();
            //delete subimg from wwwroot
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
            //delete subimg from db
            subImageRepo.Delete(subimg);
            await subImageRepo.CommitAsync(cancellationToken);
            return NoContent();
        }

    }
}
