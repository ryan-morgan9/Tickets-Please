using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using NuGet.Protocol.Plugins;
using System.Data;
using System.Security.Claims;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Tickets_Please.Pages.ResultsModel;

namespace Tickets_Please.Pages
{
    public class Show
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public int Price { get; set; }
        public int Capacity { get; set; }
    }

    [Authorize]
    public class BookingModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public BookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Show Show { get; set; }

        public void OnGet(string Id)
        {
            int showing_id = Convert.ToInt32(Id);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand("SELECT * FROM dbo.Shows WHERE Id = @Show_Id", connection);
                command.Parameters.Add("@Show_Id", SqlDbType.Int).Value = Convert.ToInt32(showing_id);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime dateWithTime = (DateTime)reader["Date"];
                    Show = new Show
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                        Date = dateWithTime.ToString("dd/MM/yyyy"),
                        Time = reader["Time"].ToString(),
                        City = reader["City"].ToString(),
                        Venue = reader["Venue"].ToString(),
                        Price = Convert.ToInt32(reader["Price"]),
                        Capacity = Convert.ToInt32(reader["Capacity"])
                    };
                }
            }
        }

        public IActionResult OnPost()
        {
            var Show_Id = Request.Form["Id"].ToString();
            var quantity = Request.Form["ticketquantity"].ToString();
            var cost = Request.Form["hiddenprice"].ToString();

            System.Diagnostics.Debug.WriteLine("COST"+cost);

            var capacity = Request.Form["capacity"].ToString();

            var newCapacity = (Convert.ToInt32(capacity) - Convert.ToInt32(quantity));


            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand("INSERT INTO dbo.Bookings (Show_Id, User_Id, Ticket_Quantity, Cost) VALUES (@Show_Id, @User_Id, @Ticket_Quantity, @Cost)", connection);
                command.Parameters.Add("@Show_Id", SqlDbType.Int).Value = Convert.ToInt32(Show_Id);
                // User's ID is retrieved here
                command.Parameters.Add("@User_Id", SqlDbType.NVarChar).Value = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                command.Parameters.Add("@Ticket_Quantity", SqlDbType.Int).Value = Convert.ToInt32(quantity);
                
                command.Parameters.Add("@Cost", SqlDbType.Float).Value = Convert.ToDouble(cost);

                int insertBooking = command.ExecuteNonQuery();
            }

            using (SqlConnection newconnection = new SqlConnection(connectionString))
            {
                newconnection.Open();
                using SqlCommand command = new SqlCommand("UPDATE dbo.Shows SET capacity = @Capacity WHERE Id = @Show_Id", newconnection);
                command.Parameters.Add("@Capacity", SqlDbType.Int).Value = Convert.ToInt32(newCapacity);
                command.Parameters.Add("@Show_Id", SqlDbType.Int).Value = Convert.ToInt32(Show_Id);

                int updateBooking = command.ExecuteNonQuery();
            }

            return Redirect("Orders");
        }
    }
}
