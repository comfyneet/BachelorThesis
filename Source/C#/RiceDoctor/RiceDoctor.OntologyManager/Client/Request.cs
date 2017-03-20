using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public class Request : ISerializable
    {
        public Request(RequestType type, [CanBeNull] IReadOnlyDictionary<string, object> data)
        {
            Type = type;
            Data = data;
        }

        public RequestType Type { get; }

        [CanBeNull]
        public IReadOnlyDictionary<string, object> Data { get; }

        public string Serialize()
        {
            return JsonConvert.Serialize(this);
        }
    }
}