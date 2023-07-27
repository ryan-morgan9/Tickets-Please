using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using static Tickets_Please.Pages.OrdersModel;
using System.Data;

namespace Tickets_Please.Pages
{
    public class ManageShowsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ManageShowsModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class Show
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string City { get; set; }
            public string Venue { get; set; }
            public string Capacity { get; set; }
            public string Price { get; set; }
        }
        public List<Show> Showings { get; set; }

        public IActionResult OnGet()
        {

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            Showings = new List<Show>();

            // Get all showings from the DB to display on page
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT * FROM dbo.Shows", connection);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime dateWithTime = (DateTime)reader["Date"];
                    Show show = new Show
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                        Date = dateWithTime.ToString("dd/MM/yyyy"),
                        Time = reader["Time"].ToString(),
                        City = reader["City"].ToString(),
                        Venue = reader["Venue"].ToString(),
                        Capacity = reader["Capacity"].ToString(),
                        Price = reader["Price"].ToString()
                    };

                    Showings.Add(show);
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            var Show_Id = Request.Form["showing"].ToString();
            System.Diagnostics.Debug.WriteLine(Show_Id);
            string action = Request.Form["action"];
            
            // No implementation yet for modifying bookings
            if (action == "Modify")
            {
               
            }
            else if (action == "Cancel")
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Cancel any bookings for the show
                    using SqlCommand command1 = new SqlCommand("DELETE FROM dbo.Bookings WHERE Show_Id = @Show_Id", connection);
                    command1.Parameters.Add("@Show_Id", SqlDbType.Int).Value = Convert.ToInt32(Show_Id);
                    int deleteBookings = command1.ExecuteNonQuery();
                    
                    // Delete the show from DB
                    using SqlCommand command2 = new SqlCommand("DELETE FROM dbo.Shows WHERE Id = @Id", connection);
                    command2.Parameters.Add("@Id", SqlDbType.Int).Value = Convert.ToInt32(Show_Id);
                    int deleteShows = command2.ExecuteNonQuery();
                }

                return Redirect("ManageShows");
            }

            return Redirect("ManageShows");
        }
    }
}
