using Hexa_Hub.DTO;
using static Hexa_Hub.Models.MultiValues;

namespace Hexa_Hub.Interface
{
    public interface IAsset
    {
        Task<List<Asset>> GetAllAssets();
        //Task<List<AssetDto>> GetAllAssets();
        Task<List<AssetDtoClass>> GetAssetsAll();
        Task<Asset?> GetAssetById(int id);
        Task<AssetDtoClass?> GetAssetByAssetId(int id);
        Task<List<Asset>> GetAllDetailsOfAssets();

        Task<Asset> AddAsset(AssetDto assetDto);
        Task<Asset> UpdateAsset(Asset asset);
        Task<Asset> UpdateAssetDto(int id, AssetUpdateDto assetDto);
        Task DeleteAsset(int id);
        Task Save();
        Task<string?> UploadAssetImageAsync(int assetId, IFormFile file);
        public string GetImagePath(string fileName);

        Task<IEnumerable<AssetDto>> GetAssetByName(string name);

        Task<IEnumerable<AssetDto>> GetAssetsByValue(decimal minPrice, decimal maxprice);

        Task<IEnumerable<AssetDto>> GetAssetsByLocation(string location);

        Task<IEnumerable<AssetDto>> GetAssetsByStatus(AssetStatus status);
    }

}
