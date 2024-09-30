using Hexa_Hub.Interface;
using Microsoft.EntityFrameworkCore;
using Hexa_Hub.Exceptions;
using Hexa_Hub.DTO;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hexa_Hub.Repository
{
    public class ReturnRequestRepo : IReturnReqRepo
    {
        private readonly DataContext _context;
        private readonly IUserRepo _userRepo;
        private readonly INotificationService _notificationService;
        public ReturnRequestRepo(DataContext context, IUserRepo userRepo, INotificationService notificationService)
        {
            _context = context;
            _userRepo = userRepo;
            _notificationService = notificationService;
        }
        public async Task<ReturnRequest> AddReturnRequest(ReturnRequestDto returnRequestDto)
        {
            var req = new ReturnRequest
            {
                ReturnId = returnRequestDto.ReturnId,
                UserId = returnRequestDto.UserId,
                AssetId = returnRequestDto.AssetId,
                CategoryId = returnRequestDto.CategoryId,
                ReturnDate = returnRequestDto.ReturnDate,
                Reason = returnRequestDto.Reason,
                Condition = returnRequestDto.Condition
            };
            await _context.AddAsync(req);
            var adminUsers = await _userRepo.GetUsersByAdmin();

           
            foreach (var admin in adminUsers)
            {

                await _notificationService.ReturnRequestSent(admin.UserMail, admin.UserName, returnRequestDto.AssetId, req.ReturnId);
            }
            return req;
        }

        public async Task DeleteReturnRequest(int id)
        {
            var req = await _context.ReturnRequests.FindAsync(id);
            if (req == null)
                throw new ReturnRequestNotFoundException($"Return Request with ID {id} Not Found");
            _context.ReturnRequests.Remove(req);
        }

        public async Task<List<ReturnRequest>> GetAllReturnRequest()
        {
            return await _context.ReturnRequests
                .Include(rr => rr.Asset)
                .Include(rr => rr.User)
                .ToListAsync();
        }

        public async Task<ReturnRequest?> GetReturnRequestById(int id)
        {
            return await _context.ReturnRequests
                .Include(rr => rr.Asset)
                .Include(rr => rr.User)
                .FirstOrDefaultAsync(rr => rr.ReturnId == id);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateReturnRequest(ReturnRequest returnRequest)
        {
           
            _context.Attach(returnRequest);
            _context.Entry(returnRequest).State = EntityState.Modified;
        }

        public async Task<List<ReturnRequest>> GetReturnRequestsByUserId(int userId)
        {
            return await _context.ReturnRequests
                .Where(rr => rr.UserId == userId)
                .Include(rr => rr.Asset)
                .Include(rr => rr.User)
                .ToListAsync();
        }

        public async Task<bool> UserHasAsset(int id)
        {
            return await _context.AssetAllocations
                 .AnyAsync(aa => aa.UserId == id);
        }
    }
}
