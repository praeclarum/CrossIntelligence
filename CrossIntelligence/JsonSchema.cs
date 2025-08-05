using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace CrossIntelligence;

public static class TypeExtensions
{
    static readonly ConcurrentDictionary<Type, JSchema> schemaCache = new ConcurrentDictionary<Type, JSchema>();
    public static JSchema GetJsonSchemaObject(this Type type)
    {
        if (schemaCache.TryGetValue(type, out var cachedSchema))
        {
            return cachedSchema;
        }
        var sgen = new JSchemaGenerator()
        {
            DefaultRequired = Required.Always,
        };
        var schema = sgen.Generate(type);
        if (schema is null)
        {
            throw new Exception($"Failed to generate JSON schema for type: {type.Name}.");
        }
        schema.AllowAdditionalProperties = false;
        schemaCache[type] = schema;
        return schema;
    }

    public static string GetJsonSchema(this Type type)
    {
        var schema = type.GetJsonSchemaObject();
        var sw = new StringWriter();
        schema.WriteTo(new JsonTextWriter(sw));
        var schemaText = sw.ToString();
        return schemaText;
    }
}
