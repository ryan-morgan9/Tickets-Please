using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace Tickets_Please.Pages
{

    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public OrdersModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class Show
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string City { get; set; }
            public string Venue { get; set; }
            public string Date { get; set; }
            public TimeSpan Time { get; set; }
            public int Capacity { get; set; }
        }

        public class Booking
        {
            public int Id { get; set; }
            public string Show_Id { get; set; }
            public string User_Id { get; set; }
            public int Quantity { get; set; }
            public double Cost { get; set; }
            public Show Show { get; set; }
        }
        
        public List<Booking> Bookings { get; set; }

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand(@"
                    SELECT B.Id, B.Show_Id, B.User_Id, B.Ticket_Quantity, B.Cost, S.Title, S.City, S.Venue, S.[Date], S.[Time], S.[Capacity]
                    FROM dbo.Bookings B
                    INNER JOIN dbo.Shows S ON B.Show_Id = S.Id
                    WHERE B.User_Id = @User_Id", connection);

                // User's ID is retrieved here
                command.Parameters.Add("@User_Id", SqlDbType.NVarChar).Value = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();

                Bookings = new List<Booking>();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime dateWithTime = (DateTime)reader["Date"];
                    Booking booking = new Booking
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Show_Id = reader["Show_Id"].ToString(),
                        User_Id = reader["User_Id"].ToString(),
                        Quantity = Convert.ToInt32(reader["Ticket_Quantity"]),
                        Cost = Convert.ToDouble(reader["Cost"]),
                        Show = new Show
                        {
                            Id = Convert.ToInt32(reader["Show_Id"]),
                            Title = reader["Title"].ToString(),
                            City = reader["City"].ToString(),
                            Venue = reader["Venue"].ToString(),
                            Date = dateWithTime.ToString("dd/MM/yyyy"),
                            Time = (TimeSpan)reader["Time"],
                            Capacity = Convert.ToInt32(reader["Capacity"])
                        } 
                    };

                    Bookings.Add(booking);
                }

            }

        }

        public IActionResult OnPost()
        {
            var booking = Request.Form["booking"].ToString();
            var Show_Id = Request.Form["show_id"].ToString();
            var capacity = Request.Form["capacity"].ToString();
            var quantity = Request.Form["quantity"].ToString();
            var action = Request.Form["action"].ToString();

            if (action == "Cancel")
            {
                int updatedCapacity = (Convert.ToInt32(capacity) + Convert.ToInt32(quantity));

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using SqlCommand command1 = new SqlCommand("UPDATE dbo.Shows SET Capacity = @Capacity WHERE Id = @Show_Id;", connection);
                    command1.Parameters.Add("@Capacity", SqlDbType.Int).Value = Convert.ToInt32(updatedCapacity);
                    command1.Parameters.Add("@Show_Id", SqlDbType.Int).Value = Convert.ToInt32(Show_Id);

                    int updateShow = command1.ExecuteNonQuery();

                    using SqlCommand command2 = new SqlCommand("DELETE FROM dbo.Bookings WHERE Id = @Id", connection);
                    command2.Parameters.Add("@Id", SqlDbType.Int).Value = Convert.ToInt32(booking);

                    int deleteShow = command2.ExecuteNonQuery();
                }

                return Redirect("Orders");
            }
            else if (action == "Modify")
            {
                return Redirect("Orders");
            }

            return Page();
        }
    }
}
