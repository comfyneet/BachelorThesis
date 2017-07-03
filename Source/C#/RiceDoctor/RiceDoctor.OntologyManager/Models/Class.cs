using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.OntologyManager
{
    public class Class : Entity<Class>, IAnalyzable
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
        [CanBeNull] private IReadOnlyList<Class> _directSubClasses;
        [CanBeNull] private IReadOnlyCollection<Class> _directSuperClasses;
        [CanBeNull] private IReadOnlyCollection<Relation> _domainRelations;
        [CanBeNull] private IReadOnlyCollection<Relation> _rangeRelations;

        public Class([NotNull] string id, [CanBeNull] string label = null) : base(id, label)
        {
        }

        public override EntityType Type => EntityType.Class;

        [CanBeNull]
        public IReadOnlyCollection<Class> GetDirectSuperClasses()
        {
            if (_canGetDirectSuperClasses) return _directSuperClasses;

            _directSuperClasses = Manager.Instance.GetSuperClasses(Id, GetDirect);
            _canGetDirectSuperClasses = true;

            return _directSuperClasses;
        }

        public void SetDirectSuperClasses([CanBeNull] IReadOnlyCollection<Class> directSuperClasses)
        {
            if (_canGetDirectSuperClasses)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectSuperClasses)));

            _directSuperClasses = directSuperClasses;
            _canGetDirectSuperClasses = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetAllSuperClasses()
        {
            if (_canGetAllSuperClasses) return _allSuperClasses;

            _allSuperClasses = Manager.Instance.GetSuperClasses(Id, GetAll);
            _canGetAllSuperClasses = true;

            return _allSuperClasses;
        }

        public void SetAllSuperClasses([CanBeNull] IReadOnlyCollection<Class> allSuperClasses)
        {
            if (_canGetAllSuperClasses)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllSuperClasses)));

            _allSuperClasses = allSuperClasses;
            _canGetAllSuperClasses = true;
        }

        [CanBeNull]
        public IReadOnlyList<Class> GetDirectSubClasses()
        {
            if (_canGetDirectSubClasses) return _directSubClasses;

            _directSubClasses = Manager.Instance.GetSubClasses(Id, GetDirect);
            _canGetDirectSubClasses = true;

            return _directSubClasses;
        }

        public void SetDirectSubClasses([CanBeNull] IReadOnlyList<Class> directSubClasses)
        {
            if (_canGetDirectSubClasses)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectSubClasses)));

            _directSubClasses = directSubClasses;
            _canGetDirectSubClasses = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetAllSubClasses()
        {
            if (_canGetAllSubClasses) return _allSubClasses;

            _allSubClasses = Manager.Instance.GetSubClasses(Id, GetAll);
            _canGetAllSubClasses = true;

            return _allSubClasses;
        }

        public void SetAllSubClasses([CanBeNull] IReadOnlyCollection<Class> allSubClasses)
        {
            if (_canGetAllSubClasses)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllSubClasses)));

            _allSubClasses = allSubClasses;
            _canGetAllSubClasses = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Relation> GetDomainRelations()
        {
            if (_canGetDomainRelations) return _domainRelations;

            _domainRelations = Manager.Instance.GetDomainRelations(Id);
            _canGetDomainRelations = true;

            return _domainRelations;
        }

        public void SetDomainRelations([CanBeNull] IReadOnlyCollection<Relation> domainRelations)
        {
            if (_canGetDomainRelations)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDomainRelations)));

            _domainRelations = domainRelations;
            _canGetDomainRelations = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Relation> GetRangeRelations()
        {
            if (_canGetRangeRelations) return _rangeRelations;

            _rangeRelations = Manager.Instance.GetRangeRelations(Id);
            _canGetRangeRelations = true;

            return _rangeRelations;
        }

        public void SetRangeRelations([CanBeNull] IReadOnlyCollection<Relation> rangeRelations)
        {
            if (_canGetRangeRelations)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetRangeRelations)));

            _rangeRelations = rangeRelations;
            _canGetRangeRelations = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Attribute> GetAttributes()
        {
            if (_canGetAttributes) return _attributes;

            _attributes = Manager.Instance.GetClassAttributes(Id);
            _canGetAttributes = true;

            return _attributes;
        }

        public void SetAttributes([CanBeNull] IReadOnlyCollection<Attribute> attributes)
        {
            if (_canGetAttributes)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(GetAttributes)));

            _attributes = attributes;
            _canGetAttributes = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Individual> GetDirectIndividuals()
        {
            if (_canGetDirectIndividuals) return _directIndividuals;

            _directIndividuals = Manager.Instance.GetClassIndividuals(Id, GetDirect);
            _canGetDirectIndividuals = true;

            return _directIndividuals;
        }

        public void SetDirectIndividuals([CanBeNull] IReadOnlyCollection<Individual> directIndividuals)
        {
            if (_canGetDirectIndividuals)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectIndividuals)));

            _directIndividuals = directIndividuals;
            _canGetDirectIndividuals = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Individual> GetAllIndividuals()
        {
            if (_canGetAllIndividuals) return _allIndividuals;

            _allIndividuals = Manager.Instance.GetClassIndividuals(Id, GetAll);
            _canGetAllIndividuals = true;

            return _allIndividuals;
        }

        public void SetAllIndividuals([CanBeNull] IReadOnlyCollection<Individual> allIndividuals)
        {
            if (_canGetAllIndividuals)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllIndividuals)));

            _allIndividuals = allIndividuals;
            _canGetAllIndividuals = true;
        }
    }
}