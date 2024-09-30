using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hexa_Hub.DTO;
using Hexa_Hub.Exceptions;
using Hexa_Hub.Interface;
using Hexa_Hub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Hexa_Hub.Models.MultiValues;

namespace Hexa_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IServiceRequest _serviceRequest;
        private readonly IMaintenanceLogRepo _maintenanceLog;
        private readonly INotificationService _notificationService;

        public ServiceRequestsController(DataContext context, IServiceRequest serviceRequest, IMaintenanceLogRepo maintenanceLog, INotificationService notificationService)
        {
            _context = context;
            _serviceRequest = serviceRequest;
            _maintenanceLog = maintenanceLog;
            _notificationService = notificationService;
        }

        //// GET: api/ServiceRequests

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Admin")
            {
                return Ok(await _serviceRequest.GetAllServiceRequests());
            }
            else
            {
                var userRequests = await _serviceRequest.GetServiceRequestsByUserId(userId);
                if (userRequests == null || !userRequests.Any())
                {
                    return NotFound($"No service requests found for the logged-in user {userId}.");
                }
                return Ok(userRequests);
            }
        }

        // PUT: api/ServiceRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutServiceRequest(int id, ServiceRequestDto serviceRequestDto)
        {
            if (id != serviceRequestDto.ServiceId)
            {
                return BadRequest($"Given Id's {id} and {serviceRequestDto.ServiceId} don't match");
            }

            var existingRequest = await _serviceRequest.GetServiceRequestById(id);
            if (existingRequest == null)
            {
                return NotFound($"Service request with ID {id} not found.");
            }

            existingRequest.AssetId = serviceRequestDto.AssetId;
            existingRequest.UserId = serviceRequestDto.UserId;
            existingRequest.ServiceRequestDate = serviceRequestDto.ServiceRequestDate;
            existingRequest.Issue_Type = serviceRequestDto.Issue_Type;
            existingRequest.ServiceDescription = serviceRequestDto.ServiceDescription;

            if (Enum.TryParse(serviceRequestDto.ServiceReqStatus, out ServiceReqStatus parsedStatus))
            {
                existingRequest.ServiceReqStatus = parsedStatus;

                if (parsedStatus == ServiceReqStatus.Approved)
                {
                    var asset = await _context.Assets.FindAsync(serviceRequestDto.AssetId);
                    if (asset != null)
                    {
                        asset.Asset_Status = AssetStatus.UnderMaintenance;
                        _context.Entry(asset).State = EntityState.Modified;
                    }
                    var user = await _context.Users.FindAsync(serviceRequestDto.UserId);
                    if (user != null)
                    {
                        await _notificationService.ServiceRequestApproved(user.UserMail, user.UserName, serviceRequestDto.AssetId, id, serviceRequestDto.Issue_Type);
                    }
                }
                else if (parsedStatus == ServiceReqStatus.Completed)
                {
                    var asset = await _context.Assets.FindAsync(serviceRequestDto.AssetId);
                    if (asset != null)
                    {
                        asset.Asset_Status = AssetStatus.Allocated;
                        _context.Entry(asset).State = EntityState.Modified;
                    }
                    var user = await _context.Users.FindAsync(serviceRequestDto.UserId);
                    if (user != null)
                    {
                        await _notificationService.ServiceRequestCompleted(user.UserMail, user.UserName, serviceRequestDto.AssetId, id, serviceRequestDto.Issue_Type);
                    }
                }
            }
            else
            {
                return BadRequest("Invalid ServiceReqStatus value");
            }

            try
            {
                _serviceRequest.UpdateServiceRequest(existingRequest);
                await _serviceRequest.Save();
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequestExists(id))
                {
                    return NotFound($"Details for the request ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }

            return Ok("Data modified successfully");
        }


        // POST: api/ServiceRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<ServiceRequest>> PostServiceRequest(ServiceRequestDto serviceRequestDto)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            serviceRequestDto.UserId = loggedInUserId;
            var serviceRequest = new ServiceRequest
            {
                AssetId = serviceRequestDto.AssetId,
                UserId = loggedInUserId,
                ServiceRequestDate = serviceRequestDto.ServiceRequestDate,
                Issue_Type = serviceRequestDto.Issue_Type,
                ServiceDescription = serviceRequestDto.ServiceDescription
            };
            await _serviceRequest.AddServiceRequest(serviceRequest);
            await _serviceRequest.Save();

            var maintenanceLog = new MaintenanceLog
            {
                AssetId = serviceRequest.AssetId,
                UserId = loggedInUserId,
                Maintenance_date = DateTime.Now,
                Maintenance_Description = serviceRequest.ServiceDescription
            };

            _maintenanceLog.AddMaintenanceLog(maintenanceLog);
            await _maintenanceLog.Save();

            return CreatedAtAction("GetServiceRequests", new { id = serviceRequest.ServiceId }, serviceRequest);
        }


        // DELETE: api/ServiceRequests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            try
            {
                var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var serviceRequest = await _serviceRequest.GetServiceRequestById(id);
                if (serviceRequest == null)
                {
                    return NotFound("Id's Mismatch"); 
                }
                if (serviceRequest.UserId != loggedInUserId )
                {
                    return Forbid("You are not able to Delete"); 
                }
                if(serviceRequest.ServiceReqStatus == ServiceReqStatus.Approved || serviceRequest.ServiceReqStatus == ServiceReqStatus.Completed)
                {
                    return BadRequest($"The Service Id {id} for USer {loggedInUserId} is already been {serviceRequest.ServiceReqStatus}");
                }

                await _serviceRequest.DeleteServiceRequest(id);
                await _serviceRequest.Save();

                return Ok("Deletion Of Data Occured");
            }
            catch (AssetNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Status/{status}")]
        public async Task<IActionResult> GetServiceRequestsByStatus(ServiceReqStatus status)
        {
            try
            {
                var serviceRequests = await _serviceRequest.GetServiceReqByStatus(status);
                if (serviceRequests == null)
                {
                    return NotFound("No service requests found with the given status.");
                }

                return Ok(serviceRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.ServiceId == id);
        }
    }
}
