namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface ITableService
    {
        Task<List<Table>> GetAllTablesAsync();
        Task<Table> GetTableByIdAsync(int id);
    }
}
