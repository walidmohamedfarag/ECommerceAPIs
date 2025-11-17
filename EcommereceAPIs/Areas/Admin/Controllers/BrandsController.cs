using ECommerceAPI.DTOs.Request;
using Mapster;

namespace ECommerceAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN} , {StaticRole.ADMIN}")]
    [Area("Admin")]
    public class BrandsController : ControllerBase
    {
        private readonly IRepositroy<Brand> repoBrand;

        public BrandsController(IRepositroy<Brand> _repoBrand)
        {
            repoBrand = _repoBrand;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var brands = await repoBrand.GetAsync(tracked: false, cancellationToken: cancellationToken);
            return Ok(brands);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var brand = await repoBrand.GetOneAsync(b => b.Id == id, cancellationToken: cancellationToken);
            if (brand is null)
                return NotFound();
            return Ok(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandRequest createBrandRequest, CancellationToken cancellationToken)
        {
            Brand brand = createBrandRequest.Adapt<Brand>();
            if (createBrandRequest.Image is not null && createBrandRequest.Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBrandRequest.Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\BrandImg", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    createBrandRequest.Image.CopyTo(stream);
                }
                brand.Image = fileName;
            }
            await repoBrand.AddAsync(brand, cancellationToken: cancellationToken);
            await repoBrand.CommitAsync(cancellationToken);
            return Created(nameof(GetOne) , new { id = brand.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, UpdateBrandRequest updateBrandRequest, CancellationToken cancellationToken)
        {
            var brandInDb = await repoBrand.GetOneAsync(b => b.Id == id, cancellationToken: cancellationToken);
            if (brandInDb is null)
                return NotFound();
            if (updateBrandRequest.Image is not null && updateBrandRequest.Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateBrandRequest.Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\BrandImg", fileName);
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\BrandImg", brandInDb.Image);
                using (var stream = System.IO.File.Create(filePath))
                {
                    updateBrandRequest.Image.CopyTo(stream);
                }
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                brandInDb.Image = fileName;
            }
            brandInDb.Name = updateBrandRequest.Name;
            brandInDb.Description = updateBrandRequest.Description;
            brandInDb.Status = updateBrandRequest.Status;
            await repoBrand.CommitAsync(cancellationToken);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var brand = await repoBrand.GetOneAsync(b => b.Id == id);
            if (brand is null)
                return NotFound();
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images\BrandImg", brand.Image);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            repoBrand.Delete(brand);
            await repoBrand.CommitAsync(cancellationToken);
            return NoContent();
        }

    }
}
