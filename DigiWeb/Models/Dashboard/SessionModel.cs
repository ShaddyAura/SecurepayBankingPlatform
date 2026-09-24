namespace DigiWeb.Models.Dashboard;

public class SessionModel
{
    public Guid     TokenId    { get; set; }
    public string   Token      { get; set; } = string.Empty;
    public string   DeviceInfo { get; set; } = string.Empty;
    public string   IpAddress  { get; set; } = string.Empty;
    public DateTime ExpiresAt  { get; set; }
}
