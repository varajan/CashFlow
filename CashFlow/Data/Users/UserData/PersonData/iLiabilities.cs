namespace CashFlow.Data.Users.UserData.PersonData;

public interface ILiabilities
{
    int BankLoan { get; set; }
    int Mortgage { get; set; }
    int SchoolLoan { get; set; }
    int CarLoan { get; set; }
    int CreditCard { get; set; }
    int SmallCredits { get; set; }

    void Clear();
    void Create(Persons.DefaultLiabilities liabilities);
}
