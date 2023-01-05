using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using TelegramUser = Telegram.Bot.Types.User;

namespace CashFlowBot.Models
{
    public class User : DataModel
    {
        public User(long id) : base(id, "Users") { }

        public History History => new(Id);
        public Person Person => new(Id);

        public bool Exists => DB.GetColumn($"SELECT ID FROM {Table} WHERE ID = {Id}").Any();

        public Stage Stage { get => (Stage) GetInt("Stage"); set => Set("Stage", (int) value); }

        public string Name { get => Get("Name"); private set => Set("Name", value); }

        public void SetName(TelegramUser user = null)
        {
            var name = $"{user?.FirstName} {user?.LastName}".Trim();
            Name = string.IsNullOrEmpty(name) ? user?.Username : name;
        }

        public DateTime FirstLogin
        {
            get => Get("FirstLogin").ToDateTime();
            private set => Set("FirstLogin", value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        }

        public DateTime LastActive
        {
            get => Get("LastActive").ToDateTime();
            set => Set("LastActive", value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        }

        public bool IsAdmin { get => GetInt("Admin") == 1; set => Set("Admin", value ? 1 : 0); }

        public Language Language { get => (Language) GetInt("Language"); set => Set("Language", (int) value); }

        public string Description
        {
            get
            {
                var file = "c:/varajan/log.txt";

                File.AppendAllText(file, $"\r\n");
                File.AppendAllText(file, $"{DateTime.Now:mm-ss-fff} - x0\r\n");
                var x1 = Person.Description;
                File.AppendAllText(file, $"{DateTime.Now:mm-ss-fff} - x1\r\n");
                var x2 = Person.Assets.Description;
                File.AppendAllText(file, $"{DateTime.Now:mm-ss-fff} - x2\r\n");
                var x3 = Person.Expenses.Description;
                File.AppendAllText(file, $"{DateTime.Now:mm-ss-fff} - x3\r\n");

                return x1 + x2 + x3;
            }
        }

        //public string Description => Person.Description +
        //                             Person.Assets.Description +
        //                             Person.Expenses.Description;

        public void GetCredit(int amount)
        {
            Person.Cash += amount;
            Person.Expenses.BankLoan += amount / 10;
            Person.Liabilities.BankLoan += amount;
            History.Add(ActionType.Credit, amount);
        }

        public int PayCredit(int amount, bool regular)
        {
            amount = amount / 1000 * 1000;
            amount = Math.Min(amount, Person.Liabilities.BankLoan);
            var percent = (decimal) 1 / 10;
            var expenses = (int)(amount * percent);

            Person.Cash -= amount;
            Person.Expenses.BankLoan -= expenses;
            Person.Liabilities.BankLoan -= amount;
            History.Add(regular ? ActionType.BankLoan : ActionType.BankruptcyBankLoan, amount);

            return amount;
        }

        public void Create()
        {
            DB.Execute($"INSERT INTO {Table} " +
                       "(ID, Stage, Admin, Name, Language, LastActive, FirstLogin) " +
                       $"VALUES ({Id}, '', '', '', '', '', '')");

            FirstLogin = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}
