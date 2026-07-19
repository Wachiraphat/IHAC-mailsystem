using FinalProject.Areas.Identity.Data;
using FinalProject.Data;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ViewEmailModel : PageModel
    {
        public string EmailSubject { get; set; }
        public string EmailSender { get; set; }
        public string EmailDate { get; set; }
        public string EmailMessage { get; set; }
        public string EmailReceiver { get; set; }

        private readonly FinalProjectContext _context;

        public ViewEmailModel(FinalProjectContext context)
        {
            _context = context;
        }

        public async Task OnGet(int emailid)
        {
            try
            {
                var email = await _context.Emails.FindAsync(emailid);
                if (email != null)
                {
                    EmailReceiver = email.EmailReceiver;
                    EmailSubject = email.Subject;
                    EmailSender = email.EmailSender;
                    EmailDate = email.DateSent.ToString("yyyy-MM-dd HH:mm:ss");
                    EmailMessage = email.Body;

                    email.ReadStatus = true;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    EmailSubject = "Email not found";
                    EmailMessage = "The email you're looking for could not be found.";
                }
            }
            catch (Exception ex)
            {
                EmailSubject = "Database Error";
                EmailMessage = $"An error occurred while fetching the email: {ex.Message}";
            }
        }
    }
}
