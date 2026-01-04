using CashFlow.Data.Consts;

namespace CashFlow.Data.DTOs;

public class HistoryDto
{
    public long UserId { get; set; }
    public DateTime Date { get; set; }
    public ActionType Action { get; set; }
    public long Value { get; set; }
    public string Description { get; set; }
}
