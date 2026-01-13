namespace CashFlowBotTests;

public class MessageDto
{
    public DateTime UtcNow { get; set; }
    public string Message { get; set; }
    public string[] Buttons { get; set; }
}
