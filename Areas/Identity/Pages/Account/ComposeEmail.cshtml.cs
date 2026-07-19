using FinalProject.Data;  // Your DbContext
using FinalProject.Models;  // The Email model
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;  // Required for UserManager
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FinalProject.Areas.Identity.Data;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ComposeEmailModel : PageModel
    {
        private readonly FinalProjectContext _context;
        private readonly ILogger<ComposeEmailModel> _logger;
        private readonly UserManager<FinalProjectUser> _userManager;  // Add UserManager

        [BindProperty]
        public string To { get; set; }

        [BindProperty]
        public string Subject { get; set; }

        [BindProperty]
        public string Body { get; set; }

        public ComposeEmailModel(FinalProjectContext context, ILogger<ComposeEmailModel> logger, UserManager<FinalProjectUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;  // Inject UserManager
        }

        // OnGet: You can use this to set up anything on page load
        public void OnGet()
        {
            // Any setup code can go here, if needed
        }

        // OnPostAsync: This method handles form submission
        public async Task<IActionResult> OnPostAsync()
        {
            // Validate required fields
            if (string.IsNullOrEmpty(To))
            {
                ModelState.AddModelError("To", "Recipient email is required.");
            }

            if (string.IsNullOrEmpty(Subject))
            {
                ModelState.AddModelError("Subject", "Subject is required.");
            }

            if (string.IsNullOrEmpty(Body))
            {
                ModelState.AddModelError("Body", "Body cannot be empty.");
            }

            // If model state is invalid, return with errors
            if (!ModelState.IsValid)
            {
                return Page(); // If validation fails, return the page with errors
            }

            // Validate email address format
            if (!Regex.IsMatch(To, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                ModelState.AddModelError("To", "Please enter a valid email address.");
                return Page();
            }

            // Check if recipient exists in the system (AspNetUsers)
            var user = await _userManager.FindByEmailAsync(To);
            if (user == null)
            {
                ModelState.AddModelError("To", "No user found with this email address.");
                return Page(); // Return with error message
            }

            try
            {
                // Create an Email object to store in the database
                var email = new Email
                {
                    EmailReceiver = To,
                    Subject = Subject,
                    Body = Body,
                    DateSent = DateTime.UtcNow,
                    EmailSender = User.Identity.Name,  // Ensure the sender is captured
                    ReadStatus = false // Default to unread
                };

                // Add the email to the DbContext
                _context.Emails.Add(email);
                await _context.SaveChangesAsync();

                // Log the success
                _logger.LogInformation("Email sent to {EmailReceiver} from {EmailSender} with subject: {Subject}",
                                       To, User.Identity.Name, Subject);

                // Show success message in TempData
                TempData["SuccessMessage"] = "Email sent successfully!";
                return RedirectToPage("/Index"); // Redirect to the index page
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while sending the email.");
                TempData["ErrorMessage"] = "An error occurred while sending the email.";
                return RedirectToPage("/Error"); // Redirect to the error page
            }
        }
    }
}
