using Hexa_Hub.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Hexa_Hub.Exceptions;
using Hexa_Hub.DTO;
using System.Text;
using static Hexa_Hub.Models.MultiValues;

namespace Hexa_Hub.Repository
{
    public class AssetService : IAsset
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public AssetService(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        //public async Task<List<Asset>> GetAllAssets()
        //{
        //    return await _context.Assets
        //                         //.Include(a => a.Category)
        //                         //.Include(a => a.SubCategories)
        //                         .ToListAsync();
        //    //var assets = await _context.Assets
        //    //                    .Include(a => a.Category)
        //    //                    .Include(a => a.SubCategories)
        //    //                    .ToListAsync();

        //    //return assets.Select(a => new Asset
        //    //{
        //    //    AssetId = a.AssetId,
        //    //    AssetName = a.AssetName,
        //    //    AssetDescription = a.AssetDescription,
        //    //    CategoryId = a.CategoryId,
        //    //    SubCategoryId = a.SubCategoryId,
        //    //    AssetImage = a.AssetImage,
        //    //    SerialNumber = a.SerialNumber,
        //    //    Model = a.Model,
        //    //    ManufacturingDate = a.ManufacturingDate,
        //    //    Location = a.Location,
        //    //    Value = a.Value,
        //    //    Expiry_Date = a.Expiry_Date,
        //    //    Asset_Status = a.Asset_Status,
        //    //    Category = a.Category,
        //    //    SubCategories = a.SubCategories
        //    //}).ToList();

        //}

        public async Task<List<AssetDtoClass>> GetAssetsAll()
        {

 
            return await _context.Assets
         .Include(a => a.Category)
         .Include(a => a.SubCategories)
         .Select(a => new AssetDtoClass
         {
             AssetId = a.AssetId,
             AssetName = a.AssetName,
             Location = a.Location,
             Value = a.Value,
             Model=a.Model,
             AssetDescription=a.AssetDescription,
             SerialNumber=a.SerialNumber,
             CategoryName = a.Category.CategoryName,
             CategoryId=a.Category.CategoryId,  
             SubCategoryId=a.SubCategoryId, 
             SubCategoryName = a.SubCategories.SubCategoryName,
             AssetStatus = a.Asset_Status,

         })
         .ToListAsync();
        }
        public async Task<List<Asset>> GetAllAssets()
        {
            return await _context.Assets
                                 .ToListAsync();
        }

        //return assets.Select(a => new AssetDto
        //{
        //    AssetId = a.AssetId,
        //    AssetName = a.AssetName,
        //    AssetDescription = a.AssetDescription,
        //    CategoryId = a.CategoryId,
        //    SubCategoryId = a.SubCategoryId,
        //    AssetImage = a.AssetImage,
        //    SerialNumber = a.SerialNumber,
        //    Model = a.Model,
        //    ManufacturingDate = a.ManufacturingDate,
        //    Location = a.Location,
        //    Value = a.Value,
        //    Expiry_Date = a.Expiry_Date,
        //    Asset_Status = a.Asset_Status?.ToString()
        //}).ToList();
        //}

        public async Task<List<Asset>> GetAllDetailsOfAssets()
        {
            return await _context.Assets
                                 .Include(a => a.Category)
                                 .Include(a => a.SubCategories)
                                 .Include(a => a.AssetRequests)
                                 .Include(a => a.ServiceRequests)
                                 .Include(a => a.MaintenanceLogs)
                                 .Include(a => a.Audits)
                                 .Include(a => a.ReturnRequests)
                                 .Include(a => a.AssetAllocations)
                                 .ToListAsync();
        }

        public async Task<Asset?> GetAssetById(int id)
        {
            return await _context.Assets
                                 .FirstOrDefaultAsync(a => a.AssetId == id);
        }

        public async Task<AssetDtoClass?> GetAssetByAssetId(int id)
        {
            return await _context.Assets
                                 .Include(a => a.Category)
                                 .Include(a => a.SubCategories)
                                  .Select(a => new AssetDtoClass
                                  {
                                      AssetId = a.AssetId,
                                      AssetName = a.AssetName,
                                      AssetDescription = a.AssetDescription,
                                      Location = a.Location,
                                      Value = a.Value,
                                      CategoryName = a.Category.CategoryName,
                                      SubCategoryName = a.SubCategories.SubCategoryName,
                                      AssetStatus = a.Asset_Status ?? AssetStatus.OpenToRequest,
                                      SerialNumber = a.SerialNumber,
                                      Model = a.Model,
                                      ManufacturingDate = a.ManufacturingDate,
                                      Expiry_Date = a.Expiry_Date,

                                  })
                                 .FirstOrDefaultAsync(a => a.AssetId == id);
        }


        //public async Task<Asset> UpdateAssetDto(int id, AssetUpdateDto assetDto)
        //{
        //    byte[]? assetImageBytes = null;

        //    if (assetDto.AssetImage != null)
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await assetDto.AssetImage.CopyToAsync(memoryStream);
        //            assetImageBytes = memoryStream.ToArray();
        //        }
        //    }

        //    var existingAsset = await _context.Assets.FindAsync(id);
        //    if (existingAsset == null)
        //    {
        //        throw new AssetNotFoundException($"Asset with ID {id} not found");
        //    }

        //    existingAsset.AssetName = assetDto.AssetName;
        //    existingAsset.AssetDescription = assetDto.AssetDescription;
        //    existingAsset.CategoryId = assetDto.CategoryId;
        //    existingAsset.SubCategoryId = assetDto.SubCategoryId;
        //    //existingAsset.AssetImage = assetImageBytes ?? existingAsset.AssetImage;
        //    //existingAsset.AssetImage = assetImageBytes;
        //    existingAsset.SerialNumber = assetDto.SerialNumber;
        //    existingAsset.Model = assetDto.Model;
        //    existingAsset.ManufacturingDate = assetDto.ManufacturingDate;
        //    existingAsset.Location = assetDto.Location;
        //    existingAsset.Value = assetDto.Value;
        //    existingAsset.Expiry_Date = assetDto.Expiry_Date;


        //    _context.Assets.Update(existingAsset);
        //    return existingAsset;
        //}


        public async Task<Asset> UpdateAssetDto(int id, AssetUpdateDto assetDto)
        {
            byte[]? assetImageBytes = null;
            var existingAsset = await _context.Assets.FindAsync(id);
            if (assetDto.AssetImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await assetDto.AssetImage.CopyToAsync(memoryStream);
                    assetImageBytes = memoryStream.ToArray();
                }

                // Save the image bytes to the asset object (or handle saving it to disk)
                existingAsset.AssetImage = assetImageBytes;
            }


            if (existingAsset == null)
            {
                throw new AssetNotFoundException($"Asset with ID {id} not found");
            }

            // Update asset properties
            existingAsset.AssetName = assetDto.AssetName;
            existingAsset.AssetDescription = assetDto.AssetDescription;
            existingAsset.CategoryId = assetDto.CategoryId;
            existingAsset.SubCategoryId = assetDto.SubCategoryId;
            existingAsset.SerialNumber = assetDto.SerialNumber;
            existingAsset.Model = assetDto.Model;
            existingAsset.ManufacturingDate = assetDto.ManufacturingDate;
            existingAsset.Location = assetDto.Location;
            existingAsset.Value = assetDto.Value;
            existingAsset.Expiry_Date = assetDto.Expiry_Date;

            _context.Assets.Update(existingAsset);
            await _context.SaveChangesAsync(); // Make sure to save changes
            return existingAsset;
        }

