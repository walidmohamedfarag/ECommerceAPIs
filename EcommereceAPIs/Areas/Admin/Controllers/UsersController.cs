namespace ECommerceAPI.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(UserManager<ApplicationUser> _userManager)
        {
            userManager = _userManager;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(userManager.Users.Adapt<IEnumerable<UserResponse>>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            if (await userManager.IsInRoleAsync(user, StaticRole.SUPER_ADMIN))
                return BadRequest(new
                {
                    error = "You Can Not Lock Super Admin User"
                });
            user.LockoutEnabled = !user.LockoutEnabled;
            await userManager.UpdateAsync(user);
            return NoContent();
        }

    }
}
