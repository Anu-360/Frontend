using System.ComponentModel.DataAnnotations;

namespace Hexa_Hub.DTO
{
    public class AssetAllocationDto
    {
        [Required]
        [Key]
        public int AllocationId { get; set; }

        [Required]
        public string AssetName { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AssetReqId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime AllocatedDate { get; set; } = DateTime.Now;

    }
}
