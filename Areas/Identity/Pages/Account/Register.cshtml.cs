// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FinalProject.Areas.Identity.Data;
using FinalProject.Data;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<FinalProjectUser> _signInManager;
        private readonly UserManager<FinalProjectUser> _userManager;
        private readonly IUserStore<FinalProjectUser> _userStore;
        private readonly IUserEmailStore<FinalProjectUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly FinalProjectContext _dbContext;
        private readonly IDataProtector _demoRegistrationProtector;
        private readonly IWebHostEnvironment _environment;

        public RegisterModel(
            UserManager<FinalProjectUser> userManager,
            IUserStore<FinalProjectUser> userStore,
            SignInManager<FinalProjectUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            FinalProjectContext dbContext,
            IDataProtectionProvider dataProtectionProvider,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _demoRegistrationProtector = dataProtectionProvider.CreateProtector("IHAC demo registration ticket v1");
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public String ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(255, ErrorMessage = "The First Name must be in between 1 to 255.", MinimumLength = 1)]
            public string FirstName { get; set; }

            [Required]
            [StringLength(255, ErrorMessage = "The Last Name must be in between 1 to 255.", MinimumLength = 1)]
            public string LastName { get; set; }

            [Required]
            [StringLength(15, ErrorMessage = "The Mobile Phone must be in between 7 to 15.", MinimumLength = 7)]
            public string MobilePhone { get; set; }

            [Required]
            [StringLength(255, ErrorMessage = "The Username must be between 1 to 255.", MinimumLength = 1)]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.MobilePhone = Input.MobilePhone;

                if (!Input.UserName.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Input.UserName", "The username must end with '@gmail.com'.");
                    return Page();
                }

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.UserName, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    SaveDemoRegistrationTicket(user);

                    // Set success message to TempData
                    TempData["SuccessMessage"] = "Your account has been created successfully! Please confirm your email.";

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.UserName, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    var welcomeEmail = new Email
                    {
                        EmailReceiver = Input.UserName,
                        EmailSender = "IHAC Email System",
                        Subject = "Welcome to IHAC Email!",
                        Body = "ยินดีต้อนรับสู่ IHAC Email!\n\nโปรดเริ่มต้นด้วยการยืนยันอีเมลของคุณ จากนั้นคุณสามารถอ่านเมลใหม่ ส่งเมล หรือจัดการบัญชีได้จากหน้าเมนูหลัก.\n\n- คลิก 'Inbox' เพื่อดูเมลของคุณ\n- คลิก 'Compose Email' เพื่อส่งเมล\n- คลิก 'Setting' หรือ 'View Profile' เพื่อแก้ไขข้อมูลส่วนตัว\n\nหากต้องการความช่วยเหลือเพิ่มเติม ให้ติดต่อผู้ดูแลระบบหรือดูคำแนะนำในหน้าเว็บไซต์.",
                        DateSent = DateTime.UtcNow,
                        ReadStatus = false
                    };

                    _dbContext.Emails.Add(welcomeEmail);
                    await _dbContext.SaveChangesAsync();

                    return RedirectToPage("/Account/Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private void SaveDemoRegistrationTicket(FinalProjectUser user)
        {
            if (!_environment.IsEnvironment("Docker"))
            {
                return;
            }

            var ticket = new DemoRegistrationTicket
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MobilePhone = user.MobilePhone,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp
            };

            var protectedTicket = _demoRegistrationProtector.Protect(JsonSerializer.Serialize(ticket));
            Response.Cookies.Append("IHAC.DemoRegistration", protectedTicket, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(1)
            });
        }



        private FinalProjectUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<FinalProjectUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(FinalProjectUser)}'. " +
                    $"Ensure that '{nameof(FinalProjectUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<FinalProjectUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<FinalProjectUser>)_userStore;
        }
    }
}
