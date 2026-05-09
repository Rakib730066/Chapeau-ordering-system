using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        public Employee? GetByUsername(string username)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);


            // Allow login by username or by employee number
            bool isNumeric = int.TryParse(username, out int employeeNumber);
            string query;
            SqlCommand cmd;

            if (isNumeric)
            {
                query = @"SELECT EmployeeId, Username, PasswordHash, FirstName, LastName, Email, Role, IsActive
                             FROM dbo.Employees
                             WHERE EmployeeId = @EmployeeId AND IsActive = 1";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@EmployeeId", System.Data.SqlDbType.Int).Value = employeeNumber;
            }
            else
            {
                query = @"SELECT EmployeeId, Username, PasswordHash, FirstName, LastName, Email, Role, IsActive
                             FROM dbo.Employees
                             WHERE Username = @Username AND IsActive = 1";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
            }
            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                // use ordinals and typed access to avoid invalid cast issues
                int idOrd = reader.GetOrdinal("EmployeeId");
                int userOrd = reader.GetOrdinal("Username");
                int passOrd = reader.GetOrdinal("PasswordHash");
                int firstOrd = reader.GetOrdinal("FirstName");
                int lastOrd = reader.GetOrdinal("LastName");
                int roleOrd = reader.GetOrdinal("Role");
                int emailOrd = reader.GetOrdinal("Email");
                int isActiveOrd = reader.GetOrdinal("IsActive");

                int employeeId = reader.IsDBNull(idOrd) ? 0 : reader.GetInt32(idOrd);
                string usernameDb = reader.IsDBNull(userOrd) ? string.Empty : reader.GetString(userOrd);
                string passwordHash = reader.IsDBNull(passOrd) ? string.Empty : reader.GetString(passOrd);
                string firstName = reader.IsDBNull(firstOrd) ? string.Empty : reader.GetString(firstOrd);
                string lastName = reader.IsDBNull(lastOrd) ? string.Empty : reader.GetString(lastOrd);
                string email = reader.IsDBNull(emailOrd) ? string.Empty : reader.GetString(emailOrd);
                bool isActive = reader.IsDBNull(isActiveOrd) ? true : reader.GetBoolean(isActiveOrd);

                // Role column may be stored as INT or as text containing a number or name.
                EmployeeRole role;
                if (reader.IsDBNull(roleOrd))
                {
                    role = default;
                }
                else
                {
                    object roleObj = reader.GetValue(roleOrd);
                    if (roleObj is int ri)
                    {
                        role = (EmployeeRole)ri;
                    }
                    else if (roleObj is long rl)
                    {
                        role = (EmployeeRole)Convert.ToInt32(rl);
                    }
                    else if (roleObj is string rs)
                    {
                        // try numeric parse first, then enum name parse
                        if (int.TryParse(rs, out int parsed))
                        {
                            role = (EmployeeRole)parsed;
                        }
                        else if (Enum.TryParse<EmployeeRole>(rs, true, out var parsedEnum))
                        {
                            role = parsedEnum;
                        }
                        else
                        {
                            // fallback to default
                            role = default;
                        }
                    }
                    else
                    {
                        // last resort conversion
                        role = (EmployeeRole)Convert.ToInt32(roleObj);
                    }
                }

                return new Employee
                {
                    EmployeeId = employeeId,
                    Username = usernameDb,
                    PasswordHash = passwordHash,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    IsActive = isActive,
                    Role = role
                };
            }

            return null;
        }
    }
}