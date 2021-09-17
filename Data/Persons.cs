namespace CashFlowBot.Data
{
    public static class Persons
    {
        public class DefaultPerson
        {
            public string Profession { get; set; }
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

        public static DefaultPerson[] Get(long userId)
        {
            return new DefaultPerson[]
            {
                new()
                {
                    Profession = DataBase.Terms.Get(1001, userId, "Lawyer"),
                    Salary = 7_500,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 1_830,
                        Mortgage = 1_100,
                        SchoolLoan = 390,
                        CarLoan = 220,
                        CreditCard = 180,
                        Others = 1_650,
                        PerChild = 380,
                        SmallCredits = 50
                    },
                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 115_000,
                        SchoolLoan = 78_000,
                        CarLoan = 11_000,
                        CreditCard = 6_000,
                        SmallCredits = 1_000
                    }
                },

                new()
                {
                    Profession = DataBase.Terms.Get(1002, userId, "Business manager"),
                    Salary = 4_600,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 910,
                        Mortgage = 700,
                        SchoolLoan = 60,
                        CarLoan = 120,
                        CreditCard = 90,
                        SmallCredits = 50,
                        Others = 1_000,
                        PerChild = 240
                    },
                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 75_000,
                        SchoolLoan = 12_000,
                        CarLoan = 6_000,
                        CreditCard = 3_000,
                        SmallCredits = 1_000
                    }
                },

                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Track driver"),
                    Salary = 2_500,
                    Cash = 750,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 460,
                        Mortgage = 400,
                        SchoolLoan = 0,
                        CarLoan = 80,
                        CreditCard = 60,
                        SmallCredits = 50,
                        Others = 570,
                        PerChild = 140
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 38_000,
                        SchoolLoan = 0,
                        CarLoan = 4_000,
                        CreditCard = 2_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Двірник"),
                    Salary = 1_600,
                    Cash = 560,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 280,
                        Mortgage = 200,
                        SchoolLoan = 0,
                        CarLoan = 60,
                        CreditCard = 60,
                        SmallCredits = 50,
                        Others = 300,
                        PerChild = 70
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 20_000,
                        SchoolLoan = 0,
                        CarLoan = 4_000,
                        CreditCard = 2_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "медсестра"),
                    Salary = 3_100,
                    Cash = 480,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 600,
                        Mortgage = 400,
                        SchoolLoan = 30,
                        CarLoan = 100,
                        CreditCard = 90,
                        SmallCredits = 50,
                        Others = 710,
                        PerChild = 170
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 47_000,
                        SchoolLoan = 6_000,
                        CarLoan = 5_000,
                        CreditCard = 3_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "поліціянт"),
                    Salary = 3_000,
                    Cash = 520,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 580,
                        Mortgage = 400,
                        SchoolLoan = 0,
                        CarLoan = 100,
                        CreditCard = 60,
                        SmallCredits = 50,
                        Others = 690,
                        PerChild = 160
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 46_000,
                        SchoolLoan = 0_000,
                        CarLoan = 5_000,
                        CreditCard = 2_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Лікар"),
                    Salary = 13_200,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 3_420,
                        Mortgage = 1_900,
                        SchoolLoan = 750,
                        CarLoan = 380,
                        CreditCard = 270,
                        SmallCredits = 50,
                        Others = 2_880,
                        PerChild = 640
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 202_000,
                        SchoolLoan = 150_000,
                        CarLoan = 19_000,
                        CreditCard = 9_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Учитель"),
                    Salary = 3_300,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 630,
                        Mortgage = 500,
                        SchoolLoan = 60,
                        CarLoan = 100,
                        CreditCard = 90,
                        SmallCredits = 50,
                        Others = 760,
                        PerChild = 180
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 50_000,
                        SchoolLoan = 12_000,
                        CarLoan = 5_000,
                        CreditCard = 3_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Автомеханік"),
                    Salary = 2_000,
                    Cash = 670,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 360,
                        Mortgage = 300,
                        SchoolLoan = 0,
                        CarLoan = 60,
                        CreditCard = 60,
                        SmallCredits = 50,
                        Others = 450,
                        PerChild = 110
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 31_000,
                        SchoolLoan = 0_000,
                        CarLoan = 3_000,
                        CreditCard = 2_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Секретар"),
                    Salary = 2_500,
                    Cash = 710,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 460,
                        Mortgage = 400,
                        SchoolLoan = 0,
                        CarLoan = 80,
                        CreditCard = 60,
                        SmallCredits = 50,
                        Others = 570,
                        PerChild = 140
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 38_000,
                        SchoolLoan = 0_000,
                        CarLoan = 4_000,
                        CreditCard = 2_000,
                        SmallCredits = 1_000
                    }
                },


                new()
                {
                    Profession = DataBase.Terms.Get(-1, userId, "Пілот"),
                    Salary = 9_500,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 2_350,
                        Mortgage = 1_330,
                        SchoolLoan = 0,
                        CarLoan = 300,
                        CreditCard = 660,
                        SmallCredits = 50,
                        Others = 2_210,
                        PerChild = 480
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 143_000,
                        SchoolLoan = 0_000,
                        CarLoan = 15_000,
                        CreditCard = 22_000,
                        SmallCredits = 1_000
                    }
                },

                new()
                {
                    Profession = DataBase.Terms.Get(1003, userId, "Engineer"),
                    Salary = 4_900,
                    Cash = 400,

                    Expenses = new DefaultExpenses
                    {
                        Taxes = 1_050,
                        Mortgage = 700,
                        SchoolLoan = 60,
                        CarLoan = 140,
                        CreditCard = 120,
                        SmallCredits = 50,
                        Others = 1_090,
                        PerChild = 250
                    },

                    Liabilities = new DefaultLiabilities
                    {
                        Mortgage = 75_000,
                        SchoolLoan = 12_000,
                        CarLoan = 7_000,
                        CreditCard = 4_000,
                        SmallCredits = 1_000
                    }
                },
            };
        }
    }
}
