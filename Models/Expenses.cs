using CashFlowBot.Data;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;
using System;
using Terms = CashFlowBot.DataBase.Terms;

namespace CashFlowBot.Models;

public class Expenses : DataModel
{
    public Expenses(long id) : base(id, "Expenses") { }

    public int Total => Others + Taxes + Mortgage + SchoolLoan + CarLoan + CreditCard + SmallCredits + BankLoan + ChildrenExpenses + BoatLoan;

    public int Taxes { get => GetInt("Taxes"); set => Set("Taxes", value); }
    public int Mortgage { get => GetInt("Mortgage"); set => Set("Mortgage", value); }
    public int SchoolLoan { get => GetInt("SchoolLoan"); set => Set("SchoolLoan", value); }
    public int CarLoan { get => GetInt("CarLoan"); set => Set("CarLoan", value); }
    public int CreditCard { get => GetInt("CreditCard"); set => Set("CreditCard", value); }
    public int SmallCredits { get => GetInt("SmallCredits"); set => Set("SmallCredits", value); }

    public int BankLoan { get => GetInt("BankLoan"); set => Set("BankLoan", value); }
    public int Others { get => GetInt("Others"); set => Set("Others", value); }

    public int Children { get => GetInt("Children"); set => Set("Children", value); }
    public int PerChild { get => GetInt("PerChild"); set => Set("PerChild", value); }
    public int ChildrenExpenses => Children * PerChild;

    private Asset Boat => new User(Id).Person.Assets.Boat;
    private int BoatLoan => Boat?.CashFlow ?? 0;

    public string Description
    {
        get
        {
            var expensesTerm = Terms.Get(54, Id, "Expenses");
            var taxesTerm = Terms.Get(58, Id, "Taxes");
            var mortgageTerm = Terms.Get(59, Id, "Mortgage/Rent Pay");
            var schoolLoanTerm = Terms.Get(44, Id, "School Loan");
            var carLoanTerm = Terms.Get(45, Id, "Car Loan");
            var creditCardTerm = Terms.Get(46, Id, "Credit Card");
            var smallCreditsTerm = Terms.Get(92, Id, "Small Credit");
            var bankLoanTerm = Terms.Get(47, Id, "Bank Loan");
            var boatLoanTerm = Terms.Get(114, Id, "Boat Loan");
            var otherPaymentTerm = Terms.Get(60, Id, "Other Payments");
            var childrenTerm = Terms.Get(61, Id, "Children");
            var childrenExpensesTerm = Terms.Get(62, Id, "Children Expenses");
            var perChildTerm = Terms.Get(63, Id, "per child");

            var expenses = $"{Environment.NewLine}{Environment.NewLine}*{expensesTerm}:*{Environment.NewLine}";
            expenses += $"*{taxesTerm}:* {Taxes.AsCurrency()}{Environment.NewLine}";
            if (Mortgage > 0) expenses += $"*{mortgageTerm}:* {Mortgage.AsCurrency()}{Environment.NewLine}";
            if (SchoolLoan > 0) expenses += $"*{schoolLoanTerm}:* {SchoolLoan.AsCurrency()}{Environment.NewLine}";
            if (CarLoan > 0) expenses += $"*{carLoanTerm}:* {CarLoan.AsCurrency()}{Environment.NewLine}";
            if (CreditCard > 0) expenses += $"*{creditCardTerm}:* {CreditCard.AsCurrency()}{Environment.NewLine}";
            if (SmallCredits > 0) expenses += $"*{smallCreditsTerm}:* {SmallCredits.AsCurrency()}{Environment.NewLine}";
            if (BankLoan > 0) expenses += $"*{bankLoanTerm}:* {BankLoan.AsCurrency()}{Environment.NewLine}";
            if (BoatLoan > 0) expenses += $"*{boatLoanTerm}:* {BoatLoan.AsCurrency()}{Environment.NewLine}";
            expenses += $"*{otherPaymentTerm}:* {Others.AsCurrency()}{Environment.NewLine}";
            if (ChildrenExpenses > 0) expenses += $"*{childrenTerm}:* {Children} ({PerChild.AsCurrency()} {perChildTerm}){Environment.NewLine}";
            if (ChildrenExpenses > 0) expenses += $"*{childrenExpensesTerm}:* {ChildrenExpenses.AsCurrency()}{Environment.NewLine}";

            return expenses;
        }
    }

    public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

    public void Create(Persons.DefaultExpenses expenses)
    {
        Clear();
        DB.Execute($"INSERT INTO {Table} " +
                   "(ID, Taxes, Mortgage, SchoolLoan, CarLoan, CreditCard, SmallCredits, BankLoan, Others, Children, PerChild) " +
                   $"VALUES ({Id}, '', '', '', '', '', '', '', '', '', '')");

        Taxes = expenses.Taxes;
        Mortgage = expenses.Mortgage;
        SchoolLoan = expenses.SchoolLoan;
        CarLoan = expenses.CarLoan;
        CreditCard = expenses.CreditCard;
        SmallCredits = expenses.SmallCredits;
        BankLoan = expenses.BankLoan;
        Others = expenses.Others;
        PerChild = expenses.PerChild;
        Children = 0;
    }
}