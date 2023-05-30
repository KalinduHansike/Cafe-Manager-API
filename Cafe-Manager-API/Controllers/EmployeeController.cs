using Cafe_Manager_API.Entities;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cafe_Manager_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        //// GET: api/<EmployeeController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        private string connectionString = "Server=127.0.0.1;User ID=root;Password=RMi4iIEYZfHjswU+z75NrEqmNVGh1QX1;Database=db_cafe_register";

        // GET /api/employees?cafe=<café>
        [HttpGet, Route("GetEmployees")]
        public IActionResult GetEmployees(string cafe = null)
        {
            List<Employee> employees = new List<Employee>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Employees";
                if (!string.IsNullOrEmpty(cafe))
                {
                    query += " JOIN EmployeeCafe ON Employees.Id = EmployeeCafe.EmployeeId WHERE EmployeeCafe.CafeId = @CafeId";
                }

                MySqlCommand command = new MySqlCommand(query, connection);
                if (!string.IsNullOrEmpty(cafe))
                {
                    command.Parameters.AddWithValue("@CafeId", cafe);
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            EmailAddress = reader["EmailAddress"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            Gender = reader["Gender"].ToString()
                        };

                        employees.Add(employee);
                    }
                }
            }

            var employeesWithDaysWorked = employees.Select(emp => new
            {
                emp.Id,
                emp.Name,
                emp.EmailAddress,
                emp.PhoneNumber,
                DaysWorked = GetDaysWorkedForEmployee(emp.Id),
                Cafe = GetCafeForEmployee(emp.Id)
            }).OrderByDescending(emp => emp.DaysWorked).ToList();

            return Ok(employeesWithDaysWorked);
        }


        // POST /api/employee
        [HttpPost, Route("CreateEmployee")]
        public IActionResult CreateEmployee(Employee employee)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Employees (Id, Name, EmailAddress, PhoneNumber, Gender) VALUES (@Id, @Name, @EmailAddress, @PhoneNumber, @Gender)";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", "UI" + (GetEmployeeCount() + 1).ToString("D7"));
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@EmailAddress", employee.EmailAddress);
                command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@Gender", employee.Gender);

                command.ExecuteNonQuery();
            }

            return Ok("Successfully Saved");
        }

        // PUT /api/employee
        [HttpPut, Route("UpdateEmployee")]
        public IActionResult UpdateEmployee(string id, Employee employee)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Employees SET Name = @Name, EmailAddress = @EmailAddress, PhoneNumber = @PhoneNumber, Gender = @Gender WHERE Id = @Id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@EmailAddress", employee.EmailAddress);
                command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@Gender", employee.Gender);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    return NotFound();
            }

            return Ok(employee);
        }

        // DELETE /api/employee?id=<id>
        [HttpDelete, Route("DeleteEmployee")]
        public IActionResult DeleteEmployee(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Employees WHERE Id = @Id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    return NotFound();

                query = "DELETE FROM EmployeeCafe WHERE EmployeeId = @EmployeeId";

                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeId", id);
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        private int GetEmployeeCount()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Employees";

                MySqlCommand command = new MySqlCommand(query, connection);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private int GetDaysWorkedForEmployee(string employeeId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT DATEDIFF(NOW(), StartDate) FROM EmployeeCafe WHERE EmployeeId = @EmployeeId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                return 0;
            }
        }

        private Cafe GetCafeForEmployee(string employeeId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Cafes.* FROM Cafes JOIN EmployeeCafe ON Cafes.Id = EmployeeCafe.CafeId WHERE EmployeeCafe.EmployeeId = @EmployeeId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Cafe cafe = new Cafe
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Logo = reader["Logo"].ToString(),
                            Location = reader["Location"].ToString()
                        };

                        return cafe;
                    }
                }
            }

            return null;
        }
    }
}
