using static Hexa_Hub.Models.MultiValues;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Hexa_Hub.DTO
{
    public class AssetRequestDto
    {
        [Required]
        [Key]
        public int AssetReqId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime AssetReqDate { get; set; }

        [Required]
        public string AssetReqReason { get; set; }
        public string Request_Status { get; set; }
    }
}
