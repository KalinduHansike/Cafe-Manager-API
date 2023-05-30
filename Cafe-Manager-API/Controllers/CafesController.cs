using Cafe_Manager_API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cafe_Manager_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CafesController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        public CafesController(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        private string connectionString = "Server=127.0.0.1;User ID=root;Password=RMi4iIEYZfHjswU+z75NrEqmNVGh1QX1;Database=db_cafe_register";

        // GET /api/cafes?location=<location>
        [HttpGet, Route("GetCafes")]
        public IActionResult GetCafes(string location = null)
        {
            List<Cafe> cafes = new List<Cafe>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Cafes";
                if (!string.IsNullOrEmpty(location))
                {
                    query += " WHERE Location = @Location";
                }

                MySqlCommand command = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(location))
                {
                    command.Parameters.AddWithValue("@Location", location);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cafe cafe = new Cafe
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Logo = _hostingEnvironment.WebRootPath + "\\uploads\\" + reader["Logo"].ToString(),
                            Location = reader["Location"].ToString()
                        };

                        cafes.Add(cafe);
                    }
                }
            }

            var cafesWithEmployees = cafes.Select(c => new
            {
                c.Name,
                c.Description,
                Employees = GetEmployeeCountByCafeId(c.Id),
                c.Logo,
                c.Location,
                c.Id
            }).OrderByDescending(c => c.Employees).ToList();

            return Ok(cafesWithEmployees);
        }

        // GET /api/cafes?location=<location>
        [HttpGet, Route("GetCafesForDropdown")]
        public IActionResult GetCafesForDropdown(string location = null)
        {
            List<CafeDropdown> cafeDropdowns = new List<CafeDropdown>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Cafes";
                if (!string.IsNullOrEmpty(location))
                {
                    query += " WHERE Location = @Location";
                }

                MySqlCommand command = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(location))
                {
                    command.Parameters.AddWithValue("@Location", location);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CafeDropdown cafeDropdown = new CafeDropdown
                        {
                            value = reader["Id"].ToString(),
                            label = reader["Name"].ToString()
                        };

                        cafeDropdowns.Add(cafeDropdown);
                    }
                }
            }

            var cafedrpdwn = cafeDropdowns.Select(c => new
            {
                c.value,
                c.label,
            });

            return Ok(cafedrpdwn);
        }


        // POST /api/cafe
        [HttpPost, Route("CreateCafe")]
        public IActionResult CreateCafe(Cafe cafe)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Cafes (Id, Name, Description, Logo, Location) VALUES (@Id, @Name, @Description, @Logo, @Location)";

                cafe.Id = Guid.NewGuid().ToString();

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", cafe.Id);
                command.Parameters.AddWithValue("@Name", cafe.Name);
                command.Parameters.AddWithValue("@Description", cafe.Description);
                command.Parameters.AddWithValue("@Logo", cafe.Logo);
                command.Parameters.AddWithValue("@Location", cafe.Location);

                command.ExecuteNonQuery();

                // await this.Upload(cafe.file);
            }

            return Ok(cafe.Id);
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Save the file to the desired location
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", file.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Ok("File uploaded successfully!");
            }

            return BadRequest("No file or empty file was received.");
        }



        // PUT /api/cafe
        [HttpPut, Route("UpdateCafe")]
        public IActionResult UpdateCafe(string id, Cafe cafe)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Cafes SET Name = @Name, Description = @Description, Logo = @Logo, Location = @Location WHERE Id = @Id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", cafe.Name);
                command.Parameters.AddWithValue("@Description", cafe.Description);
                command.Parameters.AddWithValue("@Logo", cafe.Logo);
                command.Parameters.AddWithValue("@Location", cafe.Location);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    return NotFound();
            }

            return Ok(cafe);
        }



        // DELETE /api/cafe?id=<id>
        [HttpDelete, Route("DeleteCafe")]
        public IActionResult DeleteCafe(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Cafes WHERE Id = @Id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    return NotFound();

                query = "DELETE FROM EmployeeCafe WHERE CafeId = @CafeId";

                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@CafeId", id);
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        private int GetEmployeeCountByCafeId(string cafeId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM EmployeeCafe WHERE CafeId = @CafeId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@CafeId", cafeId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // GET /api/cafes?location=<location>
        [HttpGet, Route("GetCafesbyFilter")]
        public IActionResult GetCafesbyFilter(string keyword)
        {
            List<Cafe> cafes = new List<Cafe>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Cafes";
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " WHERE location LIKE '%' @Location '%'";
                }

                MySqlCommand command = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(keyword))
                {
                    command.Parameters.AddWithValue("@Location", keyword);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cafe cafe = new Cafe
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Logo = reader["Logo"].ToString(),
                            Location = reader["Location"].ToString()
                        };

                        cafes.Add(cafe);
                    }
                }
            }

            var cafesWithEmployees = cafes.Select(c => new
            {
                c.Name,
                c.Description,
                Employees = GetEmployeeCountByCafeId(c.Id),
                c.Logo,
                c.Location,
                c.Id
            }).OrderByDescending(c => c.Employees).ToList();

            return Ok(cafesWithEmployees);
        }
    }
}
