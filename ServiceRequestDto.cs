using Hexa_Hub.Models;
using static Hexa_Hub.Models.MultiValues;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Hexa_Hub.DTO
{
    public class ServiceRequestDto
    {
        [Required]
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime ServiceRequestDate { get; set; }

        [Required]
        public IssueType Issue_Type { get; set; }

        [Required]
        public string ServiceDescription { get; set; }

        
        public string ServiceReqStatus { get; set; }
    }
}
