using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.OntologyManager
{
    public class Relation : Entity<Relation>, IEquatable<Relation>
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

        public bool Equals(Relation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other);
        }

        [CanBeNull]
        public Relation GetInverseRelation()
        {
            if (_canGetInverseRelation) return _inverseRelation;

            _inverseRelation = Manager.Instance.GetInverseRelation(Id);
            _canGetInverseRelation = true;

            return _inverseRelation;
        }

        public void SetInverseRelation([CanBeNull] Relation inverseRelation)
        {
            if (_canGetInverseRelation)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetInverseRelation)));

            _inverseRelation = inverseRelation;
            _canGetInverseRelation = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetDirectDomains()
        {
            if (_canGetDirectDomains) return _directDomains;

            _directDomains = Manager.Instance.GetRelationDomains(Id, GetDirect);
            _canGetDirectDomains = true;

            return _directDomains;
        }

        public void SetDirectDomains([CanBeNull] IReadOnlyCollection<Class> directDomains)
        {
            if (_canGetDirectDomains)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectDomains)));

            _directDomains = directDomains;
            _canGetDirectDomains = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetAllDomains()
        {
            if (_canGetAllDomains) return _allDomains;

            _allDomains = Manager.Instance.GetRelationDomains(Id, GetAll);
            _canGetAllDomains = true;

            return _allDomains;
        }

        public void SetAllDomains([CanBeNull] IReadOnlyCollection<Class> allDomains)
        {
            if (_canGetAllDomains)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllDomains)));

            _allDomains = allDomains;
            _canGetAllDomains = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetDirectRanges()
        {
            if (_canGetDirectRanges) return _directRanges;

            _directRanges = Manager.Instance.GetRelationRanges(Id, GetDirect);
            _canGetDirectRanges = true;

            return _directRanges;
        }

        public void SetDirectRanges([CanBeNull] IReadOnlyCollection<Class> directRanges)
        {
            if (_canGetDirectRanges)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetDirectRanges)));

            _directRanges = directRanges;
            _canGetDirectRanges = true;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetAllRanges()
        {
            if (_canGetAllRanges) return _allRanges;

            _allRanges = Manager.Instance.GetRelationRanges(Id, GetAll);
            _canGetAllRanges = true;

            return _allRanges;
        }

        public void SetAllRanges([CanBeNull] IReadOnlyCollection<Class> allRanges)
        {
            if (_canGetAllRanges)
                throw new InvalidOperationException(CoreStrings.CannotSetAgain(nameof(SetAllRanges)));

            _allRanges = allRanges;
            _canGetAllRanges = true;
        }

        public static bool operator ==(Relation relation1, Relation relation2)
        {
            if (ReferenceEquals(relation1, relation2)) return true;
            if (ReferenceEquals(null, relation1)) return false;
            if (ReferenceEquals(null, relation2)) return false;
            return relation1.Equals(relation2);
        }

        public static bool operator !=(Relation relation1, Relation relation2)
        {
            return !(relation1 == relation2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Relation) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}