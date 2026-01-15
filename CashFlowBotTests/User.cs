namespace CashFlowBotTests;

public class User(string name, Bot bot)
{
    public string Name { get; } = name;
    private int Id { get; } = Math.Abs(name.GetHashCode());
    private Bot Bot { get; } = bot;

    public void SendMessage(string message) => Bot.SendMessage(message, Id);
    public MessageDto GetReply() => Bot.GetReply(Id);
    public string GetAllMessages() => Bot.GetAllMessages(Id);
}
