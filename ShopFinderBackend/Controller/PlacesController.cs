using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly IGooglePlacesService _places;
    private readonly ILogger<PlacesController> _logger;

    public PlacesController(IGooglePlacesService places, ILogger<PlacesController> logger)
    {
        _places = places;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] double lat, [FromQuery] double lng, [FromQuery] int radius = 5000)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var results = await _places.SearchPlacesAsync(query, lat, lng, radius);
        return Ok(results);
    }

    [HttpGet("photo")]
    public async Task<IActionResult> Photo([FromQuery] string photoreference, [FromQuery] int maxwidth = 400)
    {
        if (string.IsNullOrWhiteSpace(photoreference)) return BadRequest("photoreference is required.");
        try
        {
            var bytes = await _places.GetPhotoAsync(photoreference, maxwidth);
            return File(bytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Photo fetch failed for {Ref}", photoreference);
            return NotFound();
        }
    }

} 