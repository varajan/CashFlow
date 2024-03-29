﻿using System.Collections.Generic;
using System.Linq;

namespace CashFlowBot.Extensions;

public static class ListExtensions
{
    public static IEnumerable<int> ToInt(this IEnumerable<string> list) => list.Select(x => x.ToInt());
    public static IEnumerable<long> ToLong(this IEnumerable<string> list) => list.Select(x => x.ToLong());
    public static IEnumerable<string> AsCurrency(this IEnumerable<string> list) => list.ToInt().AsCurrency();
    public static IEnumerable<string> AsCurrency(this IEnumerable<int> list) => list.Select(x => x.AsCurrency());
}