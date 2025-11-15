using System.Text.Json;

namespace CashFlow.Extensions;

public static class DtoExtensions
{
    public static string Serialize(this object dto) => JsonSerializer.Serialize(dto);

    public static T Deserialize<T>(this string dto) => JsonSerializer.Deserialize<T>(dto);

    public static T Clone<T>(this T dto) => dto.Serialize().Deserialize<T>();
}
