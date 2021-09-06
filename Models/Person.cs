namespace CashFlowBot.Models
{
    public class Person
    {
        public string Profession { get; init; }

        public int Salary { get; init; }
        public int Assets { get; set; }

        public Expenses Expenses { get; set; } = new();
        public Liabilities Liabilities { get; set; } = new();
    }

    public class Expenses
    {
        public int Total => Others + Taxes + Mortgage + SchoolLoan + CarLoan + CreditCard + BankLoan + ChildrenExpenses;

        public int Taxes { get; set; }
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int BankLoan { get; set; }
        public int Others { get; set; }

        public int Children { get; set; } = 0;
        public int PerChild { get; set; }
        public int ChildrenExpenses => Children * PerChild;
    }

    public class Liabilities
    {
        public int Mortgage { get; set; }
        public int SchoolLoan { get; set; }
        public int CarLoan { get; set; }
        public int CreditCard { get; set; }
        public int BankLoan { get; set; }
    }
}
