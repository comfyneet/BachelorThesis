using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RiceDoctor.OntologyManager
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        Class,
        Relation,
        Attribute,
        Individual
    }
}