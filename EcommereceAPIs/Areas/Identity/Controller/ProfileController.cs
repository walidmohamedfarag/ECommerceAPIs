using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ECommerceAPI.Areas.Identity.Controller
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Identity")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public ProfileController(UserManager<ApplicationUser> _userManager , SignInManager<ApplicationUser> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest updateProfileRequest)
        {
            var user = await userManager.GetUserAsync(User);
            user!.FirstName = updateProfileRequest.FullName.Split(' ')[0];
            user.LastName = updateProfileRequest.FullName.Split(' ')[1];
            user.UserName = $"{user.FirstName}{user.LastName}{new Random().Next(0, 10)}";
            user.PhoneNumber = updateProfileRequest.PhoneNumber;
            user.Address = updateProfileRequest.Address;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            await signInManager.RefreshSignInAsync(user);
            return Ok(new
            {
                success = "Profile Updated Successfully"
            });
        }
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdateProfileRequest updateProfileRequest)
        {
            var user = await userManager.GetUserAsync(User);
            if (updateProfileRequest.CurrentPassword is null || updateProfileRequest.NewPassword is null)
                return BadRequest(new
                {
                    error = "You Must Enter The Password"
                });
            var isCorrect = await userManager.CheckPasswordAsync(user!, updateProfileRequest.CurrentPassword!);
            if (!isCorrect)
                return BadRequest(new
                {
                    error = "Current Password is Incorrect"
                });
            var result = await userManager.ChangePasswordAsync(user!, updateProfileRequest.CurrentPassword!, updateProfileRequest.NewPassword!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok(new
            {
                success = "Password Updated Successfully"
            });
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var user = await userManager.GetUserAsync(User);
            await signInManager.SignOutAsync();
            return Ok(new
            {
                success = "Logout Successfully"
            });
        }

    }
}
