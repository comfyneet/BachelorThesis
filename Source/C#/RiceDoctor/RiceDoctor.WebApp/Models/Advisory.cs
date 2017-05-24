using System.Collections.Generic;
using RiceDoctor.InferenceEngine;
using RiceDoctor.RuleManager;

namespace RiceDoctor.WebApp.Models
{
    public class Advisory
    {
        public string Guid { get; set; }

        public Request Request { get; set; }

        public Engine Engine { get; set; }

        public IReadOnlyCollection<Fact> Results { get; set; }
    }
}