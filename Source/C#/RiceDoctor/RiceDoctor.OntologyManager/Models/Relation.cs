using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.OntologyManager
{
    public class Relation : Entity<Relation>
    {
        [CanBeNull] private IReadOnlyCollection<Class> _allDomains;
        [CanBeNull] private IReadOnlyCollection<Class> _allRanges;
        private bool _canGetAllDomains;
        private bool _canGetAllRanges;
        private bool _canGetDirectDomains;
        private bool _canGetDirectRanges;
        private bool _canGetInverseRelation;
        [CanBeNull] private IReadOnlyCollection<Class> _directDomains;
        [CanBeNull] private IReadOnlyCollection<Class> _directRanges;
        [CanBeNull] private Relation _inverseRelation;

        public Relation([NotNull] string id, [CanBeNull] string label = null) : base(id, label)
        {
        }

        public override EntityType Type => EntityType.Relation;

        [CanBeNull]
        public Relation InverseRelation
        {
            get
            {
                if (_canGetInverseRelation) return _inverseRelation;

                _inverseRelation = Manager.Instance.GetInverseRelation(Id);
                _canGetInverseRelation = true;

                return _inverseRelation;
            }
            set
            {
                if (_canGetInverseRelation)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(InverseRelation)));

                _inverseRelation = value;
                _canGetInverseRelation = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> DirectDomains
        {
            get
            {
                if (_canGetDirectDomains) return _directDomains;

                _directDomains = Manager.Instance.GetRelationDomains(Id, GetDirect);
                _canGetDirectDomains = true;

                return _directDomains;
            }
            set
            {
                if (_canGetDirectDomains)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectDomains)));

                _directDomains = value;
                _canGetDirectDomains = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> AllDomains
        {
            get
            {
                if (_canGetAllDomains) return _allDomains;

                _allDomains = Manager.Instance.GetRelationDomains(Id, GetAll);
                _canGetAllDomains = true;

                return _allDomains;
            }
            set
            {
                if (_canGetAllDomains)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllDomains)));

                _allDomains = value;
                _canGetAllDomains = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> DirectRanges
        {
            get
            {
                if (_canGetDirectRanges) return _directRanges;

                _directRanges = Manager.Instance.GetRelationRanges(Id, GetDirect);
                _canGetDirectRanges = true;

                return _directRanges;
            }
            set
            {
                if (_canGetDirectRanges)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(DirectRanges)));

                _directRanges = value;
                _canGetDirectRanges = true;
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> AllRanges
        {
            get
            {
                if (_canGetAllRanges) return _allRanges;

                _allRanges = Manager.Instance.GetRelationRanges(Id, GetAll);
                _canGetAllRanges = true;

                return _allRanges;
            }
            set
            {
                if (_canGetAllRanges)
                    throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(AllRanges)));

                _allRanges = value;
                _canGetAllRanges = true;
            }
        }
    }
}