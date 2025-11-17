namespace ECommerceAPI.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = $"{StaticRole.SUPER_ADMIN},{StaticRole.ADMIN}")]
    public class CategoriesController : ControllerBase
    {
        private readonly IRepositroy<Categroy> repoCategpry;

        public CategoriesController(IRepositroy<Categroy> _repoCategpry)
        {
            repoCategpry = _repoCategpry;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await repoCategpry.GetAsync(cancellationToken: cancellationToken);
            return Ok(categories);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var category = await repoCategpry.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken, tracked: false);
            if (category is null)
                return NotFound();
            return Ok(category);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Categroy categroy, CancellationToken cancellationToken)
        {
            await repoCategpry.AddAsync(categroy, cancellationToken: cancellationToken);
            await repoCategpry.CommitAsync(cancellationToken);
            return Created(nameof(GetOne) , new {id = categroy.Id});
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id,Categroy categroy, CancellationToken cancellationToken)
        {
            var categoryInDB = await repoCategpry.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (categoryInDB is null)
                return NotFound();
            categoryInDB.Name = categroy.Name;
            categoryInDB.Description = categroy.Description;
            categoryInDB.Status = categroy.Status;
            await repoCategpry.CommitAsync(cancellationToken);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var categroy = await repoCategpry.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (categroy is null)
                return NotFound();
            repoCategpry.Delete(categroy);
            await repoCategpry.CommitAsync(cancellationToken);
            return NoContent();
        }

    }
}
