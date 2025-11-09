using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

// Put this class in Services/GooglePlacesService.cs
public class GooglePlacesService : IGooglePlacesService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<GooglePlacesService> _logger;
    private readonly IDistributedCache _cache;

    public GooglePlacesService(IHttpClientFactory factory, IOptions<GoogleOptions> opts, ILogger<GooglePlacesService> logger, IDistributedCache cache)
    {
        _http = factory.CreateClient();
        _apiKey = opts.Value.ApiKey;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<PlaceDto>> SearchPlacesAsync(string query, double lat, double lng, int radius = 5000)
    {
        var cacheKey = $"places_{query}_{lat}_{lng}_{radius}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _logger.LogInformation("Returning cached results for query: {Query}", query);
            return JsonSerializer.Deserialize<IEnumerable<PlaceDto>>(cachedData);
        }
        var location = $"{lat},{lng}";
        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&location={location}&radius={radius}&key={_apiKey}";
        var resp = await _http.GetFromJsonAsync<GoogleTextSearchResponse>(url);

        if (resp == null || resp.Results == null) return Enumerable.Empty<PlaceDto>();

        var places = new List<PlaceDto>();

        foreach (var r in resp.Results.Take(10))
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

        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        var serialized = JsonSerializer.Serialize(places);
        await _cache.SetStringAsync(cacheKey, serialized, cacheEntryOptions);

        _logger.LogInformation("Cached new results for query: {Query}", query);

        return places;
    }

    public async Task<byte[]> GetPhotoAsync(string photoReference, int maxWidth = 400)
    {
        var cacheKey = $"photo_{photoReference}_{maxWidth}";
        var cachedPhoto = await _cache.GetAsync(cacheKey);
        if (cachedPhoto != null)
        {
            _logger.LogInformation("Returning cached photo: {PhotoReference}", photoReference);
            return cachedPhoto;
        }

        var url = $"https://maps.googleapis.com/maps/api/place/photo?photoreference={Uri.EscapeDataString(photoReference)}&maxwidth={maxWidth}&key={_apiKey}";
        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        var photoBytes = await resp.Content.ReadAsByteArrayAsync();
        await _cache.SetAsync(cacheKey, photoBytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });

        return photoBytes;
    }

    private async Task<PlaceDetailsResponse> GetPlaceDetailsAsync(string placeId)
    {
        var cacheKey = $"details_{placeId}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached details for {PlaceId}", placeId);
            return JsonSerializer.Deserialize<PlaceDetailsResponse>(cached);
        }

        var fields = "opening_hours,formatted_phone_number,weekday_text";
        var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={Uri.EscapeDataString(placeId)}&fields=name,opening_hours,formatted_phone_number&key={_apiKey}";
        var result = await _http.GetFromJsonAsync<PlaceDetailsResponse>(url);
        if (result != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }

        return result;
    }

    
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
