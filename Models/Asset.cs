﻿using System;
using CashFlowBot.DataBase;
using CashFlowBot.Extensions;

namespace CashFlowBot.Models
{
    public class Asset
    {
        private long UserId { get; }
        private long Id { get; }
        private string Table { get; }

        private string Get(string column) => DB.GetValue($"SELECT {column} FROM {Table} WHERE ID = {Id} AND UserId = {UserId}");
        private int GetInt(string column) => Get(column).ToInt();
        private void Set(string column, int value) => DB.Execute($"UPDATE {Table} SET {column} = {value} WHERE ID = {Id} AND UserId = {UserId}");
        private void Set(string column, string value) => DB.Execute($"UPDATE {Table} SET {column} = '{value}' WHERE ID = {Id} AND UserId = {UserId}");

        public Asset(long userId, int id) => (UserId, Id, Table) = (userId, id, DB.Tables.Assets);

        public string Description => $"*{Title}* - {Qtty} @ ${Price}{Environment.NewLine}";

        public string Title { get => Get("Title"); set => Set("Title", value); }
        public int Price { get => GetInt("Price"); set => Set("Price", value); }
        public int Qtty { get => GetInt("Qtty"); set => Set("Qtty", value); }

        public void Delete() => DB.Execute($"DELETE FROM {Table} WHERE ID = {Id} AND UserId = {UserId}");
    }
}
