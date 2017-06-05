using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.OntologyManager
{
    public class Attribute : Entity<Attribute>
    {
        [CanBeNull] private IReadOnlyCollection<Class> _allDomains;
        private bool _canGetAllDomains;
        private bool _canGetComment;
        private bool _canGetDirectDomains;
        [CanBeNull] private string _comment;
        [CanBeNull] private IReadOnlyCollection<Class> _directDomains;

        public Attribute(
            [NotNull] string id,
            [CanBeNull] string label = null,
            [CanBeNull] string comment = null,
            [CanBeNull] DataType? range = null,
            [CanBeNull] IReadOnlyList<string> enumeratedValues = null)
            : base(id, label)
        {
            if (comment != null)
            {
                _comment = comment;
                _canGetComment = false;
            }

            Range = range;
            if (Range == DataType.Enumerated)
            {
                Check.NotEmpty(enumeratedValues, nameof(enumeratedValues));
                EnumeratedValues = enumeratedValues;
            }
        }

        public DataType? Range { get; }

        [CanBeNull]
        public IReadOnlyList<string> EnumeratedValues { get; }

        public override EntityType Type => EntityType.Attribute;

        [CanBeNull]
        public string GetComment()
        {
            if (_canGetComment) return _comment;

            _comment = Manager.Instance.GetComment(Id);
            _canGetComment = true;

            return _comment;
        }

        [CanBeNull]
        public IReadOnlyCollection<Class> GetDirectDomains()
        {
            if (_canGetDirectDomains) return _directDomains;

            _directDomains = Manager.Instance.GetAttributeDomains(Id, GetDirect);
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

            _allDomains = Manager.Instance.GetAttributeDomains(Id, GetAll);
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
    }
}