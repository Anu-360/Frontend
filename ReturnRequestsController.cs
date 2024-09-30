using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hexa_Hub.DTO;
using Hexa_Hub.Interface;
using Hexa_Hub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hexa_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnRequestsController : ControllerBase
    {
        private readonly IReturnReqRepo _returnRequestRepo;
        private readonly DataContext _context;
        private readonly IAsset _asset;
        private readonly IAssetAllocation _assetAlloc;
        private readonly IAssetRequest _assetRequest;
        private readonly INotificationService _notificationService;

        public ReturnRequestsController(DataContext context, IReturnReqRepo returnRequestRepo,IAsset asset, IAssetAllocation assetAllocation, IAssetRequest assetRequest, INotificationService notificationService)
        {
            _context = context;
            _returnRequestRepo = returnRequestRepo;
            _asset = asset;
            _assetAlloc = assetAllocation;
            _assetRequest = assetRequest;
            _notificationService = notificationService;
        }

        // GET: api/ReturnRequests
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReturnRequest>>> GetReturnRequests()
        {
            //return await _context.ReturnRequests.ToListAsync();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Admin")
            {
                return await _returnRequestRepo.GetAllReturnRequest();
            }
            else
            {
                var req = await _returnRequestRepo.GetReturnRequestsByUserId(userId);
                if (req == null||req.Count==0)
                {
                    return NotFound($"No details found");
                }
                return Ok(req);
            }
        }

        // GET: api/ReturnRequests/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReturnRequest>> GetReturnRequest(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Admin")
            {
                var req = await _returnRequestRepo.GetReturnRequestById(id);
                if(req == null)
                {
                    return NotFound($"Details For the User id {id} is not found");
                }
                return Ok(req);
            }
            return Forbid();
        }


        // PUT: api/ReturnRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutReturnRequest(int id, ReturnRequestDto returnRequestDto)
        {
            if (id != returnRequestDto.ReturnId)
            {
                return BadRequest("Return Id Mismatch");
            }

            var existingRequest = await _returnRequestRepo.GetReturnRequestById(id);
            if (existingRequest == null)
            {
                return NotFound($"Details For the Request id {id} is not found");
            }

            existingRequest.UserId = returnRequestDto.UserId;
            existingRequest.AssetId = returnRequestDto.AssetId;
            existingRequest.CategoryId = returnRequestDto.CategoryId;
            existingRequest.ReturnDate = returnRequestDto.ReturnDate;
            existingRequest.Reason = returnRequestDto.Reason;
            existingRequest.Condition = returnRequestDto.Condition;

            if (Enum.TryParse(returnRequestDto.ReturnStatus, out Models.MultiValues.ReturnReqStatus parsedStatus))
            {
                existingRequest.ReturnStatus = parsedStatus;
            }
            else
            {
                return BadRequest("Invalid Return Status provided.");
            }

            // If status is Approved or Returned
            if (parsedStatus == Models.MultiValues.ReturnReqStatus.Approved || parsedStatus == Models.MultiValues.ReturnReqStatus.Returned || parsedStatus == Models.MultiValues.ReturnReqStatus.Rejected)
            {
                existingRequest.ReturnDate = DateTime.Now;

                var asset = await _context.Assets.FindAsync(existingRequest.AssetId);
                if (asset != null)
                {
                    if (parsedStatus == Models.MultiValues.ReturnReqStatus.Returned)
                    {
                        asset.Asset_Status = Models.MultiValues.AssetStatus.OpenToRequest;
                        _context.Entry(asset).State = EntityState.Modified;

                        var allocation = await _context.AssetAllocations
                            .Where(a => a.AssetId == existingRequest.AssetId && a.UserId == existingRequest.UserId)
                            .FirstOrDefaultAsync();

                        if (allocation != null)
                        {
                            try
                            {
                                await _assetAlloc.DeleteAllocation(allocation.AllocationId);
                                await _assetAlloc.Save();
                            }
                            catch (Exception ex)
                            {
                                return BadRequest($"Failed to delete allocation with ID {allocation.AllocationId}: {ex.Message}");
                            }
                        }

                        var assetRequest = await _context.AssetRequests
                            .Where(a => a.AssetId == existingRequest.AssetId && a.UserId == existingRequest.UserId && a.Request_Status == Models.MultiValues.RequestStatus.Allocated)
                            .FirstOrDefaultAsync();

                        if (assetRequest != null)
                        {
                            try
                            {
                                _assetRequest.DeleteAssetRequest(assetRequest.AssetReqId);
                                await _asset.Save();
                            }
                            catch (Exception ex)
                            {
                                return BadRequest($"Failed to delete AssetRequest with ID {assetRequest.AssetReqId}: {ex.Message}");
                            }
                        }
                    }
                }
                var user = await _context.Users.FindAsync(existingRequest.UserId);
                if (user != null)
                {
                    if (parsedStatus == Models.MultiValues.ReturnReqStatus.Approved)
                    {
                        await _notificationService.ReturnRequestApproved(user.UserMail, user.UserName, existingRequest.AssetId, id);
                    }
                    else if (parsedStatus == Models.MultiValues.ReturnReqStatus.Returned)
                    {
                        await _notificationService.ReturnRequestCompleted(user.UserMail, user.UserName, existingRequest.AssetId);
                    }
                    else if (parsedStatus == Models.MultiValues.ReturnReqStatus.Rejected)
                    {
                        await _notificationService.ReturnRequestRejected(user.UserMail, user.UserName, existingRequest.AssetId, id);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReturnRequestExists(id))
                {
                    return NotFound($"Details For the Request id {id} is not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ReturnRequests
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<ReturnRequest>> PostReturnRequest(ReturnRequestDto returnRequestDto)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var userHasAsset = await _returnRequestRepo.UserHasAsset(loggedInUserId);
            if (!userHasAsset)
            {
                return BadRequest("User does not have an asset to return.");
            }

            returnRequestDto.UserId = loggedInUserId;

            var createdRequest = await _returnRequestRepo.AddReturnRequest(returnRequestDto);
            await _returnRequestRepo.Save();


            return CreatedAtAction("GetReturnRequest", new { id = createdRequest.ReturnId }, createdRequest);
        }

        // DELETE: api/ReturnRequests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> DeleteReturnRequest(int id)
        {
            try
            {
                var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var returnRequest = await _returnRequestRepo.GetReturnRequestById(id);
                if (returnRequest == null)
                {
                    return NotFound($"Details For the Request id {id} is not found");
                }
                if (returnRequest.UserId != loggedInUserId)
                {
                    return Forbid("You are not allowed to delete other records");
                }
                await _returnRequestRepo.DeleteReturnRequest(id);
                await _returnRequestRepo.Save();

                return Ok($"Deletion Occured for { id }");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete return request: {ex.Message}");
            }
        }

        private bool ReturnRequestExists(int id)
        {
            return _context.ReturnRequests.Any(e => e.ReturnId == id);
        }
    }
}