        public async Task<Asset> UpdateAsset(Asset asset)
        {
            _context.Assets.Update(asset);
            return asset;
        }

        public async Task<IEnumerable<AssetDto>> GetAssetByName(string name)
        {
            var assetDetails = await (from asset in _context.Assets
                                      where EF.Functions.Like(asset.AssetName, $"%{name}%")
                                      select new AssetDto
                                      {
                                          //AssetId = asset.AssetId,
                                          AssetName = asset.AssetName,
                                          AssetDescription = asset.AssetDescription,
                                          ManufacturingDate = asset.ManufacturingDate,
                                          Location = asset.Location,
                                          Value = asset.Value,
                                          Expiry_Date = asset.Expiry_Date,
                                          Asset_Status = asset.Asset_Status.ToString(),
                                          CategoryId = asset.CategoryId,
                                          SubCategoryId = asset.SubCategoryId,
                                          SerialNumber = asset.SerialNumber,
                                          Model = asset.Model
                                      }).ToListAsync();

            return assetDetails;
        }

        public async Task<IEnumerable<AssetDto>> GetAssetsByValue(decimal minPrice, decimal maxPrice)
        {
            var assetsInRange = await (from asset in _context.Assets
                                       where asset.Value >= minPrice && asset.Value <= maxPrice
                                       select new AssetDto
                                       {
                                           //AssetId = asset.AssetId,
                                           AssetName = asset.AssetName,
                                           AssetDescription = asset.AssetDescription,
                                           //AssetImage = asset.AssetImage,
                                           ManufacturingDate = asset.ManufacturingDate,
                                           Location = asset.Location,
                                           Value = asset.Value,
                                           Expiry_Date = asset.Expiry_Date,
                                           Asset_Status = asset.Asset_Status.ToString(),
                                           CategoryId = asset.CategoryId,
                                           SubCategoryId = asset.SubCategoryId,
                                           SerialNumber = asset.SerialNumber,
                                           Model = asset.Model
                                       }).ToListAsync();

            return assetsInRange;
        }

