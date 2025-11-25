

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerceAPI
{
    public static class AppConfiguration
    {
        public static void RegisterConfig(this IServiceCollection services , string connection)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Identity/Register/Login";
                option.AccessDeniedPath = "/Identity/Profile/AccessDenied";

            });
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IRepositroy<Brand>, Repositroy<Brand>>();
            services.AddScoped<IRepositroy<Categroy>, Repositroy<Categroy>>();
            services.AddScoped<IRepositroy<Product>, Repositroy<Product>>();
            services.AddScoped<IRepositroy<ProductSubImage>, Repositroy<ProductSubImage>>();
            services.AddScoped<IRepositroy<Cart>, Repositroy<Cart>>();
            services.AddScoped<IRepositroy<ProductColor>, Repositroy<ProductColor>>();
            services.AddDbContext<ApplicationDBContext>(option =>
            {
                option.UseSqlServer(connection);
            });
            services.AddScoped<IRepositroy<ApplicationUserOtp>, Repositroy<ApplicationUserOtp>>();
            services.AddScoped<IRepositroy<Promotion>, Repositroy<Promotion>>();
            services.AddScoped<IDBInitializer, DBInitializer>();
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "https://localhost:7218",
                        ValidAudience = "https://localhost:7218",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yyuysuyiusdussdjncncxmnmncxmnjdskjskjklaklksakllasklklasklasad"))
                    };
                });
        }
    }
}
