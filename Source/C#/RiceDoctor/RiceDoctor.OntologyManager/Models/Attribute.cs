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
        private bool _canGetDirectDomains;
        [CanBeNull] private IReadOnlyCollection<Class> _directDomains;

        public Attribute([NotNull] string id, [CanBeNull] string label = null, [CanBeNull] DataType? range = null)
            : base(id, label)
        {
            Range = range;
        }

        public DataType? Range { get; }

        public override EntityType Type => EntityType.Attribute;

        [CanBeNull]
        public IReadOnlyCollection<Class> DirectDomains
        {
            get
            {
                if (_canGetDirectDomains) return _directDomains;

                _directDomains = Manager.Instance.GetAttributeDomains(Id, GetDirect);
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

                _allDomains = Manager.Instance.GetAttributeDomains(Id, GetAll);
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
    }
}