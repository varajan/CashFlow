using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashFlow.Extensions;

public static class DtoExtensions
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };

    public static string Serialize(this object dto) => JsonSerializer.Serialize(dto, Options);

    public static T Deserialize<T>(this string dto) => JsonSerializer.Deserialize<T>(dto, Options);

    public static T Clone<T>(this T dto) => dto.Serialize().Deserialize<T>();
}
