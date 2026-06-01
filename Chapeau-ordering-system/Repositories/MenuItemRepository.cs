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

        public List<MenuItem> GetAll()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock FROM dbo.MenuItems ORDER BY Course, Name";

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

        public List<MenuItem> GetFiltered(MenuItemType? type, CourseType? course, CardType? card)
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock FROM dbo.MenuItems WHERE 1=1";

            if (type.HasValue)
                query += " AND Type = @Type";

            if (course.HasValue)
                query += " AND Course = @Course";

            if (card.HasValue)
                query += " AND Card = @Card";

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

                    if (card.HasValue)
                        command.Parameters.AddWithValue("@Card", (int)card.Value);

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

        public MenuItem? GetById(int menuItemId)
        {
            MenuItem? menuItem = null;
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock FROM dbo.MenuItems WHERE MenuItemId = @MenuItemId";

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

        public void DecreaseStock(int menuItemId, int quantity)
        {
            string query = @"UPDATE dbo.MenuItems
                             SET    Stock = Stock - @Quantity
                             WHERE  MenuItemId  = @MenuItemId
                             AND    Stock      >= @Quantity";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error decreasing stock: " + ex.Message);
            }
        }

        public void IncreaseStock(int menuItemId, int quantity)
        {
            string query = @"UPDATE dbo.MenuItems
                             SET    Stock = Stock + @Quantity
                             WHERE  MenuItemId = @MenuItemId";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemId", menuItemId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error increasing stock: " + ex.Message);
            }
        }

        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemId = (int)reader["MenuItemId"],
                Name = (string)reader["Name"],
                Price = (decimal)reader["Price"],
                Type = (MenuItemType)(int)reader["Type"],
                Course = (CourseType)(int)reader["Course"],
                Card = (CardType)(int)reader["Card"],
                VatRate = (decimal)reader["VatRate"],
                Stock = (int)reader["Stock"]
            };
        }
    }
}