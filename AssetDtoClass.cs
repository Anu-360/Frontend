using static Hexa_Hub.Models.MultiValues;
using System.ComponentModel.DataAnnotations;

namespace Hexa_Hub.DTO
{
    public class AssetDtoClass
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetDescription { get; set; }
        public string Location { get; set; }
        public decimal Value { get; set; }
        public string CategoryName { get; set; }

        public int CategoryId {  get; set; }

        public int SubCategoryId { get; set; }  
        public string SubCategoryName { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime ManufacturingDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? Expiry_Date { get; set; }

        // Add this computed property to return the AssetStatus as a string
        public string AssetStatusName => AssetStatus.ToString();  // This converts the enum to a string

        // The existing property (you can keep it if needed)
        public AssetStatus? AssetStatus { get; set; }
    }
}
