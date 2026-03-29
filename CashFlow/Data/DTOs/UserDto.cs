using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public Language Language { get; set; }
    public string StageName { get; set; }
    public string NotificationChannel { get; set; }
}
