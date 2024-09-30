using System.ComponentModel.DataAnnotations;

namespace Hexa_Hub.DTO
{
    public class AllocationDto
    {
        [Required]
        public int UserId {  get; set; }
        [Required]
        public int AssetId {  get; set; }   
        [Required]
        public string AssetName {  get; set; }

        [Required]
        public int CategoryId {  get; set; }    

        [Required]
        public string CategoryName {  get; set; }

        [Required]
        public decimal Value {  get; set; }
        [Required]
        public string Asset_Status {  get; set; }
        [Required]
        public string Model { get; set; }  

      
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime AllocatedDate { get; set; } = DateTime.Now;


    }
}
