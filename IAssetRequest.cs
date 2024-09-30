using Hexa_Hub.DTO;
using static Hexa_Hub.Models.MultiValues;


namespace Hexa_Hub.Interface
{
    public interface IAssetRequest
    {
        Task<List<AssetRequest>> GetAllAssetRequests();
        Task<AssetRequest?> GetAssetRequestById(int id);
        Task AddAssetRequest(AssetRequestDto dto);
        Task<List<AssetRequest>> GetAssetRequestByMonthAsync(string month);
        Task<List<AssetRequest>> GetAssetRequestByYearAsync(int year);
        Task<List<AssetRequest>> GetAssetRequestByMonthAndYearAsync(string month, int year);
        Task<List<AssetRequest>> GetAssetRequestByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<IEnumerable<AssetRequestDto>> GetAssetRequestByStatus(RequestStatus status);
        Task<AssetRequest> UpdateAssetRequest(int id, AssetRequestDto assetRequestDto);
        Task DeleteAssetRequest(int id);
        Task Save();
        Task<List<AssetRequest>> GetAssetRequestsByUserId(int userId);
    }

}
