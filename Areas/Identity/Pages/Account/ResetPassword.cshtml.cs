using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using FinalProject.Areas.Identity.Data;
using System.Threading.Tasks;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<FinalProjectUser> _userManager;

        public ResetPasswordModel(UserManager<FinalProjectUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }

        // GET: ResetPassword page
        public void OnGet(string email)
        {
            Email = email;
        }

        // POST: Reset the password
        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return Page(); // Stay on the same page and show error message
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page(); // Stay on the same page and show error message
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                var passwordResult = await _userManager.AddPasswordAsync(user, NewPassword);
                if (passwordResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "Your password has been reset successfully.";
                    return RedirectToPage("./Login"); // Redirect to login page after password reset
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to set new password.\n The Password must be at least 6 and at max 100 characters long.\n Passwords must have at least one non-alphanumeric character.\n Passwords must have at least one uppercase ('A'-'Z').");

                    return Page(); // Stay on the same page and show error message
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to remove old password.");
                return Page(); // Stay on the same page and show error message
            }
        }


    }
}
