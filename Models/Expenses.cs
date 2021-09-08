using System;
using CashFlowBot.Data;
using CashFlowBot.DataBase;

namespace CashFlowBot.Models
{
    public class Expenses : DataModel
    {
        public Expenses(long id) : base(id, DB.Tables.Expenses) { }

        public int Total => Others + Taxes + Mortgage + SchoolLoan + CarLoan + CreditCard + BankLoan + ChildrenExpenses;

        public int Taxes { get => GetInt("Taxes"); set => Set("Taxes", value); }
        public int Mortgage { get => GetInt("Mortgage"); set => Set("Mortgage", value); }
        public int SchoolLoan { get => GetInt("SchoolLoan"); set => Set("SchoolLoan", value); }
        public int CarLoan { get => GetInt("CarLoan"); set => Set("CarLoan", value); }
        public int CreditCard { get => GetInt("CreditCard"); set => Set("CreditCard", value); }
        public int BankLoan { get => GetInt("BankLoan"); set => Set("BankLoan", value); }
        public int Others { get => GetInt("Others"); set => Set("Others", value); }

        public int Children { get => GetInt("Children"); set => Set("Children", value); }
        public int PerChild { get => GetInt("PerChild"); set => Set("PerChild", value); }
        public int ChildrenExpenses => Children * PerChild;

        public string Description
        {
            get
            {
                var expenses = $"{Environment.NewLine}{Environment.NewLine}*Expenses:*{Environment.NewLine}";
                expenses += $"*Taxes:* ${Taxes}{Environment.NewLine}";
                if (Mortgage > 0) expenses += $"*Mortgage/Rent Pay:* ${Mortgage}{Environment.NewLine}";
                if (SchoolLoan > 0) expenses += $"*School Loan Pay:* ${SchoolLoan}{Environment.NewLine}";
                if (CarLoan > 0) expenses += $"*Car Payment:* ${CarLoan}{Environment.NewLine}";
                if (CarLoan > 0) expenses += $"*Credit Card Payment:* ${CreditCard}{Environment.NewLine}";
                if (BankLoan > 0) expenses += $"*Bank Loan Payment:* ${BankLoan}{Environment.NewLine}";
                expenses += $"*Other Payment:* ${Others}{Environment.NewLine}";
                if (ChildrenExpenses > 0) expenses += $"*Children:* ${Children}{Environment.NewLine}";
                if (ChildrenExpenses > 0) expenses += $"*Children Expenses:* ${ChildrenExpenses}{Environment.NewLine}";
                expenses += $"*Total Expenses:* ${Total}{Environment.NewLine}";

                return expenses;
            }
        }

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

        public void Create(Persons.DefaultExpenses expenses)
        {
            Clear();
            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Expenses}) VALUES ({Id}, {DB.DefaultValues.Expenses})");

            Taxes = expenses.Taxes;
            Mortgage = expenses.Mortgage;
            SchoolLoan = expenses.SchoolLoan;
            CarLoan = expenses.CarLoan;
            CreditCard = expenses.CreditCard;
            BankLoan = expenses.BankLoan;
            Others = expenses.Others;
            PerChild = expenses.PerChild;
            Children = 0;
        }
    }
}
