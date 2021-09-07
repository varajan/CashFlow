namespace CashFlowBot.Data
{
    public static class Persons
    {
        public class DefaultPerson
        {
            public string Profession { get; set; }
            public int Salary { get; set; }
            public int Assets { get; set; }

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
        }


        public class DefaultLiabilities
        {
            public int Mortgage { get; set; }
            public int SchoolLoan { get; set; }
            public int CarLoan { get; set; }
            public int CreditCard { get; set; }
            public int BankLoan { get; set; }
        }

        public static DefaultPerson[] Items =
        {
            new ()
            {
                Profession = "Lawyer",
                Salary = 7_500,
                Assets = 2_000,

                Expenses = new DefaultExpenses
                {
                    Taxes = 1_800,
                    Mortgage = 1_100,
                    SchoolLoan = 300,
                    CarLoan = 200,
                    CreditCard = 200,
                    Others = 1_500,
                    PerChild = 400
                },

                Liabilities = new DefaultLiabilities
                {
                    Mortgage = 115_000,
                    SchoolLoan = 78_000,
                    CarLoan = 11_000,
                    CreditCard = 7_000
                }
            },

            new ()
            {
                Profession = "Business manager",
                Salary = 4_600,
                Assets = 400,

                Expenses = new DefaultExpenses
                {
                    Taxes = 900,
                    Mortgage = 700,
                    SchoolLoan = 60,
                    CarLoan = 120,
                    CreditCard = 90,
                    Others = 1_000,
                    PerChild = 240
                },

                Liabilities = new DefaultLiabilities
                {
                    Mortgage = 75_000,
                    SchoolLoan = 12_000,
                    CarLoan = 6_000,
                    CreditCard = 3_000
                }
            },

            new ()
            {
                Profession = "Engineer",
                Salary = 4_900,
                Assets = 400,

                Expenses = new DefaultExpenses
                {
                    Taxes = 1_000,
                    Mortgage = 700,
                    SchoolLoan = 100,
                    CarLoan = 200,
                    CreditCard = 200,
                    Others = 1_000,
                    PerChild = 200
                },

                Liabilities = new DefaultLiabilities
                {
                    Mortgage = 75_000,
                    SchoolLoan = 12_000,
                    CarLoan = 6_000,
                    CreditCard = 3_000
                }
            },
        };
    }
}
