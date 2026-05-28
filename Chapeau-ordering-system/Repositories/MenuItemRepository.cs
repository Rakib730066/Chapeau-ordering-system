using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly string _connectionString;

        public MenuItemRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        // Get all menu items from the database
        public List<MenuItem> GetAll()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Stock FROM MenuItems ORDER BY Course, Name";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        menuItems.Add(ReadMenuItem(reader));
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching menu items: " + ex.Message);
            }
            return menuItems;
        }

        // Get menu items filtered by type and/or course — filtering is done in SQL with WHERE clause
        public List<MenuItem> GetFiltered(MenuItemType? type, CourseType? course)
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            // Build query dynamically based on which filters are active
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Stock FROM MenuItems WHERE 1=1";

            if (type.HasValue)
                query += " AND Type = @Type";

            if (course.HasValue)
                query += " AND Course = @Course";

            query += " ORDER BY Course, Name";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (type.HasValue)
                        command.Parameters.AddWithValue("@Type", (int)type.Value);

                    if (course.HasValue)
                        command.Parameters.AddWithValue("@Course", (int)course.Value);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        menuItems.Add(ReadMenuItem(reader));
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching filtered menu items: " + ex.Message);
            }
            return menuItems;
        }

        // Get a single menu item by its id
        public MenuItem? GetById(int menuItemId)
        {
            MenuItem? menuItem = null;
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Stock FROM MenuItems WHERE MenuItemId = @MenuItemId";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        menuItem = ReadMenuItem(reader);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching menu item by id: " + ex.Message);
            }
            return menuItem;
        }

        // Helper method to map a database row to a MenuItem object — avoids duplicate code
        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemId = (int)reader["MenuItemId"],
                Name = (string)reader["Name"],
                Price = (decimal)reader["Price"],
                Type = (MenuItemType)(int)reader["Type"],
                Course = (CourseType)(int)reader["Course"],
                Stock = (int)reader["Stock"]
            };
        }
    }
}