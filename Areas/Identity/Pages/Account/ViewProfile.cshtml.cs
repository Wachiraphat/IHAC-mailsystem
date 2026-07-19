using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ViewProfileModel : PageModel
    {
        // Properties to hold profile information
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        // Database connection string
        private readonly string connectionString = "Server=tcp:ihac2cpu.database.windows.net,1433;Initial Catalog=IHAC;Persist Security Info=False;User ID=IHAC2CPU;Password=Admin2IHAC;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // Method to retrieve user profile data by email ID
        public async Task OnGetAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL query to get user details by Email
                    string sql = "SELECT Email, FirstName, LastName, MobilePhone FROM AspNetUsers WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Ensure to check if data is NULL before retrieving
                                Email = reader.IsDBNull(0) ? "N/A" : reader.GetString(0); // Email is in column 0
                                FirstName = reader.IsDBNull(1) ? "N/A" : reader.GetString(1); // FirstName is in column 1
                                LastName = reader.IsDBNull(2) ? "N/A" : reader.GetString(2); // LastName is in column 2
                                PhoneNumber = reader.IsDBNull(3) ? "N/A" : reader.GetString(3); // MobilePhone is in column 3
                            }
                            else
                            {
                                // If no data found
                                Email = "No data found";
                                FirstName = "N/A";
                                LastName = "N/A";
                                PhoneNumber = "N/A";
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {sqlEx.Message}");
                Email = "Error retrieving data";
                FirstName = "Error";
                LastName = "Error";
                PhoneNumber = "Error";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                Email = "Error retrieving data";
                FirstName = "Error";
                LastName = "Error";
                PhoneNumber = "Error";
            }
        }

    }
}