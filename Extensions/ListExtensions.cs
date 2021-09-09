using System.Collections.Generic;
using System.Linq;

namespace CashFlowBot.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<string> AsCurrency(this IEnumerable<string> list) => list.Select(x => x.ToInt()).AsCurrency();
        public static IEnumerable<string> AsCurrency(this IEnumerable<int> list) => list.Select(x => x.AsCurrency());
    }
}
