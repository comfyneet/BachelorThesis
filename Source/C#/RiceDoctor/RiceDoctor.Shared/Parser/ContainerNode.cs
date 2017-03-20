using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public abstract class ContainerNode<T> : Node<T>
    {
        [NotNull] private readonly List<Node<T>> _childNodes;

        protected ContainerNode()
        {
            _childNodes = new List<Node<T>>();
            ChildNodes = _childNodes.AsReadOnly();
        }

        [NotNull]
        protected IReadOnlyList<Node<T>> ChildNodes { get; [param: NotNull] private set; }

        [NotNull]
        public virtual Node<T> Append([NotNull] Node<T> node)
        {
            Check.NotNull(node, nameof(node));

            node.Parent = this;
            if (_childNodes.Count > 0)
            {
                var prevNode = _childNodes.Last();
                node.PrevNode = prevNode;
                prevNode.NextNode = node;
            }

            _childNodes.Add(node);
            ChildNodes = _childNodes.AsReadOnly();

            return node;
        }
    }
}