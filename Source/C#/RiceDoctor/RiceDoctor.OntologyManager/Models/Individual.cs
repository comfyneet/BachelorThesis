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
        private bool _canGetRelationValues;
        [CanBeNull] private Class _directClass;
        [CanBeNull] private IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> _relationValues;

        public Individual([NotNull] string id, [CanBeNull] string label = null) : base(id, label)
        {
        }

        public override EntityType Type => EntityType.Individual;

        [CanBeNull]
        public Class DirectClass
        {
            get
            {
                if (_canGetDirectClass) return _directClass;

                _directClass = Manager.Instance.GetIndividualClass(Id);
                _canGetDirectClass = true;

                return _directClass;
            }
            set
            {
                if (_canGetDirectClass)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectClass)));

                _directClass = value;
                _canGetDirectClass = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> AllClasses
        {
            get
            {
                if (_canGetAllClasses) return _allClasses;

                _allClasses = Manager.Instance.GetIndividualClasses(Id);
                _canGetAllClasses = true;

                return _allClasses;
            }
            set
            {
                if (_canGetAllClasses)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllClasses)));

                _allClasses = value;
                _canGetAllClasses = true;
            }
        }

        [CanBeNull]
        public IReadOnlyDictionary<Relation, IReadOnlyCollection<Individual>> RelationValues
        {
            get
            {
                if (_canGetRelationValues) return _relationValues;

                _relationValues = Manager.Instance.GetRelationValues(Id);
                _canGetRelationValues = true;

                return _relationValues;
            }
            set
            {
                if (_canGetRelationValues)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(RelationValues)));

                _relationValues = value;
                _canGetRelationValues = true;
            }
        }

        [CanBeNull]
        public IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>> AttributeValues
        {
            get
            {
                if (_canGetAttributeValues) return _attributeValues;

                _attributeValues = Manager.Instance.GetAttributeValues(Id);
                _canGetAttributeValues = true;

                return _attributeValues;
            }
            set
            {
                if (_canGetAttributeValues)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AttributeValues)));

                _attributeValues = value;
                _canGetAttributeValues = true;
            }
        }
    }
}