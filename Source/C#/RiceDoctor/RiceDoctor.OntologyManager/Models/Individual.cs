using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public class Individual : Entity<Individual>
    {
        [CanBeNull] private IReadOnlyCollection<Class> _allClasses;
        [CanBeNull] private IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> _attributeValues;
        private bool _canGetAllClasses;
        private bool _canGetAttributeValues;
        private bool _canGetDirectClass;
        private bool _canGetNames;
        private bool _canGetRelationValues;
        [CanBeNull] private Class _directClass;
        [CanBeNull] private IReadOnlyCollection<string> _names;
        [CanBeNull] private IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> _relationValues;

        public Individual([NotNull] string id, [CanBeNull] string label = null) : base(id, label)
        {
        }

        public override EntityType Type => EntityType.Individual;

        [CanBeNull]
        public Class GetDirectClass()
        {
            if (_canGetDirectClass) return _directClass;

            _directClass = Manager.Instance.GetIndividualClass(Id);
            _canGetDirectClass = true;

            return _directClass;
        }

        public void SetDirectClass([CanBeNull] Class directClass)
        {
            if (_canGetDirectClass)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectClass)));

            _directClass = directClass;
            _canGetDirectClass = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetAllClasses()
        {
            if (_canGetAllClasses) return _allClasses;

            _allClasses = Manager.Instance.GetIndividualClasses(Id);
            _canGetAllClasses = true;

            return _allClasses;
        }

        [CanBeNull]
        public IReadOnlyCollection<string> GetNames()
        {
            if (_canGetNames) return _names;

            _names = Manager.Instance.GetIndividualNames(Id);
            _canGetNames = true;

            return _names;
        }

        public void SetAllClasses([CanBeNull] IReadOnlyCollection<Class> allClasses)
        {
            if (_canGetAllClasses)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllClasses)));

            _allClasses = allClasses;
            _canGetAllClasses = true;
        }

        [CanBeNull]
        public IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> GetRelationValues()
        {
            if (_canGetRelationValues) return _relationValues;

            _relationValues = Manager.Instance.GetRelationValues(Id);
            _canGetRelationValues = true;

            return _relationValues;
        }

        public void SetRelationValues(
            [CanBeNull] IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> relationValues)
        {
            if (_canGetRelationValues)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetRelationValues)));

            _relationValues = relationValues;
            _canGetRelationValues = true;
        }

        [CanBeNull]
        public IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> GetAttributeValues()
        {
            if (_canGetAttributeValues) return _attributeValues;

            _attributeValues = Manager.Instance.GetAttributeValues(Id);
            _canGetAttributeValues = true;

            return _attributeValues;
        }

        public void SetAttributeValues(
            [CanBeNull] IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> attributeValues)
        {
            if (_canGetAttributeValues)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAttributeValues)));

            _attributeValues = attributeValues;
            _canGetAttributeValues = true;
        }
    }
}