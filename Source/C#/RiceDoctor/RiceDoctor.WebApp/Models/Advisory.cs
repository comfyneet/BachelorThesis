using RiceDoctor.InferenceEngine;

namespace RiceDoctor.WebApp.Models
{
    public class Advisory
    {
        public string Guid { get; set; }

        public Request Request { get; set; }

        public IInferenceEngine Engine { get; set; }
    }
}