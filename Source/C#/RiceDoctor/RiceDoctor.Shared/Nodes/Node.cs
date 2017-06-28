using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public abstract class Node<T>
    {
        [CanBeNull] private Node<T> _nextNode;

        [CanBeNull] private Node<T> _parent;

        [CanBeNull] private Node<T> _prevNode;

        [CanBeNull]
        public Node<T> Parent
        {
            get { return _parent; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _parent = value;
            }
        }

        [CanBeNull]
        public Node<T> PrevNode
        {
            get { return _prevNode; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _prevNode = value;
            }
        }

        [CanBeNull]
        public Node<T> NextNode
        {
            get { return _nextNode; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _nextNode = value;
            }
        }
    }
}