using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static Hexa_Hub.Models.MultiValues;

namespace Hexa_Hub.DTO
{
    public class AssetDto
    {
        //[Required]
        //[Key]
        //public int AssetId { get; set; }

        [Required]
        [MaxLength(55)]
        public string AssetName { get; set; }

        public string? AssetDescription { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public IFormFile? AssetImage { get; set; }

        [Required]
        public string SerialNumber { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime ManufacturingDate { get; set; }

        [Required]
        [MaxLength(55)]
        public string Location { get; set; }

        [Required]
        public decimal Value { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? Expiry_Date { get; set; }

        public string? Asset_Status { get; set; }
    }
}

