using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;
using static RiceDoctor.OntologyManager.ResponseType;
using JsonConvert = RiceDoctor.Shared.JsonConvert;

namespace RiceDoctor.OntologyManager
{
    public class Manager : IOntologyManager
    {
        private readonly Client _client;

        static Manager()
        {
            Instance = new Manager("127.0.0.1", 2002);
        }

        [JsonConstructor]
        private Manager()
        {
        }

        private Manager([NotNull] string ip, int port)
        {
            Check.NotEmpty(ip, nameof(ip));

            _client = new Client(ip, port);
        }

        [NotNull]
        public static Manager Instance { get; }

        public string GetComment(string objectName)
        {
            Check.NotEmpty(objectName, nameof(objectName));

            var data = new Dictionary<string, object> {{"Object", objectName}};
            var request = new Request(RequestType.GetComment, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var comment = (string) response.Data["Comment"];

            return comment;
        }

        public Class GetClass(string className)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object> {{"Class", className}};
            var request = new Request(RequestType.GetClass, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonClass = (JObject) response.Data["Class"];
            var @class = Class.Deserialize(jsonClass.ToString());

            return @class;
        }

        public IReadOnlyCollection<Class> GetSuperClasses(string className, GetType getSuperClassType)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object>
            {
                {"Class", className},
                {"GetSuperClassType", getSuperClassType}
            };
            var request = new Request(RequestType.GetSuperClasses, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonSuperClasses = (JArray) response.Data["SuperClasses"];
            var superClasses = jsonSuperClasses
                .Select(c => Class.Deserialize(c.ToString()))
                .OrderBy(c => c.Label ?? c.Id)
                .ToList();

            return superClasses;
        }

        public IReadOnlyList<Class> GetSubClasses(string className, GetType getSubClassType)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object>
            {
                {"Class", className},
                {"GetSubClassType", getSubClassType}
            };
            var request = new Request(RequestType.GetSubClasses, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonSubClasses = (JArray) response.Data["SubClasses"];
            var subClasses = jsonSubClasses
                .Select(c => Class.Deserialize(c.ToString()))
                .OrderBy(c => c.Label ?? c.Id)
                .ToList();

            return subClasses;
        }

        public IReadOnlyCollection<Relation> GetDomainRelations(string className)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object> {{"Class", className}};
            var request = new Request(RequestType.GetDomainRelations, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelations = (JArray) response.Data["DomainRelations"];
            var relations = jsonRelations
                .Select(r => Relation.Deserialize(r.ToString()))
                .OrderBy(r => r.Label ?? r.Id)
                .ToList();

            return relations;
        }

        public IReadOnlyCollection<Relation> GetRangeRelations(string className)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object> {{"Class", className}};
            var request = new Request(RequestType.GetRangeRelations, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelations = (JArray) response.Data["RangeRelations"];
            var relations = jsonRelations
                .Select(r => Relation.Deserialize(r.ToString()))
                .OrderBy(r => r.Label ?? r.Id)
                .ToList();

            return relations;
        }

        public IReadOnlyCollection<Attribute> GetClassAttributes(string className)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object> {{"Class", className}};
            var request = new Request(RequestType.GetClassAttributes, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonAttributes = (JArray) response.Data["ClassAttributes"];
            var attributes = jsonAttributes
                .Select(a => Attribute.Deserialize(a.ToString()))
                .OrderBy(a => a.Label ?? a.Id)
                .ToList();

            return attributes;
        }

        public IReadOnlyCollection<Individual> GetClassIndividuals(string className, GetType getIndividualType)
        {
            Check.NotEmpty(className, nameof(className));

            var data = new Dictionary<string, object>
            {
                {"Class", className},
                {"GetIndividualType", getIndividualType}
            };
            var request = new Request(RequestType.GetClassIndividuals, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonIndividuals = (JArray) response.Data["ClassIndividuals"];
            var individuals = jsonIndividuals
                .Select(i => JsonTemplates.JsonIndividual.Deserialize(i.ToString()))
                .OrderBy(i => i.Label ?? i.Id)
                .ToList();

            return individuals;
        }

        public Relation GetRelation(string relationName)
        {
            Check.NotEmpty(relationName, nameof(relationName));

            var data = new Dictionary<string, object> {{"Relation", relationName}};
            var request = new Request(RequestType.GetRelation, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelation = (JObject) response.Data["Relation"];
            var relation = Relation.Deserialize(jsonRelation.ToString());

            return relation;
        }

        public IReadOnlyList<Relation> GetRelations()
        {
            var request = new Request(RequestType.GetRelations, null);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelations = (JArray) response.Data["Relations"];
            var relations = jsonRelations
                .Select(r => Relation.Deserialize(r.ToString()))
                .OrderBy(r => r.Label ?? r.Id)
                .ToList();

            return relations;
        }

        public Relation GetInverseRelation(string relationName)
        {
            Check.NotEmpty(relationName, nameof(relationName));

            var data = new Dictionary<string, object> {{"Relation", relationName}};
            var request = new Request(RequestType.GetInverseRelation, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelation = (JObject) response.Data["InverseRelation"];
            var relation = Relation.Deserialize(jsonRelation.ToString());

            return relation;
        }

        public IReadOnlyCollection<Class> GetRelationDomains(string relationName, GetType getDomainType)
        {
            Check.NotEmpty(relationName, nameof(relationName));

            var data = new Dictionary<string, object>
            {
                {"Relation", relationName},
                {"GetDomainType", getDomainType}
            };
            var request = new Request(RequestType.GetRelationDomains, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonDomains = (JArray) response.Data["RelationDomains"];
            var domains = jsonDomains
                .Select(d => Class.Deserialize(d.ToString()))
                .OrderBy(d => d.Label ?? d.Id)
                .ToList();

            return domains;
        }

        public IReadOnlyCollection<Class> GetRelationRanges(string relationName, GetType getRangeType)
        {
            Check.NotEmpty(relationName, nameof(relationName));

            var data = new Dictionary<string, object>
            {
                {"Relation", relationName},
                {"GetRangeType", getRangeType}
            };
            var request = new Request(RequestType.GetRelationRanges, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRanges = (JArray) response.Data["RelationRanges"];
            var ranges = jsonRanges
                .Select(r => Class.Deserialize(r.ToString()))
                .OrderBy(r => r.Label ?? r.Id)
                .ToList();

            return ranges;
        }

        public Attribute GetAttribute(string attributeName)
        {
            Check.NotEmpty(attributeName, nameof(attributeName));

            var data = new Dictionary<string, object> {{"Attribute", attributeName}};
            var request = new Request(RequestType.GetAttribute, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonAttribute = (JObject) response.Data["Attribute"];
            var attribute = Attribute.Deserialize(jsonAttribute.ToString());

            return attribute;
        }

        public IReadOnlyList<Attribute> GetAttributes()
        {
            var request = new Request(RequestType.GetAttributes, null);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonAttributes = (JArray) response.Data["Attributes"];
            var attributes = jsonAttributes
                .Select(a => Attribute.Deserialize(a.ToString()))
                .OrderBy(a => a.Label ?? a.Id)
                .ToList();

            return attributes;
        }

        public IReadOnlyCollection<Class> GetAttributeDomains(string attributeName, GetType getDomainType)
        {
            Check.NotEmpty(attributeName, nameof(attributeName));

            var data = new Dictionary<string, object>
            {
                {"Attribute", attributeName},
                {"GetDomainType", getDomainType}
            };
            var request = new Request(RequestType.GetAttributeDomains, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonDomains = (JArray) response.Data["AttributeDomains"];
            var domains = jsonDomains
                .Select(d => Class.Deserialize(d.ToString()))
                .OrderBy(d => d.Label ?? d.Id)
                .ToList();

            return domains;
        }

        public Individual GetIndividual(string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var data = new Dictionary<string, object> {{"Individual", individualName}};
            var request = new Request(RequestType.GetIndividual, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonIndividual = response.Data["Individual"].ToString();
            var individual = JsonTemplates.JsonIndividual.Deserialize(jsonIndividual);

            return individual;
        }

        public IReadOnlyCollection<Individual> GetIndividuals()
        {
            var request = new Request(RequestType.GetIndividuals, null);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonIndividuals = (JArray) response.Data["Individuals"];
            var individuals = jsonIndividuals
                .Select(i => JsonTemplates.JsonIndividual.Deserialize(i.ToString()))
                .OrderBy(i => i.Label ?? i.Id)
                .ToList();

            return individuals;
        }

        public Class GetIndividualClass(string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var data = new Dictionary<string, object>
            {
                {"Individual", individualName},
                {"GetClassType", GetDirect}
            };
            var request = new Request(RequestType.GetIndividualClasses, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonClass = response.Data["IndividualClass"].ToString();
            var @class = Class.Deserialize(jsonClass);

            return @class;
        }

        public IReadOnlyCollection<Class> GetIndividualClasses(string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var data = new Dictionary<string, object>
            {
                {"Individual", individualName},
                {"GetClassType", GetAll}
            };
            var request = new Request(RequestType.GetIndividualClasses, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonClasses = (JArray) response.Data["IndividualClasses"];
            var classes = jsonClasses
                .Select(d => Class.Deserialize(d.ToString()))
                .OrderBy(d => d.Label ?? d.Id)
                .ToList();

            return classes;
        }

        public IReadOnlyCollection<Individual> GetRelationValue(
            string individualName,
            string relationName)
        {
            Check.NotEmpty(individualName, nameof(individualName));
            Check.NotEmpty(relationName, nameof(relationName));

            var data = new Dictionary<string, object>
            {
                {"Individual", individualName},
                {"Relation", relationName}
            };
            var request = new Request(RequestType.GetRelationValue, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelationValue = response.Data["RelationValue"];
            var relationValue = JsonConvert.Deserialize<IReadOnlyCollection<Individual>>(jsonRelationValue.ToString());

            return relationValue;
        }

        public IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> GetRelationValues(string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var data = new Dictionary<string, object> {{"Individual", individualName}};
            var request = new Request(RequestType.GetRelationValues, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonRelationValues = (JArray) response.Data["RelationValues"];
            var relationValues = jsonRelationValues
                .Select(r => JsonConvert.Deserialize<JsonTemplates.JsonRelationValue>(r.ToString()))
                .Select(r => new JsonTemplates.JsonRelationValue
                {
                    Left = r.Left,
                    Right = r.Right.OrderBy(i => i.Label ?? i.Id).ToList()
                })
                .ToDictionary(r => r.Left, r => r.Right)
                .OrderBy(r => r.Key.Label ?? r.Key.Id)
                .ToDictionary(r => r.Key, r => r.Value);

            return relationValues;
        }

        public IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> GetAttributeValues(string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var data = new Dictionary<string, object> {{"Individual", individualName}};
            var request = new Request(RequestType.GetAttributeValues, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonAttributeValues = (JArray) response.Data["AttributeValues"];
            var attributeValues = jsonAttributeValues
                .Select(a => JsonConvert.Deserialize<JsonTemplates.JsonAttributeValue>(a.ToString()))
                .Select(a => new JsonTemplates.JsonAttributeValue
                {
                    Left = a.Left,
                    Right = a.Right.OrderBy(v => v).ToList()
                })
                .ToDictionary(a => a.Left, a => a.Right)
                .OrderBy(a => a.Key.Label ?? a.Key.Id)
                .ToDictionary(a => a.Key, a => a.Value);

            return attributeValues;
        }

        public IReadOnlyCollection<string> GetAttributeValuesByAttributeName(
            string individualName,
            string attributeName)
        {
            Check.NotEmpty(individualName, nameof(individualName));
            Check.NotEmpty(attributeName, nameof(attributeName));

            var data = new Dictionary<string, object>
            {
                {"Individual", individualName},
                {"Attribute", attributeName}
            };
            var request = new Request(RequestType.GetAttributeValuesByAttributeName, data);

            var response = Send(request);
            if (response.Status != Success) return null;

            var jsonIndividualNames = (JArray) response.Data["AttributeValues"];
            var individualNames = jsonIndividualNames
                .Select(name => name.ToString())
                .OrderBy(name => name)
                .ToList();

            return individualNames;
        }

        public string ThingClassId => "Thing";

        public string NameAttributeId => "name";

        public string TermAttributeId => "term";

        [NotNull]
        private Response Send([NotNull] Request request)
        {
            Check.NotNull(request, nameof(request));

            var requestJson = request.Serialize();
            var responseJson = _client.Send(requestJson);
            var response = Response.Deserialize(responseJson);

            return response;
        }

        private class JsonTemplates
        {
            public class JsonRelationValue
            {
                public Relation Left { get; set; }
                public IReadOnlyCollection<Individual> Right { get; set; }
            }

            public class JsonAttributeValue
            {
                public Attribute Left { get; set; }
                public IReadOnlyCollection<string> Right { get; set; }
            }

            public class JsonSearchIndividual
            {
                public Individual Left { get; set; }
                public IReadOnlyCollection<JsonAttributeValue> Right { get; set; }
            }

            public class JsonIndividual
            {
                public string Id { get; set; }
                public string Label { get; set; }
                public EntityType Type { get; set; }
                public IReadOnlyCollection<JsonRelationValue> RelationValues { get; set; }
                public IReadOnlyCollection<JsonAttributeValue> AttributeValues { get; set; }

                [NotNull]
                public static Individual Deserialize([NotNull] string jsonIndividual)
                {
                    Check.NotEmpty(jsonIndividual, nameof(jsonIndividual));

                    var templateIndividual = JsonConvert.Deserialize<JsonIndividual>(jsonIndividual);

                    var individual = new Individual(templateIndividual.Id, templateIndividual.Label);

                    if (templateIndividual.AttributeValues != null)
                        individual.SetAttributeValues(templateIndividual
                            .AttributeValues
                            .ToDictionary(a => a.Left, a => a.Right));

                    if (templateIndividual.RelationValues != null)
                        individual.SetRelationValues(templateIndividual
                            .RelationValues
                            .ToDictionary(r => r.Left, r => r.Right));

                    return individual;
                }
            }
        }
    }
}