﻿using System.Collections.Generic;
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
        }

        [NotNull]
        protected IReadOnlyList<Node<T>> ChildNodes => _childNodes;

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

            return node;
        }

        [NotNull]
        public Node<T> Prepend([NotNull] Node<T> node)
        {
            Check.NotNull(node, nameof(node));

            node.Parent = this;
            if (_childNodes.Count > 0)
            {
                var nextNode = _childNodes.First();
                node.NextNode = nextNode;
                nextNode.PrevNode = node;
            }

            _childNodes.Insert(0, node);

            return node;
        }
    }
}