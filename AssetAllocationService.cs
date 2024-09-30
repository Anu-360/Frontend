using Hexa_Hub.Interface;
using Microsoft.EntityFrameworkCore;
using Hexa_Hub.Exceptions;
using Hexa_Hub.DTO;
using static Hexa_Hub.Models.MultiValues;
using static Hexa_Hub.Repository.AssetAllocationService;
using System.Configuration;

namespace Hexa_Hub.Repository
{
    public class AssetAllocationService : IAssetAllocation
    {
        private readonly DataContext _context;
        private readonly IEmail _email;
        private readonly IConfiguration _configuration;  // For accessing appsettings

        public AssetAllocationService(DataContext context, IEmail email,IConfiguration configuration)
        {
            _context = context;
            _email = email;
            _configuration = configuration;
        }

        public async Task<List<AllocationDto>> GetAllAllocations()
        {
            return await _context.AssetAllocations
                .Include(aa => aa.Asset)
                    .ThenInclude(asset => asset.Category)
                .Include(aa => aa.User)
                .Select(aa => new AllocationDto
                {
                    AllocatedDate = aa.AllocatedDate,
                    AssetName = aa.Asset != null ? aa.Asset.AssetName : "No Asset Name",
                    Value = aa.Asset != null ? aa.Asset.Value : 0,  // Default to 0 if Asset is null
                    Asset_Status = aa.Asset != null && aa.Asset.Asset_Status.HasValue ? aa.Asset.Asset_Status.ToString() : "Not Available",
                    CategoryName = aa.Asset != null && aa.Asset.Category != null ? aa.Asset.Category.CategoryName : "No Category"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AllocationDto>> GetAllocationsByUserIdAsync(int userId)
        {
            var allocations = await _context.AssetAllocations
                .Include(a => a.Asset) // Include the related Asset entity
                .Where(a => a.UserId == userId)
                .Select(a => new AllocationDto
                {
                    UserId = a.UserId,
                    AssetName = a.Asset.AssetName,
                    AssetId = a.Asset.AssetId,  
                    CategoryName=a.Asset.Category.CategoryName,
                    CategoryId = a.Asset.Category.CategoryId,
                    Value = a.Asset.Value,
                    Model = a.Asset.Model,
                    AllocatedDate = a.AllocatedDate
                })
                .ToListAsync();

            return allocations;
        }


        public async Task<List<AssetAllocation>> GetAllocationsByMonthAsync(string month)
        {
            var monthname = DateTime.ParseExact(month, "MMMM", null).Month;
            return await _context.AssetAllocations
                                 .Where(a => a.AllocatedDate.Month == monthname)
                                 .ToListAsync();
        }

        public async Task<List<AssetAllocation>> GetAllocationsByYearAsync(int year)
        {
            return await _context.AssetAllocations
                                 .Where(a => a.AllocatedDate.Year == year)
                                 .ToListAsync();
        }

        public async Task<List<AssetAllocation>> GetAllocationsByMonthAndYearAsync(string month, int year)
        {
            var monthname = DateTime.ParseExact(month, "MMMM", null).Month;
            return await _context.AssetAllocations
                                 .Where(a => a.AllocatedDate.Month == monthname && a.AllocatedDate.Year == year)
                                 .ToListAsync();
        }

        public async Task<List<AssetAllocation>> GetAllocationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AssetAllocations
                                 .Where(a => a.AllocatedDate >= startDate && a.AllocatedDate <= endDate)
                                 .ToListAsync();
        }


        public async Task AddAllocation(AssetAllocation allocation)
        {
            try
            {
                _context.AssetAllocations.Add(allocation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Adding Allocation: {ex.Message}");
            }
        }

        public async Task DeleteAllocation(int id)
        {
            try
            {
                var allocation = await GetAllocationById(id);
                if (allocation == null)
                {
                    throw new AllocationNotFoundException($"Allocation with ID {id} Not Found");
                }
                _context.AssetAllocations.Remove(allocation);
            }
            catch (AllocationNotFoundException ex)
            {
                throw new AllocationNotFoundException(ex.Message);
            }
            catch(Exception ex)
            {
                throw new Exception($"Error Deleting the Allocation{ex.Message}");
            }
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<AssetAllocation?> GetAllocationById(int id)
        {
            return await _context.AssetAllocations
                .Include(aa => aa.Asset)
                    .ThenInclude(asset => asset.Category)
                    .ThenInclude(category => category.SubCategories)
                .Include(aa => aa.User)
                .FirstOrDefaultAsync(aa => aa.AllocationId == id);
        }

        public async Task<List<AssetAllocation>> GetAllocationListById(int userId)
        {
            return await _context.AssetAllocations
                .Where(aa => aa.UserId == userId)
                .Include(aa => aa.Asset)
                    .ThenInclude(asset => asset.Category)
                    .ThenInclude(category => category.SubCategories)
                .Include(aa => aa.User)
                .Include(aa => aa.AssetRequests)
            .ToListAsync();
        }



        //public async Task<AssetAllocation> AllocateAssetAsync(AssetAllocationDto allocationDto, int adminUserId)
        //{
        //    // Check if the asset exists
        //    var asset = await _context.Assets.FindAsync(allocationDto.AssetId);
        //    if (asset == null)
        //    {
        //        throw new AssetNotFoundException("Asset not found.");
        //    }

        //    // Check if the user exists (employee to whom the asset is being allocated)
        //    var user = await _context.Users.FindAsync(allocationDto.UserId);
        //    if (user == null)
        //    {
        //        throw new UserNotFoundException("User not found.");
        //    }

        //    // Check if the current user (admin) exists
        //    var admin = await _context.Users.FindAsync(adminUserId);
        //    if (admin == null || admin.User_Type != UserType.Admin)
        //    {
        //        throw new UnauthorizedAccessException("Only an admin can allocate assets.");
        //    }

        //    // Create the AssetAllocation entity
        //    var assetAllocation = new AssetAllocation
        //    {
        //        AssetId = allocationDto.AssetId,
        //        UserId = allocationDto.UserId,
        //        AssetReqId = allocationDto.AssetReqId,
        //        AllocatedDate = DateTime.Now,
        //        Asset = asset,
        //        User = user
        //    };

        //    // Save the allocation to the database
        //    _context.AssetAllocations.Add(assetAllocation);
        //    await _context.SaveChangesAsync();

        //    // Admin details for email
        //    string fromEmail = admin.UserMail;  // Admin's email
        //    string fromName = admin.UserName;    // Admin's name

        //    // Employee details
        //    string toEmail = user.UserMail;     // Employee's email
        //    string subject = "Asset Allocation Notification";
        //    string message = $"Dear {user.UserName},<br>Your asset {asset.AssetName} has been allocated successfully on {assetAllocation.AllocatedDate}.";

        //    // Send email notification from Admin to Employee
        //    await _email.SendEmailAsync(fromEmail, fromName, toEmail, subject, message);

        //    return assetAllocation;
        //}
    }
}



    


