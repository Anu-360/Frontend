using Hexa_Hub.DTO;

namespace Hexa_Hub.Interface
{
    public interface IAssetAllocation
    {
        Task<List<AllocationDto>> GetAllAllocations();
        Task<AssetAllocation?> GetAllocationById(int id);
        Task<IEnumerable<AllocationDto>> GetAllocationsByUserIdAsync(int userId);
        Task<List<AssetAllocation>> GetAllocationsByMonthAsync(string month);
        Task<List<AssetAllocation>> GetAllocationsByYearAsync(int year);
        Task<List<AssetAllocation>> GetAllocationsByMonthAndYearAsync(string month, int year);
        Task<List<AssetAllocation>> GetAllocationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task AddAllocation(AssetAllocation allocation);

        //Task<AssetAllocation> AllocateAssetAsync(AssetAllocationDto allocationDto,int adminUserId);
        Task DeleteAllocation(int id);
        Task Save();
        Task<List<AssetAllocation>> GetAllocationListById(int userId);
    }

}
