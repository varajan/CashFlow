using CashFlow.Interfaces;
using GitCredentialManager;
using System.Text.RegularExpressions;

namespace CashFlowBot;

public class BotIdProvider(ILogger logger)
{
    private ICredentialStore _credentialStore;
    private ICredentialStore CredentialStore => _credentialStore ??= CredentialManager.Create();

    private readonly string _account = "CashFlowBot";
    private readonly string _service = "http://localhost/CashFlowBot";
    private readonly string botIdTxtFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BotID.txt");

    private string ReadBotIdFromFile()
    {
        try
        {
            var pattern = @"^\d{10}:[a-zA-Z0-9-_]{35}$";

            if (!File.Exists(botIdTxtFile))
            {
                logger.Log("BotID.txt file found.");
                return null;
            }

            var token = File.ReadAllLines(botIdTxtFile).FirstOrDefault(x => !string.IsNullOrEmpty(x));

            if (string.IsNullOrEmpty(token))
            {
                logger.Log("id is null or empty");
                return null;
            }

            if (!Regex.IsMatch(token, pattern))
            {
                logger.Log("Invalid bot ID");
                return null;
            }

            return token;
        }
        catch (Exception ex)
        {
            logger.Log(ex.Message);
            return null;
        }
    }

    public string InitializeToken()
    {
        var token = ReadBotIdFromFile();
        if (token != null)
        {
            CredentialStore.AddOrUpdate(_service, _account, token);
            File.Delete(botIdTxtFile);
            return token;
        }

        token = CredentialStore.Get(_service, _account)?.Password;

        return token ?? throw new Exception("BotId is not configured.");
    }
}
