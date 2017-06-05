using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.OntologyManager
{
    public interface IOntologyManager
    {
        [CanBeNull]
        IReadOnlyCollection<Individual> SearchIndividuals([NotNull] string keywords);

        [CanBeNull]
        string GetComment([NotNull] string objectName);

        [CanBeNull]
        Class GetClass([NotNull] string className);

        [CanBeNull]
        IReadOnlyCollection<Class> GetSuperClasses([NotNull] string className, GetType getSuperClassType);

        [CanBeNull]
        IReadOnlyCollection<Class> GetSubClasses([NotNull] string className, GetType getSubClassType);

        [CanBeNull]
        IReadOnlyCollection<Relation> GetDomainRelations([NotNull] string className);

        [CanBeNull]
        IReadOnlyCollection<Relation> GetRangeRelations([NotNull] string className);

        [CanBeNull]
        IReadOnlyCollection<Attribute> GetClassAttributes([NotNull] string className);

        [CanBeNull]
        IReadOnlyCollection<Individual> GetClassIndividuals([NotNull] string className, GetType getIndividualType);

        [CanBeNull]
        Relation GetRelation([NotNull] string relationName);

        [CanBeNull]
        IReadOnlyCollection<Relation> GetRelations();

        [CanBeNull]
        Relation GetInverseRelation([NotNull] string relationName);

        [CanBeNull]
        IReadOnlyCollection<Class> GetRelationDomains([NotNull] string relationName, GetType getDomainType);

        [CanBeNull]
        IReadOnlyCollection<Class> GetRelationRanges([NotNull] string relationName, GetType getRangeType);

        [CanBeNull]
        Attribute GetAttribute([NotNull] string attributeName);

        [CanBeNull]
        IReadOnlyCollection<Attribute> GetAttributes();

        [CanBeNull]
        IReadOnlyCollection<Class> GetAttributeDomains([NotNull] string attributeName, GetType getDomainType);

        [CanBeNull]
        Individual GetIndividual([NotNull] string individualName);

        [CanBeNull]
        IReadOnlyCollection<Individual> GetIndividuals();

        [CanBeNull]
        Class GetIndividualClass([NotNull] string individualName);

        [CanBeNull]
        IReadOnlyCollection<Class> GetIndividualClasses([NotNull] string individualName);

        [CanBeNull]
        IReadOnlyCollection<Individual> GetRelationValue(
            [NotNull] string individualName,
            [NotNull] string relationName);

        [CanBeNull]
        IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> GetRelationValues(
            [NotNull] string individualName);

        [CanBeNull]
        IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> GetAttributeValues([NotNull] string individualName);
    }
}