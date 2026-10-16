using System.Data.SqlClient;
using Microsoft.AspNetCore.Http; //to use HttpContext and session
using Microsoft.Data.SqlClient;


namespace RaceDayAPI.Controllers
{
  public class login_backend
    {
        //start of login_backend class

        //creating a method to check if the user exists in the database
        //using SqlConnection and SqlCommand, verify the password,
        //start a session and return the user's details

        public string login_user(string login_email, string login_password, HttpContext httpContext)
        {
            string message = "Failed to login, try again later...";
            string connectionString = @"Data Source=(localdb)\Raceday_API;Initial Catalog=Raceday_API;Integrated Security=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connect = new SqlConnection(connectionString))
                {
                    connect.Open();

                    string query = @"SELECT id, fullname, email, role, password
                             FROM register_and_login WHERE email = @email";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@email", login_email);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int id = Convert.ToInt32(reader["id"]);
                                string fullname = reader["fullname"].ToString()??"";
                                string email = reader["email"].ToString()??"";
                                string role = reader["role"].ToString()??"";
                                string hashedPassword = reader["password"].ToString()??"";

                                if (BCrypt.Net.BCrypt.Verify(login_password, hashedPassword))
                                {
                                    httpContext.Session.SetInt32("user_id", id);
                                    httpContext.Session.SetString("user_email", email);
                                    httpContext.Session.SetString("user_fullname", fullname);
                                    httpContext.Session.SetString("user_role", role);

                                    message = "Login successful. Welcome " + fullname +
                                              " | ID: " + id +
                                              " | Email: " + email +
                                              " | Role: " + role;
                                }
                                else
                                {
                                    message = "Invalid email or password.";
                                }
                            }
                            else
                            {
                                message = "Invalid email or password.";
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }

    }
}