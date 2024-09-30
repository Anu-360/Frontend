using Hexa_Hub.DTO;
using Hexa_Hub.Exceptions;
using Hexa_Hub.Interface;
using Hexa_Hub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Hexa_Hub.Models.MultiValues;

namespace Hexa_Hub.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;
        public UserRepo(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<User> RegisterUser(UserRegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                UserMail = dto.UserMail,
                Gender = dto.Gender,
                Dept = dto.Dept,
                Designation = dto.Designation,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Password = dto.Password,
                Branch = dto.Branch,
                User_Type = Models.MultiValues.UserType.Employee
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            const string defaultImageFileName = "profile-img.jpg";
            const string imagesFolder = "Images";
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), imagesFolder);
            string defaultImagePath = Path.Combine(imagePath, defaultImageFileName);
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            if (!File.Exists(defaultImagePath))
            {
                string sourcePath = GetDefaultImageSourcePath();
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException("Source default image file not found.", sourcePath);
                }
                File.Copy(sourcePath, defaultImagePath);
            }
            user.ProfileImage = Encoding.UTF8.GetBytes(defaultImageFileName);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new UserNotFoundException($"User with ID {id} Not Found");
            _context.Users.Remove(user);
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _context.Users
                .Include(u => u.AssetAllocations)
                .Include(u => u.ReturnRequests)
                .Include(u => u.AssetRequests)
                .Include(u => u.ServiceRequests)
                .Include(u => u.Audits)
                .Include(u => u.MaintenanceLogs)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRole(UserType role)
        {
            return await _context.Users.Where(u => u.User_Type == role).ToListAsync();
        }


        public async Task<User?> GetUserId(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users
                .Include(u => u.AssetAllocations)
                .Include(u => u.ReturnRequests)
                .Include(u => u.AssetRequests)
                .Include(u => u.ServiceRequests)
                .Include(u => u.Audits)
                .Include(u => u.MaintenanceLogs)
                .FirstOrDefaultAsync(u=>u.UserId==id);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public Task<User> UpdateUser(User user)
        {
            _context.Users.Update(user);
            return Task.FromResult(user);
        }

        public async Task<User?> validateUser(string email, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(vu=>vu.UserMail == email &&  vu.Password == password);
        }

        public async Task<string?> UploadProfileImage(int userId, IFormFile file)
        {
            var userProfile = await _context.Users.FindAsync(userId);
            if (userProfile == null)
            {
                return null;
            }


            const string imagesFolder = "Images";
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), imagesFolder);
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            string fileName;
            if (userProfile.ProfileImage == null && file == null)
            {
                fileName = "profile-img.jpg";
            }
            else if (file != null)
            {
                string extension = Path.GetExtension(file.FileName);
                fileName = $"{userId}{extension}";
                string fullPath = Path.Combine(imagePath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            else
            {
                return Encoding.UTF8.GetString(userProfile.ProfileImage);
            }
            userProfile.ProfileImage = Encoding.UTF8.GetBytes(fileName);
            await _context.SaveChangesAsync();
            return fileName;

}
        private string GetDefaultImageSourcePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Images", "profile-img.jpg");
        }
        public string GetImagePath(string fileName)
        {
            return Path.Combine("Images", fileName);
        }

        public async Task<List<User>> GetUsersByAdmin()
        {
            var adminUserType = MultiValues.UserType.Admin;
            return await _context.Users
                .Where(u => u.User_Type == adminUserType)
                    .ToListAsync();
        }
    }
}
