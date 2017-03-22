using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RiceDoctor.Shared
{
    public static class JsonConvert
    {
        [NotNull]
        public static string Serialize([NotNull] object value)
        {
            Check.NotNull(value, nameof(value));

            return Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
        }

        [NotNull]
        public static T Deserialize<T>([NotNull] string value)
        {
            Check.NotEmpty(value, nameof(value));

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }
    }
}