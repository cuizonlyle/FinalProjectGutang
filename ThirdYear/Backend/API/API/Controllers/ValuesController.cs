using API.Models;
using API.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API.Controllers
{
    public class ValuesController : ApiController
    {
        // CREATE ACCOUNT for existing Employee
        [HttpPost]
        [Route("api/create", Name = "Create_Account")]
        public IHttpActionResult CreateAccount([FromBody] Create create)
        {
            if (create == null ||
                string.IsNullOrWhiteSpace(create.id) ||
                string.IsNullOrWhiteSpace(create.username) ||
                string.IsNullOrWhiteSpace(create.password))
                return Content(HttpStatusCode.BadRequest, "Invalid data");
            try
            {
                using (MySqlConnection conn = new MySqlConnection(
                    ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();

                    string hashedPassword = PasswordHasher.Hash(create.password);

                    string query = @"SELECT COUNT(*) 
                             FROM users 
                             WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", create.id);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 1)
                        {
                            return Content(HttpStatusCode.BadRequest,
                                "User already exist!");
                        }
                    }

                    //CREATE user account for Employee
                    DateTime now = DateTime.Now;

                    string insertQuery = @"INSERT INTO users 
                                   VALUES(@id1, @username, @password, 'Active', @date_time_created, 'User')";

                    using (MySqlCommand addCmd = new MySqlCommand(insertQuery, conn))
                    {
                        addCmd.Parameters.AddWithValue("@id1", create.id);
                        addCmd.Parameters.AddWithValue("@username", create.username);
                        addCmd.Parameters.AddWithValue("@password", hashedPassword);
                        addCmd.Parameters.AddWithValue("@date_time_created", now.ToString("yyyy-MM-dd HH:mm:ss"));
                        addCmd.ExecuteNonQuery();
                    }

                    return Ok("Account created successfully.");
                }
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.Forbidden, "Unable to proceed.");
            }
        }

        //something

        //Get All Data
        [HttpGet]
        [Route("api/employee/info", Name = "Employee_Info")]
        public IHttpActionResult EmployeeInfo()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(
                    ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();

                    string query = @"SELECT * FROM employee";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return NotFound();

                            List<Employee> stats = new List<Employee>();

                            while (reader.Read())
                            {
                                stats.Add(new Employee
                                {
                                    employee_id = reader["employee_id"].ToString(),
                                    first_name = reader["first_name"].ToString(),
                                    last_name = reader["last_name"].ToString(),
                                    gender = reader["gender"].ToString(),
                                    email = reader["email"].ToString(),
                                });
                            }

                            return Ok(stats);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Get Data by ID
        [HttpGet]
        [Route("api/employee/info/{id}", Name = "Employee_Info_Id")]
        public IHttpActionResult EmployeeInfoId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid data.");
            try
            {
                using (MySqlConnection conn = new MySqlConnection(
                    ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();

                    string query = @"SELECT * FROM employee 
                             WHERE employee_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return Content(HttpStatusCode.NotFound, "No data.");

                            reader.Read();

                            Employee employee = new Employee
                            {
                                employee_id = reader["employee_id"].ToString(),
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString(),
                                gender = reader["gender"].ToString(),
                                email = reader["email"].ToString(),
                            };

                            return Ok(employee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Edit data
        [HttpPut]
        [Route("api/employee/{id}", Name = "Update_Employee")]
        public IHttpActionResult UpdateEmployee(/*string id, */[FromBody] Employee employee)
        {
            //if (string.IsNullOrEmpty(id))
            //    return BadRequest("ID is required.");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(
                    ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
                {
                    conn.Open();

                    string query = @"
                UPDATE employee
                SET email = @email
                WHERE employee_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", employee.employee_id);
                        cmd.Parameters.AddWithValue("@email", employee.email);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return Content(HttpStatusCode.BadRequest, "Invalid data.");
                    }
                }

                return Ok("Successfully updated.");
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.InternalServerError,
                    "Error updating data: " + ex.Message
                );
            }
        }

    }
}
