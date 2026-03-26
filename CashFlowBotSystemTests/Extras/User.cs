using System.Security.Cryptography;
using System.Text;

namespace CashFlowBotTests.Extras;

public class User(string name)
{
    public string Name { get; } = name;
    public string Profession { get; set; }

    private int Id
    {
        get
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(Name));
            var result = BitConverter.ToInt32(hash, 0);
            return Math.Abs(result);
        }
    }

    public void StopCurrentGame()
    {
        SendMessage("Cancel");
        SendMessage("Main menu");
        SendMessage("Show my data");
        SendMessage("Stop game");
        SendMessage("Yes");
    }

    public void SendMessage(string message) => Bot.SendMessage(message, Id);
    public MessageDto GetReply(int indexFromEnd = 0) => Bot.GetReply(Id, indexFromEnd);
}
