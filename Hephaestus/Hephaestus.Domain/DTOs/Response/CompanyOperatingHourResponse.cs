namespace Hephaestus.Domain.DTOs.Response
{
    public class CompanyOperatingHourResponse
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
