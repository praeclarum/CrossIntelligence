using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

namespace CrossIntelligence;

public static class TypeExtensions
{
    public static string GetJsonSchema(this Type type)
    {
        var sgen = new JSchemaGenerator()
        {
            DefaultRequired = Required.DisallowNull,
        };
        var schema = sgen.Generate(type);
        if (schema is null)
        {
            throw new Exception($"Failed to generate JSON schema for type: {type.Name}.");
        }
        schema.AllowAdditionalProperties = false;
        var sw = new StringWriter();
        schema.WriteTo(new JsonTextWriter(sw));
        var schemaText = sw.ToString();
        return schemaText;
    }
}
