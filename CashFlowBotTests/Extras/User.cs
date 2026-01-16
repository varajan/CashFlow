namespace CashFlowBotTests.Extras;

public class User(string name)
{
    public string Name { get; } = name;
    private int Id { get; } = Math.Abs(name.GetHashCode());
    private Bot Bot { get; } = new Bot();

    public void SendMessage(string message) => Bot.SendMessage(message, Id);
    public MessageDto GetReply() => Bot.GetReply(Id);
}
