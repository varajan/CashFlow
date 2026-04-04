namespace CashFlowBotSystemTests.Extras;

public class StepsContext
{
    public Bot Bot { get; set; }
    public User User { get; set; }
    public List<User> Users { get; set; } = [];
}
