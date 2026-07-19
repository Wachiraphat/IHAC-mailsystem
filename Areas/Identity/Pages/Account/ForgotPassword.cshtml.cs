using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        // Properties to hold user input
        [BindProperty]
        public string UserName { get; set; }
        [BindProperty]
        public string MobilePhone { get; set; }

        // Database connection string
        private readonly string connectionString = "Server=tcp:ihac2cpu.database.windows.net,1433;Initial Catalog=IHAC;Persist Security Info=False;User ID=IHAC2CPU;Password=Admin2IHAC;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public string ErrorMessage { get; set; }

        // Method to handle GET request
        public void OnGet() { }

        // Method to handle POST request for password reset
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(MobilePhone))
            {
                ErrorMessage = "UserName and MobilePhone are required.";
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL query to get user details by UserName and MobilePhone
                    string sql = "SELECT Email FROM AspNetUsers WHERE UserName = @UserName AND MobilePhone = @MobilePhone";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", UserName);
                        command.Parameters.AddWithValue("@MobilePhone", MobilePhone);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // If the data matches, redirect to change password page
                                return RedirectToPage("./ResetPassword", new { email = reader.GetString(0) });
                            }
                            else
                            {
                                // If no match found
                                ErrorMessage = "No account found with the provided username and mobile phone.";
                                return Page();
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                ErrorMessage = $"Database error: {sqlEx.Message}";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }
    }
}
