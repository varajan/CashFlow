using CashFlowBot.Data.Consts;
using CashFlowBot.Data.DataBase;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashFlowBot.Data;

public static class Persons
{
    private static ILogger logger = new FileLogger();
    private static IDataBase dataBase = new SQLiteDataBase(logger);
    private static ITermsService Terms => new TermsService(dataBase);

    public class DefaultPerson
    {
        public Dictionary<Language, string> Profession { get; set; }
        public int Salary { get; set; }
        public int Cash { get; set; }

        public DefaultExpenses Expenses { get; set; }
        public DefaultLiabilities Liabilities { get; set; }
    }

    public class DefaultExpenses
    {
        public int Taxes { get; set; }
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int BankLoan { get; set; }
        public int Others { get; set; }
        public int PerChild { get; set; }
        public int SmallCredits { get; set; }
    }

    public class DefaultLiabilities
    {
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int SmallCredits { get; set; }
        public int BankLoan { get; set; }
    }

    public static DefaultPerson Get(string profession) => GetAll().First(x => x.Profession.ContainsValue(profession));

    public static List<DefaultPerson> GetAll()
    {
        var result = new List<DefaultPerson>();
        var data = dataBase.GetRows("SELECT ID, Salary, Cash, ExpensesTaxes, ExpensesMortgage, ExpensesSchoolLoan, ExpensesCarLoan, ExpensesCreditCard, ExpensesOthers, ExpensesPerChild, ExpensesSmallCredits, LiabilitiesMortgage, LiabilitiesSchoolLoan, LiabilitiesCarLoan, LiabilitiesCreditCard, LiabilitiesSmallCredits FROM DefaultPersonData");
        
        foreach (var profesion in data)
        {
            var professionId = profesion[0].ToInt();
            var profession = Enum.GetValues<Language>().ToDictionary(l => l, l => Terms.Get(professionId, l));
            var person = new DefaultPerson
            {
                Profession = profession,
                Salary = profesion[1].ToInt(),
                Cash = profesion[2].ToInt(),

                Expenses = new DefaultExpenses
                {
                    Taxes = profesion[3].ToInt(),
                    Mortgage = profesion[4].ToInt(),
                    SchoolLoan = profesion[5].ToInt(),
                    CarLoan = profesion[6].ToInt(),
                    CreditCard = profesion[7].ToInt(),
                    Others = profesion[8].ToInt(),
                    PerChild = profesion[9].ToInt(),
                    SmallCredits = profesion[10].ToInt()
                },
                Liabilities = new DefaultLiabilities
                {
                    Mortgage = profesion[11].ToInt(),
                    SchoolLoan = profesion[12].ToInt(),
                    CarLoan = profesion[13].ToInt(),
                    CreditCard = profesion[14].ToInt(),
                    SmallCredits = profesion[15].ToInt()
                }
            };

            result.Add(person);
        }

        return result;
    }
}