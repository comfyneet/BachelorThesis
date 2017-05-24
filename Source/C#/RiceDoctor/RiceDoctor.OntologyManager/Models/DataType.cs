using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RiceDoctor.OntologyManager
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataType
    {
        Unknown,
        Enumerated,
        Int,
        String,
        Boolean
    }
}