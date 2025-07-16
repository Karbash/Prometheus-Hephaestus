namespace Hephaestus.Domain.Entities;

public class CompanyOperatingHour
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty; // Ex.: "Monday", "Tuesday"
    public string OpenTime { get; set; } = string.Empty; // Ex.: "09:00"
    public string CloseTime { get; set; } = string.Empty; // Ex.: "17:00"
    public bool IsClosed { get; set; } = false;
    public bool IsOpen { get; set; } = true;
}
