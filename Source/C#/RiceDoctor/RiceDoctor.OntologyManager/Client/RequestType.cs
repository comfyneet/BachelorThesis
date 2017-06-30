using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RiceDoctor.OntologyManager
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestType
    {
        GetComment,
        GetClass,
        GetSubClasses,
        GetSuperClasses,
        GetDomainRelations,
        GetRangeRelations,
        GetClassAttributes,
        GetClassIndividuals,

        GetRelation,
        GetRelations,
        GetInverseRelation,
        GetRelationDomains,
        GetRelationRanges,

        GetAttribute,
        GetAttributes,
        GetAttributeDomains,

        GetIndividual,
        GetIndividuals,
        GetIndividualClasses,
        GetIndividualNames,
        GetRelationValue,
        GetRelationValues,
        GetAttributeValues
    }
}