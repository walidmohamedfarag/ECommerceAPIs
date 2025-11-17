using Microsoft.IdentityModel.Tokens;

namespace ECommerceAPI.Utiltie.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDBContext DBContext;
        private readonly ILogger<ApplicationDBContext> logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public DBInitializer(ApplicationDBContext _dBContext , ILogger<ApplicationDBContext> logger ,RoleManager<IdentityRole> _roleManager , UserManager<ApplicationUser> _userManager)
        {
            DBContext = _dBContext;
            this.logger = logger;
            roleManager = _roleManager;
            userManager = _userManager;
        }


        public void Initialize()
        {
            try
            {
                if (DBContext.Database.GetPendingMigrations().Any())
                    DBContext.Database.Migrate();
                if (roleManager.Roles.IsNullOrEmpty())
                {
                    roleManager.CreateAsync(new(StaticRole.SUPER_ADMIN)).GetAwaiter().GetResult();
                    roleManager.CreateAsync(new(StaticRole.ADMIN)).GetAwaiter().GetResult();
                    roleManager.CreateAsync(new(StaticRole.EMPLOYEE)).GetAwaiter().GetResult();
                    roleManager.CreateAsync(new(StaticRole.CUSTOMER)).GetAwaiter().GetResult();
                }

                userManager.CreateAsync(new()
                {
                    Email = "superadmin@gmail.com",
                    FirstName = "super",
                    LastName = "admin",
                    UserName = "superadmin"
                }, "SuperAdmin#12").GetAwaiter().GetResult();
                var user = userManager.FindByEmailAsync("superadmin@gmail.com").GetAwaiter().GetResult();
                userManager.AddToRoleAsync(user!, StaticRole.SUPER_ADMIN).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
            }
        }
    }
}
