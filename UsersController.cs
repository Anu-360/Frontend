using Hexa_Hub.DTO;
using Hexa_Hub.Exceptions;
using Hexa_Hub.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using static Hexa_Hub.Models.MultiValues;
using Microsoft.AspNetCore.JsonPatch;

namespace Hexa_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly DataContext _context;

        public UsersController(DataContext context, IUserRepo userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _userRepo.GetAllUser();
        }

        [HttpGet("role")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByRole([FromQuery] UserType role)
        {
            var users = await _userRepo.GetUsersByRole(role);
            if (users == null || !users.Any())
            {
                return NotFound("No users found with the specified role.");
            }
            return Ok(users);
        }



        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(currentUserId, out int userId))
            {
                var user = await _userRepo.GetUserById(userId);
                if (user == null)
                {
                    return NotFound("User not Found");
                }
                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserMail = user.UserMail,
                    Gender = user.Gender,
                    Dept = user.Dept,
                    Designation = user.Designation,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Branch = user.Branch,
                    User_Type = user.User_Type?.ToString(),
                    ProfileImage = user.ProfileImage
                };

                return Ok(userDto);
            }

            return BadRequest("Invalid user ID.");
        }




        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userRepo.GetUserById(id);  

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Map User entity to UserDto
            var userDto = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserMail = user.UserMail,   
                Gender = user.Gender,
                Dept = user.Dept,
                Designation = user.Designation,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Branch = user.Branch,
               
            };

            return Ok(userDto);  // Return the DTO
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserDetails(int id, UserDto userDto)
        {
            if (id != userDto.UserId)
            {
                return BadRequest("Check Id");
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You do not have permission to update this user.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not Found");
            }

            user.UserName = userDto.UserName;
            user.UserMail = userDto.UserMail;
            user.Gender = userDto.Gender;
            user.Dept = userDto.Dept;
            user.Designation = userDto.Designation;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Address = userDto.Address;
            user.Branch = userDto.Branch;
            user.ProfileImage = userDto.ProfileImage;

            if (User.IsInRole("Admin"))
            {
                // Try to parse the string value from userDto to the UserType enum
                if (Enum.TryParse<UserType>(userDto.User_Type, out var parsedUserType))
                {
                    user.User_Type = parsedUserType;
                }
                else
                {
                    // Handle the case where the string cannot be parsed
                    return BadRequest($"Invalid User_Type value: {userDto.User_Type}");
                }
            }

            else
            {
                user.User_Type = user.User_Type; // No change to role
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound("User not Found");
                }
                else
                {
                    throw;
                }
            }

            return Ok("Updation Successfull");
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserDetails(int id, [FromBody] JsonPatchDocument<UserDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null.");
            }

            // Find the user in the database
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map User entity to UserDto
            var userDto = new UserDto
            {
                UserId = user.UserId,
              
                UserName = user.UserName,
                Gender = user.Gender,
                Dept = user.Dept,
                Address = user.Address,
                Designation = user.Designation,
                Branch = user.Branch
            };

            // Apply patch
            patchDoc.ApplyTo(userDto, ModelState); // Use ModelState to capture errors

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map the updated values from UserDto back to User entity
         
            user.UserName = userDto.UserName;
            user.Gender = userDto.Gender;
            user.Dept = userDto.Dept;
            user.Address = userDto.Address;
            user.Designation = userDto.Designation;
            user.Branch = userDto.Branch;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }





        [HttpPatch("{id}/password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(int id, PasswordDto passwordChangeDto)
        {
            if (id != passwordChangeDto.UserId)
            {
                return BadRequest("Check Your Id");
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.ToString() != currentUserId)
            {
                return Forbid("Check your Id ");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not Found");
            }

            if (user.Password != passwordChangeDto.CurrentPassword)
            {
                return Unauthorized("Current password is incorrect.");
            }

            user.Password = passwordChangeDto.NewPassword;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound("Id not Found");
                }
                else
                {
                    throw;
                }
            }

            return Ok("Password Changed Successfully");
        }

       

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody]UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepo.RegisterUser(dto);

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
            
        }

        // DELETE: api/Users/5

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepo.GetUserById(id);
            if (user == null)
            {
                return NotFound("Id Not Found");
            }

            try
            {
                await _userRepo.DeleteUser(id);
                await _userRepo.Save();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return Ok($"{id} Has been deleted");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }


        [HttpPut("{userId}/upload")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(int userId, IFormFile file)
        {
            var loggedUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (loggedUser != userId)
            {
                return Unauthorized("You are not authorized to update this image.");
            }


             if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var supportedFiles = new[] { "image/jpeg", "image/png" };
            if (!supportedFiles.Contains(file.ContentType))
            {
                return BadRequest("Only JPEG or PNG formats are allowed.");
            }

            var fileName = await _userRepo.UploadProfileImage(userId, file);
            if (fileName == null)
            {
                return NotFound("User profile not found.");
            }

            return Ok(new { FileName = fileName });
        }


        [Authorize]
        [HttpGet("{userId}/profileImage")]
        public async Task<IActionResult> GetProfileImage(int userId)
        {
            var userProfile = await _userRepo.GetUserById(userId);
            if (userProfile == null || userProfile.ProfileImage == null)
            {
                var defaultImagePath = _userRepo.GetImagePath("profile-img.jpg");
                return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), defaultImagePath), "image/jpeg");
            }
            string fileName = Encoding.UTF8.GetString(userProfile.ProfileImage);
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), _userRepo.GetImagePath(fileName));

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image file not found.");
            }

            string contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return PhysicalFile(imagePath, contentType);



}
    }
}
