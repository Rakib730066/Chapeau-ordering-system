using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        private const string SelectColumns = "EmployeeId, Username, PasswordHash, FirstName, LastName, Email, Role, IsActive";

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        public Employee? GetByUsername(string username)
        {
            bool isNumeric = int.TryParse(username, out int employeeId);
            string whereClause = isNumeric ? "EmployeeId = @param" : "Username = @param";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Employees WHERE {whereClause} AND IsActive = 1", conn);
            cmd.Parameters.AddWithValue("@param", isNumeric ? (object)employeeId : username);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapEmployeeFromReader(reader) : null;
        }

        private static Employee MapEmployeeFromReader(SqlDataReader reader)
        {
            return new Employee
            {
                EmployeeId   = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                Username     = reader.IsDBNull(reader.GetOrdinal("Username")) ? string.Empty : reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.IsDBNull(reader.GetOrdinal("PasswordHash")) ? string.Empty : reader.GetString(reader.GetOrdinal("PasswordHash")),
                FirstName    = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FirstName")),
                LastName     = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader.GetString(reader.GetOrdinal("LastName")),
                Email        = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                Role         = (EmployeeRole)reader.GetInt32(reader.GetOrdinal("Role")),
                IsActive     = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}
