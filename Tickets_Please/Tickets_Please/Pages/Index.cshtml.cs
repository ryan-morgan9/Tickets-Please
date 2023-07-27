using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using static Tickets_Please.Pages.IndexModel;
// Ryan Morgan

namespace Tickets_Please.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public List<string> Cities { get; set; }

        public IActionResult OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // List which enables access to cities from HTML
            Cities = new List<string>();

            // Get all unique cities of where shows are located
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using SqlCommand command = new SqlCommand("SELECT DISTINCT City FROM dbo.Shows", connection);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Cities.Add(reader["City"].ToString());
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            // Send selected city to results page
            string city = Request.Form["city"];
            return RedirectToPage("Results", new { city });
        }
    }
}