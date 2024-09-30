using static Hexa_Hub.Models.MultiValues;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Hexa_Hub.DTO
{
    public class ReturnRequestDto
    {
        [Required]
        [Key]
        public int ReturnId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime ReturnDate { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public string Condition { get; set; }

     
        public string ReturnStatus { get; set; } 
    }
}
