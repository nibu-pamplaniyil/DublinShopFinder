public class PlaceDto
{
    public string PlaceId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string PhotoReference { get; set; }
    public string PhotoUrl { get; set; } // will point to backend photo proxy
    public string OpeningHoursSummary { get; set; } // e.g., "Open until 18:00" or "Closed now"
    public bool? IsOpenNow { get; set; } // nullable
    public string PhoneNumber { get; set; }
    public IEnumerable<string> Types { get; set; }
}