        public async Task<IEnumerable<AssetDto>> GetAssetsByLocation(string location)
        {
            var assetDetails = await (from asset in _context.Assets
                                      where EF.Functions.Like(asset.Location, $"%{location}%")
                                      select new AssetDto
                                      {
                                          //AssetId = asset.AssetId,
                                          AssetName = asset.AssetName,
                                          AssetDescription = asset.AssetDescription,
                                          //AssetImage = asset.AssetImage,
                                          ManufacturingDate = asset.ManufacturingDate,
                                          Location = asset.Location,
                                          Value = asset.Value,
                                          Expiry_Date = asset.Expiry_Date,
                                          Asset_Status = asset.Asset_Status.ToString(),
                                          CategoryId = asset.CategoryId,
                                          SubCategoryId = asset.SubCategoryId,
                                          SerialNumber = asset.SerialNumber,
                                          Model = asset.Model
                                      }).ToListAsync();

            return assetDetails;
        }

        public async Task<IEnumerable<AssetDto>> GetAssetsByStatus(AssetStatus status)
        {
            var assetsByStatus = await (from asset in _context.Assets
                                        where asset.Asset_Status == status
                                        select new AssetDto
                                        {
                                            //AssetId = asset.AssetId,
                                            AssetName = asset.AssetName,
                                            AssetDescription = asset.AssetDescription,
                                            //AssetImage = asset.AssetImage,
                                            ManufacturingDate = asset.ManufacturingDate,
                                            Location = asset.Location,
                                            Value = asset.Value,
                                            Expiry_Date = asset.Expiry_Date,
                                            Asset_Status = asset.Asset_Status.ToString(),
                                            CategoryId = asset.CategoryId,
                                            SubCategoryId = asset.SubCategoryId,
                                            SerialNumber = asset.SerialNumber,
                                            Model = asset.Model
                                        }).ToListAsync();

            return assetsByStatus;
        }


