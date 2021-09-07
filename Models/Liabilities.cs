﻿using CashFlowBot.Data;
using CashFlowBot.DataBase;

namespace CashFlowBot.Models
{
    public class Liabilities : DataModel
    {
        public Liabilities(long id) : base(id, DB.Tables.Liabilities) { }

        public int Mortgage { get => GetInt("Mortgage"); set => Set("Mortgage", value); }
        public int SchoolLoan { get => GetInt("SchoolLoan"); set => Set("SchoolLoan", value); }
        public int CarLoan { get => GetInt("CarLoan"); set => Set("CarLoan", value); }
        public int CreditCard { get => GetInt("CreditCard"); set => Set("CreditCard", value); }
        public int BankLoan { get => GetInt("BankLoan"); set => Set("BankLoan", value); }

        public void Clear() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id}");

        public void Create(Persons.DefaultLiabilities liabilities)
        {
            Clear();
            DB.Execute($"INSERT INTO {Table} ({DB.ColumnNames.Liabilities}) VALUES ({Id}, {DB.DefaultValues.Liabilities})");

            Mortgage = liabilities.Mortgage;
            SchoolLoan = liabilities.SchoolLoan;
            CarLoan = liabilities.CarLoan;
            CreditCard = liabilities.CreditCard;
            BankLoan = liabilities.BankLoan;
        }
    }
}
