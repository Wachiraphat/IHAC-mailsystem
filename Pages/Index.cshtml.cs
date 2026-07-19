using FinalProject.Areas.Identity.Data;
using FinalProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Pages
{
    public class IndexModel : PageModel
    {
        public List<EmailInfo> ListEmails { get; set; } = new List<EmailInfo>();

        private readonly ILogger<IndexModel> _logger;
        private readonly FinalProjectContext _dbContext;

        public IndexModel(ILogger<IndexModel> logger, FinalProjectContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        // Load Emails (GET)
        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(User.Identity?.Name))
            {
                return;
            }

            string username = User.Identity.Name;

            try
            {
                ListEmails = await _dbContext.Emails
                    .Where(e => e.EmailReceiver == username)
                    .OrderByDescending(e => e.DateSent)
                    .Select(e => new EmailInfo
                    {
                        EmailID = e.Id.ToString(),
                        EmailSubject = e.Subject,
                        EmailMessage = e.Body,
                        EmailDate = e.DateSent.ToString("yyyy-MM-dd HH:mm:ss"),
                        EmailIsRead = e.ReadStatus ? "1" : "0",
                        EmailSender = e.EmailSender,
                        EmailReceiver = e.EmailReceiver
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while fetching emails for user {Username}.", username);
                TempData["ErrorMessage"] = "There was an issue retrieving your emails. Please try again later.";
            }
        }

        // Delete Email (POST)
        public async Task<IActionResult> OnPostDeleteEmailAsync(int emailid)
        {
            if (emailid <= 0)
            {
                _logger.LogWarning("Invalid email ID: {EmailID} provided for deletion.", emailid);
                TempData["ErrorMessage"] = "Invalid email selected for deletion.";
                return RedirectToPage();
            }

            try
            {
                var email = await _dbContext.Emails.FindAsync(emailid);
                if (email == null)
                {
                    _logger.LogWarning("No email found with ID {EmailID} for deletion.", emailid);
                    TempData["ErrorMessage"] = "Email not found or already deleted.";
                    return RedirectToPage();
                }

                _dbContext.Emails.Remove(email);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Email with ID {EmailID} deleted successfully.", emailid);
                TempData["SuccessMessage"] = "Email deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred while deleting email with ID {EmailID}.", emailid);
                TempData["ErrorMessage"] = "There was an issue deleting the email. Please try again later.";
            }

            return RedirectToPage();
        }

        private async Task MarkEmailAsRead(int emailid)
        {
            try
            {
                var email = await _dbContext.Emails.FindAsync(emailid);
                if (email == null)
                {
                    return;
                }

                email.ReadStatus = true;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating read status for email ID {EmailID}.", emailid);
            }
        }
    }

    public class EmailInfo
    {
        public string EmailID { get; set; }
        public string EmailSubject { get; set; }
        public string EmailMessage { get; set; }
        public string EmailDate { get; set; }
        public string EmailIsRead { get; set; }
        public string EmailSender { get; set; }
        public string EmailReceiver { get; set; }
    }
}

