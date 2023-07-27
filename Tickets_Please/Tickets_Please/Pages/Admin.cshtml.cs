using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Tickets_Please.Pages
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class AdminModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AdminModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }
        public IActionResult OnPost() 
        {
            // Get details of showing from form
            var title = Request.Form["title"].ToString();
            var date = Request.Form["date"].ToString();
            var time = Request.Form["time"].ToString();
            var city = Request.Form["city"].ToString();
            var venue = Request.Form["venue"].ToString();
            var price = Request.Form["price"].ToString();
            var capacity = Request.Form["capacity"].ToString();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Insert new showing into the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand("INSERT INTO dbo.Shows (Title, Date, Time, City, Venue, Price, Capacity) VALUES (@Title, @Date, @Time, @City, @Venue, @Price, @Capacity)", connection);
                command.Parameters.Add("@Title", SqlDbType.VarChar).Value = title;
                command.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                command.Parameters.Add("@Time", SqlDbType.Time).Value = time;
                command.Parameters.Add("@City", SqlDbType.VarChar).Value = city;
                command.Parameters.Add("@Venue", SqlDbType.VarChar).Value = venue;
                command.Parameters.Add("@Price", SqlDbType.Float).Value = Convert.ToDouble(price);
                command.Parameters.Add("@Capacity", SqlDbType.Int).Value = Convert.ToInt32(capacity);

                int insertShow = command.ExecuteNonQuery();
            }

            return Redirect("Admin");
        }
    }
}
