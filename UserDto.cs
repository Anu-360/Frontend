using static Hexa_Hub.Models.MultiValues;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Hexa_Hub.DTO
{
    public class UserDto
    {
        [Required]
        [Key]
        public int UserId { get; set; }
        [Required]
        [MaxLength(55)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string UserMail { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Dept { get; set; }

        [Required]
        public string Designation { get; set; }

        [Required]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }


        [Required]
        public string Branch { get; set; }

        
        public string User_Type { get; set; } 

        public byte[]? ProfileImage { get; set; }
    }
}
