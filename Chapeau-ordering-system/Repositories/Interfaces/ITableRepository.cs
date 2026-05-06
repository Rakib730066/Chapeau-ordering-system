namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface ITableRepository
    {
        Task<List<Table>> GetAllTablesAsync();
        Task<Table> GetTableByIdAsync(int id);
    }
}
