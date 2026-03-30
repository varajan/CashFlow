using System.ComponentModel;

namespace CashFlow.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var info = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }
}