using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly string _connectionString;
        private const string SelectColumns = "TableId, TableNumber, NumberOfSeats, Status, CurrentOrderId, OccupiedSince, LastUpdated, Area, IsActive";

        public TableRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        public IEnumerable<RestaurantTable> GetAllTables()
        {
            var tables = new List<RestaurantTable>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Tables WHERE IsActive = 1 ORDER BY TableId", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    tables.Add(MapTableFromReader(reader));
            }
            catch (SqlException)
            {
                return tables;
            }
            return tables;
        }

        public RestaurantTable? GetById(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand($"SELECT {SelectColumns} FROM dbo.Tables WHERE TableId = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapTableFromReader(reader) : null;
            }
            catch (SqlException)
            {
                return null;
            }
        }

        public void UpdateStatus(int id, TableStatus status, int? currentOrderId = null)
        {
            const string query = @"UPDATE dbo.Tables
                                   SET Status = @status,
                                       CurrentOrderId = @orderId,
                                       LastUpdated = SYSUTCDATETIME(),
                                       OccupiedSince = CASE
                                           WHEN @status = 1 THEN ISNULL(OccupiedSince, SYSUTCDATETIME())
                                           ELSE NULL
                                       END
                                   WHERE TableId = @id";
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", (int)status);
                cmd.Parameters.AddWithValue("@orderId", (object?)currentOrderId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                // status update failed — table display remains unchanged
            }
        }

        private static RestaurantTable MapTableFromReader(SqlDataReader reader)
        {
            return new RestaurantTable
            {
                TableId       = reader.GetInt32(reader.GetOrdinal("TableId")),
                TableNumber   = reader.IsDBNull(reader.GetOrdinal("TableNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("TableNumber")),
                NumberOfSeats = reader.IsDBNull(reader.GetOrdinal("NumberOfSeats")) ? 0 : reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                Status        = (TableStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                CurrentOrderId = reader.IsDBNull(reader.GetOrdinal("CurrentOrderId")) ? null : reader.GetInt32(reader.GetOrdinal("CurrentOrderId")),
                OccupiedSince = reader.IsDBNull(reader.GetOrdinal("OccupiedSince")) ? null : reader.GetDateTime(reader.GetOrdinal("OccupiedSince")),
                LastUpdated   = reader.GetDateTime(reader.GetOrdinal("LastUpdated")),
                Area          = reader.IsDBNull(reader.GetOrdinal("Area")) ? null : reader.GetString(reader.GetOrdinal("Area")),
                IsActive      = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}
