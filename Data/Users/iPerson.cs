namespace CashFlowBot.Data.Users;

public interface IPerson
{
    bool Exists { get; }
    int CashFlow { get; }
    bool CreditsReduced { get; set; }
    bool ReadyForBigCircle { get; set; }
    int Salary { get; set; }
    int InitialCashFlow { get; set; }
    int CurrentCashFlow { get; }
    int TargetCashFlow { get; }
    Circle Circle { get; set; }
    bool BigCircle { get; set; }
    string Description { get; }
    bool SmallRealEstate { get; set; }

    Assets Assets { get; }
    Expenses Expenses { get; }
    Liabilities Liabilities { get; }
    string Profession { get; set; }
    int Cash { get; set; }
    bool Bankruptcy { get; set; }
    void ReduceCreditsRollback();
    void ReduceCredits();
    void Clear();
    void Create(string profession);
}