        public async Task DeleteAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                throw new AssetNotFoundException($"Asset with ID {id} Not Found");
            }

            _context.Assets.Remove(asset);

        }
        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        //public async Task<string?> UploadAssetImageAsync(int assetId, IFormFile file)
        //{
        //    var asset = await _context.Assets.FindAsync(assetId);
        //    if(asset == null)
        //    {
        //        return null;
        //    }
        //    const string assetImageFolder = "AssetImages";
        //    string ImagePath = Path.Combine(Directory.GetCurrentDirectory(), assetImageFolder);
        //    if (!Directory.Exists(ImagePath))
        //    {
        //        Directory.CreateDirectory(ImagePath);
        //    }
        //    string fileName;
        //    if (asset.AssetImage == null && file == null)
        //    {
        //        fileName = "AssetDefault.jpg";
        //    }
        //    else if(file != null)
        //    {
        //        string fileExtension = Path.GetExtension(file.FileName);
        //        fileName = $"{assetId}{fileExtension}";
        //        string fullPath = Path.Combine(ImagePath, fileName);
        //        using (var stream = new FileStream(fullPath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }
        //    }
        //    else
        //    {
        //        return Encoding.UTF8.GetString(asset.AssetImage);
        //    }
        //    asset.AssetImage = Encoding.UTF8.GetBytes(fileName);
        //    await _context.SaveChangesAsync();
        //    return fileName;
        //}

        public async Task<string?> UploadAssetImageAsync(int assetId, IFormFile file)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return null;
            }

            if (file != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    asset.AssetImage = memoryStream.ToArray(); // Store as byte array
                }
            }

            await _context.SaveChangesAsync();
            return "Image uploaded successfully"; // Return a success message or other relevant info
        }

        //public async Task<string?> UploadAssetImageAsync(int assetId, IFormFile file)
        //{
        //    var asset = await _context.Assets.FindAsync(assetId);
        //    if (asset == null)
        //    {
        //        return null;
        //    }
        //    const string assetImageFolder = "AssetImages";
        //    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), assetImageFolder);

        //    if (!Directory.Exists(imagePath))
        //    {
        //        Directory.CreateDirectory(imagePath);
        //    }

        //    string fileName;
        //    if (file != null)
        //    {
        //        string fileExtension = Path.GetExtension(file.FileName);
        //        fileName = $"{assetId}{fileExtension}";
        //        string fullPath = Path.Combine(imagePath, fileName);

        //        using (var stream = new FileStream(fullPath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        asset.AssetImage = Encoding.UTF8.GetBytes(fileName);
        //    }
        //    else
        //    {
        //        fileName = Encoding.UTF8.GetString(asset.AssetImage) ?? "AssetDefault.jpg";
        //    }

        //    await _context.SaveChangesAsync();
        //    return fileName;
        //}

        public async Task<Asset> AddAsset(AssetDto assetDto)
        {
            // Convert IFormFile to byte[]
            byte[]? assetImageBytes = null;

            if (assetDto.AssetImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await assetDto.AssetImage.CopyToAsync(memoryStream);
                    assetImageBytes = memoryStream.ToArray();
                }
            }

            var asset = new Asset
            {
                AssetName = assetDto.AssetName,
                AssetDescription = assetDto.AssetDescription,
                CategoryId = assetDto.CategoryId,
                SubCategoryId = assetDto.SubCategoryId,
                SerialNumber = assetDto.SerialNumber,
                Model = assetDto.Model,
                ManufacturingDate = assetDto.ManufacturingDate,
                Location = assetDto.Location,
                Value = assetDto.Value,
                Expiry_Date = assetDto.Expiry_Date,
                AssetImage = assetImageBytes
            };

            await _context.AddAsync(asset);
            await _context.SaveChangesAsync();

            return asset;
        }

        //public async Task<Asset> AddAsset(AssetDto assetDto)
        //{
        //    var asset = new Asset
        //    {
        //        AssetName = assetDto.AssetName,
        //        AssetDescription = assetDto.AssetDescription,
        //        CategoryId = assetDto.CategoryId,
        //        SubCategoryId = assetDto.SubCategoryId,
        //        SerialNumber = assetDto.SerialNumber,
        //        Model = assetDto.Model,
        //        ManufacturingDate = assetDto.ManufacturingDate,
        //        Location = assetDto.Location,
        //        Value = assetDto.Value,
        //        Expiry_Date = assetDto.Expiry_Date
        //    };

        //    await _context.AddAsync(asset);
        //    await _context.SaveChangesAsync();

        //    // Set default image if no image is provided
        //    if (assetDto.AssetImage == null)
        //    {
        //        const string defaultImageFileName = "AssetDefault.jpg";
        //        asset.AssetImage = Encoding.UTF8.GetBytes(defaultImageFileName);
        //    }
        //    else
        //    {
        //        asset.AssetImage = assetDto.AssetImage; // Set the uploaded image
        //    }

        //    _context.Assets.Update(asset);
        //    await _context.SaveChangesAsync();

        //    return asset;
        //}
        private string GetDefaultAssetImagePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "AssetImages", "AssetDefault.jpg");
        }

        public string GetImagePath(string fileName)
        {
            return Path.Combine("AssetImages", fileName);
        }

    }

}
