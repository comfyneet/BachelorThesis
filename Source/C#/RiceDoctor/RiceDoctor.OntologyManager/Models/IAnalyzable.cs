namespace RiceDoctor.OntologyManager
{
    public interface IAnalyzable
    {
        string Id { get; }

        bool IsDocumentAnalyzable { get; }

        bool IsOntologyAnalyzable { get; }
    }
}