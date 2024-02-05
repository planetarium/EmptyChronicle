using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bencodex.Json;
using Bencodex.Types;

namespace EmptyChronicle.Utility;

public static class BencodexExtension
{
    public static JsonNode? ToJson(this IValue value)
    {
        var converter = new BencodexJsonConverter();
        var buffer = new MemoryStream();
        var writer = new Utf8JsonWriter(buffer);
        converter.Write(writer, value, new JsonSerializerOptions());
        var json = Encoding.UTF8.GetString(buffer.ToArray());

        return JsonNode.Parse(json);
    }
}
