namespace CashFlowBot.Data.Users.UserData.PersonData;

public interface IExpenses
{
    int Total { get; }
    int Taxes { get; set; }
    int Mortgage { get; set; }
    int SchoolLoan { get; set; }
    int CarLoan { get; set; }
    int CreditCard { get; set; }
    int SmallCredits { get; set; }

    int BankLoan { get; set; }
    int Others { get; set; }

    int Children { get; set; }
    int PerChild { get; set; }
    int ChildrenExpenses { get; }

    //private Asset Boat => User.Person.Assets.Boat;
    string Description { get; }

    void Clear();
    void Create(Persons.DefaultExpenses expenses);
}
