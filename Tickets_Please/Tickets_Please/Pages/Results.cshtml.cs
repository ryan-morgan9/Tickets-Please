using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace Tickets_Please.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ResultsModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public class Show
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public TimeSpan Time { get; set; }
            public string City { get; set; }
            public string Venue { get; set; }
        }
        public string City { get; set; }
        public List<Show> Showings { get; set; }

        public IActionResult OnGet(string city)
        {
            City = city;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            Showings = new List<Show>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery;
                SqlCommand command;

                System.Diagnostics.Debug.WriteLine("CITY:"+city);

                if (city == "all")
                {
                    // Get shows from all cities
                    sqlQuery = "SELECT * FROM dbo.Shows";
                    command = new SqlCommand(sqlQuery, connection);
                }
                else
                {
                    // Get shows from selected city
                    sqlQuery = "SELECT * FROM dbo.Shows WHERE City = @City";
                    command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@City", city);
                }

                // Retrieve details of all the selected shows into accessible list 
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime dateWithTime = (DateTime)reader["Date"];
                    Show show = new Show
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                        Date = dateWithTime.ToString("dd/MM/yyyy"),
                        Time = (TimeSpan)reader["Time"],
                        City = reader["City"].ToString(),
                        Venue = reader["Venue"].ToString()
                    };

                    Showings.Add(show);
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            // Send selected showing to booking page
            string Id = Request.Form["showing"];
            return RedirectToPage("/Booking", new { Id });
        }
    }
}
