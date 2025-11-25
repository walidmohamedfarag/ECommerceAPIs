using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ECommerceAPI.Areas.Identity.Controllers
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IRepositroy<ApplicationUserOtp> applicationOtpRepo;

        public AccountController(UserManager<ApplicationUser> _userManager, IEmailSender _emailSender, SignInManager<ApplicationUser> _signManager , IRepositroy<ApplicationUserOtp> _applicationOtpRepo)
        {
            userManager = _userManager;
            emailSender = _emailSender;
            signInManager = _signManager;
            applicationOtpRepo = _applicationOtpRepo;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest register)
        {
            Random random = new Random();
            var user = new ApplicationUser
            {
                UserName = $"{register.FirstName.ToLower().TrimEnd()}{register.LastName.ToLower()}{random.Next(0, 11)}",
                Email = register.EmailAddress,
                FirstName = register.FirstName,
                LastName = register.LastName
            };
            var result = await userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(EmailConfirmation), "Register", new { area = "Identity", token, userId = user.Id }, Request.Scheme);
            await emailSender.SendEmailAsync(register.EmailAddress, "ECommerce Confirm Email", $"<h1> To Confirm Your Email Click <a href='{link}'>Here</a></h1>");
            return Ok(new
            {
                success = "User Registration successfully"
            });
        }
        [HttpPost("EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation(string token, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            //هنا نبق نعمل اتشيك لو اليوزر مش موجود ولا لا ونطبع توستر
            if (user is null)
                return NotFound(new
                {
                    error = "User Not Found"
                });
            var result = await userManager.ConfirmEmailAsync(user, token);
            //هنا نبق نعمل اتشيك لو الايميل كونفرمد ولا لا ونطبع توستر
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok(new
            {
                success = "Email Confirmed Successfully"
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user is null)
                return NotFound(new
                {
                    error = "User Not Found"
                });
            if (!user.EmailConfirmed)
                return BadRequest(new
                {
                    error = "Email must be Confirmed Befor Login"
                });
            var checkPass = await signInManager.PasswordSignInAsync(user, login.Password, true, true);
            if (!checkPass.Succeeded)
            {
                if (checkPass.IsLockedOut)
                    return BadRequest(new
                    {
                        error = "Your Blocked"
                    });
                else
                    return BadRequest(new
                    {
                        error = "Invalid Your Email Or Password"
                    });
            }
            var userRoles = await userManager.GetRolesAsync(user);
            var clims = new[]
            {
                new Claim(ClaimTypes.Name , user.UserName!),
                new Claim(ClaimTypes.Email , user.Email!),
                new Claim(ClaimTypes.Role , string.Join(", ",userRoles)),
                new Claim(ClaimTypes.NameIdentifier , user.Id!),
                new Claim(JwtRegisteredClaimNames.Iat , Guid.NewGuid().ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yyuysuyiusdussdjncncxmnmncxmnjdskjskjklaklksakllasklklasklasad"));
            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
                (
                issuer: "https://localhost:7218",
                audience: "https://localhost:7218",
                claims: clims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credential
                );
            return Ok(new
            {
                success = "Your Login Successfully",
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string email, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound(new
                {
                    error = "User Not Found"
                });
            var otps = await applicationOtpRepo.GetAsync(o => o.UserId == user.Id && o.CreateAt.Day == DateTime.Now.Day, tracked: false, cancellationToken: cancellationToken);
            if (otps.Count() >= 3)
                return BadRequest(new
                {
                    error = "You Have Reached The Maximum Number Of OTP Requests.Please Try Again Later."
                });
            var otp = new Random().Next(100000, 999999).ToString();
            var userOtp = new ApplicationUserOtp
            {
                UserId = user.Id,
                CreateAt = DateTime.Now,
                ExpirationTime = DateTime.Now.AddMinutes(30),
                OtpCode = otp.ToString()
            };
            // here we can use ternary operator to set IsValid or Invalid by we check it we check the expiration time and compare it with now time
            userOtp.IsValid = userOtp.ExpirationTime > DateTime.Now ? true : false;

            await applicationOtpRepo.AddAsync(userOtp, cancellationToken: cancellationToken);
            await applicationOtpRepo.CommitAsync(cancellationToken);
            await emailSender.SendEmailAsync(email, "ECommerce Reset Password", $"<h1>Your OTP Code Is : {otp} </h1>");
            return CreatedAtAction("ValidateOtp", new { userId = userOtp.UserId });
        }
        [HttpPost("ValidateOtp")]
        public async Task<IActionResult> ValidateOtp(string userId, string otp)
        {
            var userOtp = await applicationOtpRepo.GetOneAsync(o => o.UserId == userId && o.OtpCode == otp && o.IsValid);
            if (userOtp is null)
                return NotFound(new
                {
                    error = "Invalid OTP Number"
                });
            return CreatedAtAction(nameof(ResetPassword), new { userId });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await userManager.FindByIdAsync(resetPasswordRequest.ApplicationUserId);
            var token = await userManager.GeneratePasswordResetTokenAsync(user!);
            var result = await userManager.ResetPasswordAsync(user!, token, resetPasswordRequest.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok(new
             {
                 success = "Password Reset Successfully"
             });
        }
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound(new
                {
                    error = "Invalid Email, Please Enter The Correct Email.."
                });
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(EmailConfirmation), "Register", new { area = "Identity", token, userId = user.Id }, Request.Scheme);
            await emailSender.SendEmailAsync(email, "ECommerce Resend Email Confirmation ", $"<h1> To Confirm Your Email Click <a href='{link}'>Here</a></h1>");
            return Ok(new
            {
                success = "Resend Confirmation Successfully"
            });
        }

    }
}
