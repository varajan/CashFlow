namespace CashFlow.Data.DTOs;

public class ExpensesDto
{
    public int Total => Others + Taxes + Mortgage + SchoolLoan + CarLoan + CreditCard + SmallCredits + BankLoan + ChildrenExpenses + BoatLoan;

    public int Taxes { get; set; }
    public int Mortgage { get; set; }
    public int SchoolLoan { get; set; }
    public int CarLoan { get; set; }
    public int CreditCard { get; set; }
    public int SmallCredits { get; set; }

    public int BankLoan { get; set; }
    public int Others { get; set; }

    public int Children { get; set; }
    public int PerChild { get; set; }
    public int ChildrenExpenses => Children * PerChild;

    //private Asset_OLD Boat => User.Person_OBSOLETE.Assets.Boat;
    private int BoatLoan => 0;//  Boat?.CashFlow ?? 0;
}
