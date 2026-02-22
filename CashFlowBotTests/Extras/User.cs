namespace CashFlowBotTests.Extras;

public class User(string name)
{
    public string Name { get; } = name;
    public string Profession { get; set; }
    private int Id { get; } = Math.Abs(name.GetHashCode());

    public void SendMessage(string message) => Bot.SendMessage(message, Id);
    public MessageDto GetReply(int indexFromEnd = 0) => Bot.GetReply(Id, indexFromEnd);
}
