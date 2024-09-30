using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hexa_Hub.Interface;
using Hexa_Hub.DTO;
using System.Text;
using Hexa_Hub.Repository;
using Hexa_Hub.Exceptions;
using static Hexa_Hub.Models.MultiValues;
using Hexa_Hub.DTO;


namespace Hexa_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAsset _asset;

        public AssetsController(DataContext context, IAsset asset)

        {
            _context = context;
            _asset = asset;
        }

        //GET: api/Assets
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
        {
            return await _asset.GetAllAssets();
        }


        [HttpGet("assetall")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssetDtoClass>>> GetAllAssets()
        {
            var assets = await _asset.GetAssetsAll();
            return Ok(assets);
        }
        //[HttpGet]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssets()
        //{
        //    var assetDtos = await _asset.GetAllAssets();
        //    return Ok(assetDtos);
        //}
        //[HttpGet("assetsall")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssetsAll()
        //{
        //    var assets = await _context.Assets
        // .Include(a => a.Category)
        // .Include(a => a.SubCategories)
        // .ToListAsync();

        //    var assetDtos = assets.Select(asset => new AssetDto
        //    {
        //        AssetId = asset.AssetId,
        //        AssetName = asset.AssetName,
        //        Location = asset.Location,
        //        Value = asset.Value,
        //        AssetStatus = asset.Asset_Status?.ToString() ?? "N/A",
        //        CategoryName = asset.Category?.CategoryName ?? "Unknown",
        //        SubCategoryName = asset.SubCategories?.SubCategoryName ?? "Unknown"
        //    }).ToList();

        //    return Ok(assetDtos);
        //}



        [HttpGet("Details")]
        [Authorize]
        public async Task<List<Asset>> GetAllDetailsOfAssets()
        {
            return await _asset.GetAllDetailsOfAssets();
        }

        // GET: api/Assets/ByAssetName/{name}
        [HttpGet("ByAssetName/{name}")]
        [Authorize]
        public async Task<ActionResult<AssetDto>> GetAssetByName(string name)
        {
            var assetDtos = await _asset.GetAssetByName(name);


            if (assetDtos == null || !assetDtos.Any())
            {
                throw new AssetNotFoundException($"No assets found containing '{name}'.");
            }
            return Ok(assetDtos);
        }

        // GET: api/Assets/PriceRange?minPrice=1000&maxPrice=5000
        [HttpGet("PriceRange")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssetsByValue([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
            {
                return BadRequest(" Invalid price range ");
            }

            var assetDtos = await _asset.GetAssetsByValue(minPrice, maxPrice);

            if (assetDtos == null || !assetDtos.Any())
            {
                return NotFound($" No assets found in the price range {minPrice} to {maxPrice} ");
            }

            return Ok(assetDtos);
        }


        // GET: api/Assets/ByAssetLocation/{location}
        [HttpGet("ByAssetLocation/{location}")]
        [Authorize]
        public async Task<ActionResult<AssetDto>> GetAssetsByLocation(string location)
        {
            var assetDtos = await _asset.GetAssetsByLocation(location);


            if (assetDtos == null || !assetDtos.Any())
            {
                throw new AssetNotFoundException($"No assets found containing '{location}'.");
            }
            return Ok(assetDtos);
        }

        // GET: api/Assets/Status?status=OpenToRequest
        [HttpGet("Status")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssetsByStatus([FromQuery] AssetStatus status)
        {
            var assetDtos = await _asset.GetAssetsByStatus(status);

            if (assetDtos == null || !assetDtos.Any())
            {
                return NotFound($"No assets found with status '{status}'.");
            }

            return Ok(assetDtos);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAssetByAsset(int id)
        {
            var asset = await _asset.GetAssetByAssetId(id);
            return Ok(asset);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAsset(int id, [FromBody] AssetUpdateDto assetDto)
        {
            if (id != assetDto.AssetId)
            {
                return BadRequest();
            }

            try
            {
                var existingAsset = await _asset.UpdateAssetDto(id, assetDto);
                await _asset.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {

            try
            {
                await _asset.DeleteAsset(id);
                await _asset.Save();
                return NoContent();
            }
            catch (SubCategoryNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Asset>> PostAsset([FromBody] AssetDto assetDto,IFormFile AssetImage)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    if (AssetImage != null)
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await AssetImage.CopyToAsync(memoryStream);
        //            assetDto.AssetImage = memoryStream.ToArray(); // Convert to byte array
        //        }
        //    }
        //    var asset = await _asset.AddAsset(assetDto);
        //    return CreatedAtAction("GetAssets", new { id = asset.AssetId }, asset);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Asset>> PostAsset([FromBody] AssetDto assetDto, IFormFile assetImage)
        //{
        //    // Scenario 3: Invalid Request
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Scenario 1: Valid Request with Image
        //    if (assetImage != null)
        //    {
        //        try
        //        {
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                await assetImage.CopyToAsync(memoryStream);
        //                assetDto.AssetImage = memoryStream.ToArray(); // Convert to byte array
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle image upload failure
        //            return BadRequest($"Image upload failed: {ex.Message}");
        //        }
        //    }

        //    Asset asset;
        //    try
        //    {
        //        // Scenario 2: Valid Request without Image
        //        asset = await _asset.AddAsset(assetDto);
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        // Scenario 5: Asset Creation Failure
        //        return StatusCode(500, $"Database error: {dbEx.InnerException?.Message}");
        //    }

        //    // Scenario 1: Valid Request with Image (successful)
        //    return CreatedAtAction("GetAssets", new { id = asset.AssetId }, asset);
        //}


        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Asset>> PostAsset([FromForm] AssetDto assetDto, [FromForm] IFormFile assetImage)
        //{
        //    // Scenario 3: Invalid Request
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Scenario 1: Valid Request with Image
        //    if (assetImage != null)
        //    {
        //        try
        //        {
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                await assetImage.CopyToAsync(memoryStream);
        //                assetDto.AssetImage = memoryStream.ToArray(); // Convert to byte array
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle image upload failure
        //            return BadRequest($"Image upload failed: {ex.Message}");
        //        }
        //    }

        //    Asset asset;
        //    try
        //    {
        //        // Scenario 2: Valid Request without Image
        //        asset = await _asset.AddAsset(assetDto);
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        // Scenario 5: Asset Creation Failure
        //        return StatusCode(500, $"Database error: {dbEx.InnerException?.Message}");
        //    }

        //    // Scenario 1: Valid Request with Image (successful)
        //    return CreatedAtAction("GetAssets", new { id = asset.AssetId }, asset);
        //}



        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Asset>> PostAsset([FromBody] AssetDto assetDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Check if the category exists, if not, create it
        //    var category = await _context.Categories.FindAsync(assetDto.CategoryId);
        //    if (category == null)
        //    {
        //        category = new Category
        //        {
        //            CategoryName = assetDto.CategoryName
        //        };
        //        _context.Categories.Add(category);
        //        await _context.SaveChangesAsync();
        //    }

        //    // Check if the subcategory exists, if not, create it
        //    var subcategory = await _context.SubCategories.FindAsync(assetDto.SubCategoryId);
        //    if (subcategory == null)
        //    {
        //        subcategory = new SubCategory
        //        {
        //            SubCategoryName = assetDto.SubCategoryName,
        //            CategoryId = category.CategoryId // Link to the newly created or existing category
        //        };
        //        _context.SubCategories.Add(subcategory);
        //        await _context.SaveChangesAsync();
        //    }

        //    var asset = await _asset.AddAsset(assetDto);
        //    return CreatedAtAction("GetAssets", new { id = asset.AssetId }, asset);
        //}


        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.AssetId == id);
        }

        //[HttpPut("{assetId}/upload")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UploadAssetImage(int assetId, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }
        //    var supportedFiles = new[] { "image/jpeg", "image/png" };
        //    if (!supportedFiles.Contains(file.ContentType))
        //    {
        //        return BadRequest("Only JPEG or PNG format are allowed");
        //    }
        //    var fileName = await _asset.UploadAssetImageAsync(assetId, file);
        //    if (fileName == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(new { FileName = fileName });
        //}

        //[HttpPost("{assetId}/upload")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UploadAssetImage(int assetId, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }
        //    var supportedFiles = new[] { "image/jpeg", "image/png" };
        //    if (!supportedFiles.Contains(file.ContentType))
        //    {
        //        return BadRequest("Only JPEG or PNG format are allowed");
        //    }
        //    var fileName = await _asset.UploadAssetImageAsync(assetId, file);
        //    if (fileName == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(new { FileName = fileName });
        //}



        [HttpPost("upload-image/{assetId}")]
        public async Task<ActionResult<string>> UploadAssetImage(int assetId, IFormFile file)
        {
            var result = await _asset.UploadAssetImageAsync(assetId, file);
            if (result == null)
            {
                return NotFound("Asset not found.");
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Asset>> AddAsset([FromForm] AssetDto assetDto)
        {
            var asset = await _asset.AddAsset(assetDto);
            return CreatedAtAction(nameof(GetAssetByAsset), new { id = asset.AssetId }, asset);
        }

        //[HttpGet("get-image/{assetId}")]
        //public IActionResult GetAssetImage(int assetId)
        //{
        //    var asset = _context.Assets.Find(assetId);
        //    if (asset == null || asset.AssetImage == null)
        //    {
        //        return NotFound("Asset not found or no image available.");
        //    }

        //    var fileName = Encoding.UTF8.GetString(asset.AssetImage);
        //    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "AssetImages", fileName);

        //    if (!System.IO.File.Exists(imagePath))
        //    {
        //        return NotFound("Image not found.");
        //    }

        //    var fileBytes = System.IO.File.ReadAllBytes(imagePath);
        //    return File(fileBytes, "image/jpeg"); // Adjust the content type based on your image type
        //}



        /// <summary>
        /// /ccccccccc
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>

        //[HttpGet("get-image/{assetId}")]
        //public async Task<IActionResult> GetAssetImage(int assetId)
        //{
        //    var asset = await _context.Assets.FindAsync(assetId);
        //    if (asset == null || asset.AssetImage == null)
        //    {
        //        return NotFound("Asset not found or no image available.");
        //    }

        //    // Use the byte array directly
        //    var fileBytes = asset.AssetImage;

        //    return File(fileBytes, "image/jpeg"); // Adjust the content type based on your image type
        //}

        [HttpGet("get-image/{assetId}")]
        public async Task<IActionResult> GetAssetImage(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || asset.AssetImage == null || asset.AssetImage.Length == 0)
            {
                return NotFound("Asset not found or no image available.");
            }


            var fileBytes = asset.AssetImage;

            return File(fileBytes, "image/jpeg");
        }
    }
}
