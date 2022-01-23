﻿using System.Collections.Generic;
using CashFlowBot.DataBase;

namespace CashFlowBot.Data
{
    public class AvailableAssets
    {
        public static List<string> Get(AssetType type) =>
            DB.GetColumn($"SELECT Value FROM AvailableAssets WHERE Type = {(int) type} ORDER BY CAST(Value as Number)");

        public static void Clear(AssetType type) =>
            DB.Execute($"DELETE FROM AvailableAssets WHERE Type = {(int) type}");

        public static void ClearAll() => DB.Execute("DROP TABLE AvailableAssets");

        public static void Add(int value, AssetType type) => Add(value.ToString(), type);
        public static void Add(string value, AssetType type)
        {
            if (Get(type).Contains(value)) return;

            DB.Execute("INSERT INTO AvailableAssets (Type, Value) " +
                       $"VALUES ({(int)type}, '{value}')");
        }
    }
}
