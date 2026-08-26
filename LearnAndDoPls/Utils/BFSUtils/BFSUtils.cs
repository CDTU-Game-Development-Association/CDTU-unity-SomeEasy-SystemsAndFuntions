using System;
using System.Collections.Generic;

namespace CDTU.Utils
{
    /// <summary>
    /// Breadth-first search helpers for unweighted graphs.
    /// </summary>
    public sealed class BFSUtil<T>
    {
        public void BFS(
            T start,
            Func<T, IEnumerable<T>> getNeighbors,
            Action<T> process,
            IEqualityComparer<T> comparer = null)
        {
            if (getNeighbors == null)
                throw new ArgumentNullException(nameof(getNeighbors));
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            if (IsNull(start))
                return;

            comparer = comparer ?? EqualityComparer<T>.Default;
            var queue = new Queue<T>();
            var visited = new HashSet<T>(comparer);
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                process(current);
                EnqueueUnvisited(current, getNeighbors, visited, queue);
            }
        }

        public void BFSWithLevel(
            T start,
            Func<T, IEnumerable<T>> getNeighbors,
            Action<T, int> process,
            IEqualityComparer<T> comparer = null)
        {
            if (getNeighbors == null)
                throw new ArgumentNullException(nameof(getNeighbors));
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            if (IsNull(start))
                return;

            comparer = comparer ?? EqualityComparer<T>.Default;
            var queue = new Queue<NodeWithLevel>();
            var visited = new HashSet<T>(comparer);
            queue.Enqueue(new NodeWithLevel(start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                process(current.Node, current.Level);

                foreach (var neighbor in GetNeighbors(current.Node, getNeighbors))
                {
                    if (IsNull(neighbor) || !visited.Add(neighbor))
                        continue;

                    queue.Enqueue(new NodeWithLevel(neighbor, current.Level + 1));
                }
            }
        }

        public List<T> FindShortestPath(
            T start,
            T end,
            Func<T, IEnumerable<T>> getNeighbors,
            IEqualityComparer<T> comparer = null)
        {
            if (getNeighbors == null)
                throw new ArgumentNullException(nameof(getNeighbors));

            var path = new List<T>();
            if (IsNull(start) || IsNull(end))
                return path;

            comparer = comparer ?? EqualityComparer<T>.Default;
            if (comparer.Equals(start, end))
            {
                path.Add(start);
                return path;
            }

            var queue = new Queue<T>();
            var visited = new HashSet<T>(comparer);
            var parents = new Dictionary<T, T>(comparer);
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in GetNeighbors(current, getNeighbors))
                {
                    if (IsNull(neighbor) || !visited.Add(neighbor))
                        continue;

                    parents.Add(neighbor, current);
                    if (comparer.Equals(neighbor, end))
                        return BuildPath(start, neighbor, parents, comparer);

                    queue.Enqueue(neighbor);
                }
            }

            return path;
        }

        public T FindNode(
            T start,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> predicate,
            IEqualityComparer<T> comparer = null)
        {
            if (getNeighbors == null)
                throw new ArgumentNullException(nameof(getNeighbors));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (IsNull(start))
                return default;

            comparer = comparer ?? EqualityComparer<T>.Default;
            var queue = new Queue<T>();
            var visited = new HashSet<T>(comparer);
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (predicate(current))
                    return current;

                foreach (var neighbor in GetNeighbors(current, getNeighbors))
                {
                    if (IsNull(neighbor) || !visited.Add(neighbor))
                        continue;

                    queue.Enqueue(neighbor);
                }
            }

            return default;
        }

        public List<T> FindAllNodes(
            T start,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> predicate,
            IEqualityComparer<T> comparer = null)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = new List<T>();
            BFS(
                start,
                getNeighbors,
                node =>
                {
                    if (predicate(node))
                        result.Add(node);
                },
                comparer);
            return result;
        }

        private static void EnqueueUnvisited(
            T current,
            Func<T, IEnumerable<T>> getNeighbors,
            HashSet<T> visited,
            Queue<T> queue)
        {
            foreach (var neighbor in GetNeighbors(current, getNeighbors))
            {
                if (IsNull(neighbor) || !visited.Add(neighbor))
                    continue;

                queue.Enqueue(neighbor);
            }
        }

        private static IEnumerable<T> GetNeighbors(
            T node,
            Func<T, IEnumerable<T>> getNeighbors)
        {
            return getNeighbors(node) ?? Array.Empty<T>();
        }

        private static List<T> BuildPath(
            T start,
            T end,
            IReadOnlyDictionary<T, T> parents,
            IEqualityComparer<T> comparer)
        {
            var path = new List<T>();
            var current = end;

            while (true)
            {
                path.Add(current);
                if (comparer.Equals(current, start))
                    break;

                current = parents[current];
            }

            path.Reverse();
            return path;
        }

        private static bool IsNull(T value)
        {
            return ReferenceEquals(value, null);
        }

        private readonly struct NodeWithLevel
        {
            public NodeWithLevel(T node, int level)
            {
                Node = node;
                Level = level;
            }

            public T Node { get; }
            public int Level { get; }
        }
    }
}
