using Hexa_Hub.Interface;
using Microsoft.EntityFrameworkCore;
using Hexa_Hub.Exceptions;
using static Hexa_Hub.Models.MultiValues;
using Hexa_Hub.DTO;

namespace Hexa_Hub.Repository
{
    public class ServiceRequestImpl : IServiceRequest
    {
        private readonly DataContext _context;
        private readonly IUserRepo _userRepo;
        private readonly INotificationService _notificationService;

        public ServiceRequestImpl(DataContext context, IUserRepo userRepo, INotificationService notificationService)
        {
            _context = context;
            _userRepo = userRepo;
            _notificationService = notificationService;
        }

        public async Task<List<ServiceRequest>> GetAllServiceRequests()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Asset)
                .Include(sr => sr.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceReqByStatus (ServiceReqStatus serviceReqStatus)
        {
            var serviceByStatus = await (from service in _context.ServiceRequests
                                         where service.ServiceReqStatus == serviceReqStatus
                                         select new ServiceRequestDto
                                         {
                                             ServiceId = service.ServiceId,
                                             AssetId = service.AssetId,
                                             UserId = service.UserId,
                                             ServiceRequestDate = service.ServiceRequestDate,
                                             Issue_Type = service.Issue_Type,
                                             ServiceDescription = service.ServiceDescription,
                                             ServiceReqStatus = service.ServiceReqStatus.ToString() // Converting enum to string
                                         }).ToListAsync();

            return serviceByStatus;
        }


        public async Task<ServiceRequest?> GetServiceRequestById(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Asset)
                .Include(sr => sr.User)
                .FirstOrDefaultAsync(u => u.ServiceId == id);
        }

        public async Task AddServiceRequest(ServiceRequest serviceRequest)
        {
            var assetExists = await _context.Assets.AnyAsync(a => a.AssetId == serviceRequest.AssetId);
            if (!assetExists)
            {
                throw new AssetNotFoundException("Invalid Asset. Asset Not Found");
            }

            _context.ServiceRequests.Add(serviceRequest);
            var adminUsers = await _userRepo.GetUsersByAdmin();
            foreach (var admin in adminUsers)
            {
                Console.WriteLine($"Sending email to: {admin.UserMail}");

                await _notificationService.ServiceRequestSent(admin.UserMail, admin.UserName, serviceRequest.AssetId, serviceRequest.ServiceId, serviceRequest.Issue_Type);
            }
        }

        public Task<ServiceRequest> UpdateServiceRequest(ServiceRequest existingRequest)
        {
            _context.ServiceRequests.Update(existingRequest);
            return Task.FromResult(existingRequest);
        }

        public async Task DeleteServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                throw new ServiceRequestNotFoundException($"Service Request with ID {id} Not Found");
            }
            _context.ServiceRequests.Remove(serviceRequest);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByUserId(int userId)
        {
            return await _context.ServiceRequests
                .Where(sr => sr.UserId == userId)
                .Include(sr => sr.Asset)
                .Include(sr => sr.User)
                .ToListAsync();
        }


    }

}
