using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.OntologyManager
{
    public class Class : Entity<Class>
    {
        [CanBeNull] private IReadOnlyCollection<Individual> _allIndividuals;
        [CanBeNull] private IReadOnlyCollection<Class> _allSubClasses;
        [CanBeNull] private IReadOnlyCollection<Class> _allSuperClasses;
        [CanBeNull] private IReadOnlyCollection<Attribute> _attributes;
        private bool _canGetAllIndividuals;
        private bool _canGetAllSubClasses;
        private bool _canGetAllSuperClasses;
        private bool _canGetAttributes;
        private bool _canGetDirectIndividuals;
        private bool _canGetDirectSubClasses;
        private bool _canGetDirectSuperClasses;
        private bool _canGetDomainRelations;
        private bool _canGetRangeRelations;
        [CanBeNull] private IReadOnlyCollection<Individual> _directIndividuals;
        [CanBeNull] private IReadOnlyCollection<Class> _directSubClasses;
        [CanBeNull] private IReadOnlyCollection<Class> _directSuperClasses;
        [CanBeNull] private IReadOnlyCollection<Relation> _domainRelations;
        [CanBeNull] private IReadOnlyCollection<Relation> _rangeRelations;

        public Class([NotNull] string id, [CanBeNull] string label = null) : base(id, label)
        {
        }

        public override EntityType Type => EntityType.Class;

        [CanBeNull]
        public IReadOnlyCollection<Class> DirectSuperClasses
        {
            get
            {
                if (_canGetDirectSuperClasses) return _directSuperClasses;

                _directSuperClasses = Manager.Instance.GetSuperClasses(Id, GetDirect);
                _canGetDirectSuperClasses = true;

                return _directSuperClasses;
            }
            set
            {
                if (_canGetDirectSuperClasses)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectSuperClasses)));

                _directSuperClasses = value;
                _canGetDirectSuperClasses = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> AllSuperClasses
        {
            get
            {
                if (_canGetAllSuperClasses) return _allSuperClasses;

                _allSuperClasses = Manager.Instance.GetSuperClasses(Id, GetAll);
                _canGetAllSuperClasses = true;

                return _allSuperClasses;
            }
            set
            {
                if (_canGetAllSuperClasses)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllSuperClasses)));

                _allSuperClasses = value;
                _canGetAllSuperClasses = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> DirectSubClasses
        {
            get
            {
                if (_canGetDirectSubClasses) return _directSubClasses;

                _directSubClasses = Manager.Instance.GetSubClasses(Id, GetDirect);
                _canGetDirectSubClasses = true;

                return _directSubClasses;
            }
            set
            {
                if (_canGetDirectSubClasses)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectSubClasses)));

                _directSubClasses = value;
                _canGetDirectSubClasses = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> AllSubClasses
        {
            get
            {
                if (_canGetAllSubClasses) return _allSubClasses;

                _allSubClasses = Manager.Instance.GetSubClasses(Id, GetAll);
                _canGetAllSubClasses = true;

                return _allSubClasses;
            }
            set
            {
                if (_canGetAllSubClasses)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllSubClasses)));

                _allSubClasses = value;
                _canGetAllSubClasses = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Relation> DomainRelations
        {
            get
            {
                if (_canGetDomainRelations) return _domainRelations;

                _domainRelations = Manager.Instance.GetDomainRelations(Id);
                _canGetDomainRelations = true;

                return _domainRelations;
            }
            set
            {
                if (_canGetDomainRelations)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DomainRelations)));

                _domainRelations = value;
                _canGetDomainRelations = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Relation> RangeRelations
        {
            get
            {
                if (_canGetRangeRelations) return _rangeRelations;

                _rangeRelations = Manager.Instance.GetRangeRelations(Id);
                _canGetRangeRelations = true;

                return _rangeRelations;
            }
            set
            {
                if (_canGetRangeRelations)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(RangeRelations)));

                _rangeRelations = value;
                _canGetRangeRelations = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Attribute> Attributes
        {
            get
            {
                if (_canGetAttributes) return _attributes;

                _attributes = Manager.Instance.GetClassAttributes(Id);
                _canGetAttributes = true;

                return _attributes;
            }
            set
            {
                if (_canGetAttributes)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(Attributes)));

                _attributes = value;
                _canGetAttributes = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Individual> DirectIndividuals
        {
            get
            {
                if (_canGetDirectIndividuals) return _directIndividuals;

                _directIndividuals = Manager.Instance.GetClassIndividuals(Id, GetDirect);
                _canGetDirectIndividuals = true;

                return _directIndividuals;
            }
            set
            {
                if (_canGetDirectIndividuals)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectIndividuals)));

                _directIndividuals = value;
                _canGetDirectIndividuals = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Individual> AllIndividuals
        {
            get
            {
                if (_canGetAllIndividuals) return _allIndividuals;

                _allIndividuals = Manager.Instance.GetClassIndividuals(Id, GetAll);
                _canGetAllIndividuals = true;

                return _allIndividuals;
            }
            set
            {
                if (_canGetAllIndividuals)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllIndividuals)));

                _allIndividuals = value;
                _canGetAllIndividuals = true;
            }
        }
    }
}