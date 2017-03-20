using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public class Response
    {
        public Response(ResponseType status, [NotNull] IReadOnlyDictionary<string, object> data)
        {
            Status = status;
            Data = data;
        }

        public ResponseType Status { get; private set; }

        [CanBeNull]
        public IReadOnlyDictionary<string, object> Data { get; private set; }

        [CanBeNull]
        public string Message { get; set; }

        public static Response Deserialize([NotNull] string json)
        {
            Check.NotEmpty(json, nameof(json));

            return JsonConvert.Deserialize<Response>(json);
        }
    }
}