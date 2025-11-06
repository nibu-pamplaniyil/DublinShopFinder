using System.Net.Http.Json;
using Microsoft.Extensions.Options;

// Put this class in Services/GooglePlacesService.cs
public class GooglePlacesService : IGooglePlacesService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<GooglePlacesService> _logger;

    public GooglePlacesService(IHttpClientFactory factory, IOptions<GoogleOptions> opts, ILogger<GooglePlacesService> logger)
    {
        _http = factory.CreateClient();
        _apiKey = opts.Value.ApiKey;
        _logger = logger;
    }

    public async Task<IEnumerable<PlaceDto>> SearchPlacesAsync(string query, double lat, double lng, int radius = 5000)
    {
        // Use Text Search to allow flexible queries like "clothes"
        // endpoint: https://maps.googleapis.com/maps/api/place/textsearch/json
        var location = $"{lat},{lng}";
        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&location={location}&radius={radius}&key={_apiKey}";
        var resp = await _http.GetFromJsonAsync<GoogleTextSearchResponse>(url);

        if (resp == null || resp.Results == null) return Enumerable.Empty<PlaceDto>();

        // Map results and fetch details for each (to get opening hours and phone)
        var places = new List<PlaceDto>();

        foreach (var r in resp.Results.Take(10)) // limit to first 10 to avoid too many requests
        {
            var p = new PlaceDto
            {
                PlaceId = r.Place_id,
                Name = r.Name,
                Address = r.Formatted_address ?? r.Vicinity,
                Lat = r.Geometry.Location.Lat,
                Lng = r.Geometry.Location.Lng,
                PhotoReference = r.Photos?.FirstOrDefault()?.Photo_reference,
                PhotoUrl = r.Photos?.FirstOrDefault() != null ? $"/api/places/photo?photoreference={Uri.EscapeDataString(r.Photos.First().Photo_reference)}&maxwidth=400" : null,
                Types = r.Types ?? Enumerable.Empty<string>()
            };

            // Get details for opening_hours and phone
            try
            {
                var details = await GetPlaceDetailsAsync(r.Place_id);
                if (details != null)
                {
                    p.IsOpenNow = details.Result?.Opening_hours?.Open_now;
                    p.OpeningHoursSummary = details.Result?.Opening_hours?.Weekday_text != null
                        ? string.Join("; ", details.Result.Opening_hours.Weekday_text)
                        : details.Result?.Opening_hours?.Open_now == true ? "Open now" : "Closed now";

                    p.PhoneNumber = details.Result?.Formatted_phone_number;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get details for place {PlaceId}", r.Place_id);
            }

            places.Add(p);
        }

        return places;
    }

    public async Task<byte[]> GetPhotoAsync(string photoReference, int maxWidth = 400)
    {
        var url = $"https://maps.googleapis.com/maps/api/place/photo?photoreference={Uri.EscapeDataString(photoReference)}&maxwidth={maxWidth}&key={_apiKey}";
        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync();
    }

    private async Task<PlaceDetailsResponse> GetPlaceDetailsAsync(string placeId)
    {
        var fields = "opening_hours,formatted_phone_number,weekday_text";
        // fields param: list of fields you need. Use "opening_hours,formatted_phone_number" at least.
        var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={Uri.EscapeDataString(placeId)}&fields=name,opening_hours,formatted_phone_number&key={_apiKey}";
        return await _http.GetFromJsonAsync<PlaceDetailsResponse>(url);
    }

    // Response DTOs for Google JSON (trimmed)
    private class GoogleTextSearchResponse
    {
        public IEnumerable<TextResult> Results { get; set; }
        public string Status { get; set; }
    }

    private class TextResult
    {
        public string Formatted_address { get; set; }
        public string Vicinity { get; set; }
        public Geometry Geometry { get; set; }
        public string Name { get; set; }
        public string Place_id { get; set; }
        public Photo[] Photos { get; set; }
        public string[] Types { get; set; }
    }

    private class Geometry
    {
        public Location Location { get; set; }
    }

    private class Location { public double Lat { get; set; } public double Lng { get; set; } }

    private class Photo { public string Photo_reference { get; set; } }

    private class PlaceDetailsResponse
    {
        public PlaceDetailsResult Result { get; set; }
        public string Status { get; set; }
    }

    private class PlaceDetailsResult
    {
        public OpeningHours Opening_hours { get; set; }
        public string Formatted_phone_number { get; set; }
    }

    private class OpeningHours
    {
        public bool? Open_now { get; set; }
        public string[] Weekday_text { get; set; }
    }
}
