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
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock, IsActive FROM dbo.MenuItems ORDER BY Course, Name";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    menuItems.Add(ReadMenuItem(reader));
            }
            return menuItems;
        }

        public List<MenuItem> GetFiltered(MenuItemType? type, CourseType? course, CardType? card)
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock, IsActive FROM dbo.MenuItems WHERE 1=1";

            if (type.HasValue)   query += " AND Type = @Type";
            if (course.HasValue) query += " AND Course = @Course";
            if (card.HasValue)   query += " AND Card = @Card";
            query += " ORDER BY Course, Name";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (type.HasValue)   command.Parameters.AddWithValue("@Type",   (int)type.Value);
                if (course.HasValue) command.Parameters.AddWithValue("@Course", (int)course.Value);
                if (card.HasValue)   command.Parameters.AddWithValue("@Card",   (int)card.Value);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    menuItems.Add(ReadMenuItem(reader));
            }
            return menuItems;
        }

        public MenuItem? GetById(int menuItemId)
        {
            string query = "SELECT MenuItemId, Name, Price, Type, Course, Card, VatRate, Stock, IsActive FROM dbo.MenuItems WHERE MenuItemId = @MenuItemId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemId", menuItemId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? ReadMenuItem(reader) : null;
        }

        public void DecreaseStock(int menuItemId, int quantity)
        {
            string query = @"UPDATE dbo.MenuItems SET Stock = Stock - @Quantity
                             WHERE MenuItemId = @MenuItemId AND Stock >= @Quantity";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void IncreaseStock(int menuItemId, int quantity)
        {
            string query = "UPDATE dbo.MenuItems SET Stock = Stock + @Quantity WHERE MenuItemId = @MenuItemId";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // Management methods

        public void Add(MenuItem item)
        {
            string query = @"INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock, IsActive)
                             VALUES (@Name, @Price, @Type, @Course, @Card, @VatRate, @Stock, @IsActive)";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name",     item.Name);
            cmd.Parameters.AddWithValue("@Price",    item.Price);
            cmd.Parameters.AddWithValue("@Type",     (int)item.Type);
            cmd.Parameters.AddWithValue("@Course",   (int)item.Course);
            cmd.Parameters.AddWithValue("@Card",     (int)item.Card);
            cmd.Parameters.AddWithValue("@VatRate",  item.VatRate);
            cmd.Parameters.AddWithValue("@Stock",    item.Stock);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Update(MenuItem item)
        {
            string query = @"UPDATE dbo.MenuItems
                             SET Name=@Name, Price=@Price, Type=@Type, Course=@Course,
                                 Card=@Card, VatRate=@VatRate, Stock=@Stock, IsActive=@IsActive
                             WHERE MenuItemId=@MenuItemId";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", item.MenuItemId);
            cmd.Parameters.AddWithValue("@Name",       item.Name);
            cmd.Parameters.AddWithValue("@Price",      item.Price);
            cmd.Parameters.AddWithValue("@Type",       (int)item.Type);
            cmd.Parameters.AddWithValue("@Course",     (int)item.Course);
            cmd.Parameters.AddWithValue("@Card",       (int)item.Card);
            cmd.Parameters.AddWithValue("@VatRate",    item.VatRate);
            cmd.Parameters.AddWithValue("@Stock",      item.Stock);
            cmd.Parameters.AddWithValue("@IsActive",   item.IsActive);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void SetActive(int menuItemId, bool isActive)
        {
            string query = "UPDATE dbo.MenuItems SET IsActive=@IsActive WHERE MenuItemId=@MenuItemId";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@IsActive",   isActive);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateStock(int menuItemId, int newStock)
        {
            string query = "UPDATE dbo.MenuItems SET Stock=@Stock WHERE MenuItemId=@MenuItemId";
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Stock",      newStock);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            int isActiveOrd = reader.GetOrdinal("IsActive");
            return new MenuItem
            {
                MenuItemId = (int)reader["MenuItemId"],
                Name       = (string)reader["Name"],
                Price      = (decimal)reader["Price"],
                Type       = (MenuItemType)(int)reader["Type"],
                Course     = (CourseType)(int)reader["Course"],
                Card       = (CardType)(int)reader["Card"],
                VatRate    = (decimal)reader["VatRate"],
                Stock      = (int)reader["Stock"],
                IsActive   = !reader.IsDBNull(isActiveOrd) && reader.GetBoolean(isActiveOrd)
            };
        }
    }
}
