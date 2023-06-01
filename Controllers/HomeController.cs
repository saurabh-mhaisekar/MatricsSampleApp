using MatricsSampleApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace MatricsSampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SQLiteConnection _connection;

        private const String _connectionString = "Data Source=data/database.sqlite;Version=3;";
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _connection = new SQLiteConnection(_connectionString);

            _connection.Open();

            if (!IsTableExist())
            {
                CreateTable();
            }

           
        }

        private bool IsTableExist()
        {
            string query = $"SELECT name FROM sqlite_master WHERE type='table' AND name='person'";
            using (SQLiteCommand command = new SQLiteCommand(query, _connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        private void CreateTable()
        { 
            string query = @"CREATE TABLE IF NOT EXISTS person (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Age INTEGER
                         );";
            using (SQLiteCommand command = new SQLiteCommand(query, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public IActionResult Index()
        {
            const string sql = "SELECT * FROM person";
            List<Person> persons = new List<Person>();

            try
            {
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.CommandText = sql;

                    var data = command.ExecuteReader();

                    while (data.Read())
                    {
                        persons.Add(new Person { Id = data.GetInt32(0) ,Name = data.GetString(1), Age = data.GetInt32(2) });
                    }

                    return View(persons);
                }
            }
            catch (Exception ex)
            {
                return View(ex);
            }


        }


        public IActionResult NewPerson()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeletePerson(int id) 
        {
            string query = $"DELETE FROM Person WHERE Id = {id}";

            using(SQLiteCommand command = new SQLiteCommand(query,_connection))
            {
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreatePerson(string name, int age)
        {
            string query = "INSERT INTO Person (Name, Age) VALUES (@Name, @Age)";
            using (SQLiteCommand command = new SQLiteCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Age", age);
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}