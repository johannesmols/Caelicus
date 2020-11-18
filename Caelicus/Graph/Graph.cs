using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models.Graph;
using Priority_Queue;

namespace Caelicus.Graph
{
    /// <summary>
    /// Generic Graph implementation based on Adjacency list
    /// Source: https://github.com/leandroltavares/uGraph
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public class Graph<TVertex, TEdge> : IEnumerable<Vertex<TVertex, TEdge>>
    {
        public IReadOnlyList<Vertex<TVertex, TEdge>> Vertices => _vertices.AsReadOnly();

        public int VertexCount => _verticesSet.Count;

        public int EdgeCount { get; set; }


        private readonly List<Vertex<TVertex, TEdge>> _vertices;

        private readonly HashSet<Guid> _verticesSet;

        public Graph()
        {
            _vertices = new List<Vertex<TVertex, TEdge>>();
            _verticesSet = new HashSet<Guid>();
        }

        public void AddVertex(Vertex<TVertex, TEdge> vertex)
        {
            _vertices.Add(vertex);
            _verticesSet.Add(vertex.Id);
        }

        public Vertex<TVertex, TEdge> AddVertex(TVertex info)
        {
            var vertex = new Vertex<TVertex, TEdge> { Info = info };

            AddVertex(vertex);

            return vertex;
        }

        public Edge<TVertex, TEdge> AddEdge(Vertex<TVertex, TEdge> origin, Vertex<TVertex, TEdge> destination, TEdge info)
        {
            var edge = new Edge<TVertex, TEdge>(info)
            {
                Origin = origin,
                Destination = destination
            };

            origin.Edges.Add(edge);

            EdgeCount++;

            return edge;
        }

        public Edge<TVertex, TEdge> AddEdge<T>(T origin, T destination, TEdge info) where T : TVertex, IEquatable<TVertex>
        {
            var originVertex = CustomFirstOrDefault(v => ((IEquatable<T>)(v)).Equals(origin));

            if (originVertex == null)
                throw new ArgumentNullException(nameof(originVertex));

            var destinationVertex = CustomFirstOrDefault(v => ((IEquatable<T>)(v)).Equals(destination));

            if (destinationVertex == null)
                throw new ArgumentNullException(nameof(destinationVertex));

            return AddEdge(originVertex, destinationVertex, info);
        }

        public Vertex<TVertex, TEdge> CustomFirstOrDefault(Func<TVertex, bool> predicate)
        {
            return _vertices.FirstOrDefault(v => predicate(v.Info));
        }

        public Vertex<TVertex, TEdge> CustomFirstOrDefaultWithInfo(Func<Vertex<TVertex, TEdge>, bool> predicate)
        {
            return _vertices.FirstOrDefault(v => predicate(v));
        }

        public bool Contains<T>(T value) where T : TVertex, IEquatable<TVertex>
        {
            return _vertices.Any(v => v.Info.Equals(value));
        }

        public bool Contains(Vertex<TVertex, TEdge> vertex)
        {
            return _vertices.Any(v => v.Equals(vertex));
        }

        public IEnumerator<Vertex<TVertex, TEdge>> GetEnumerator()
        {
            return _vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _vertices.GetEnumerator();
        }

        public bool Equals(Graph<TVertex, TEdge> other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (VertexCount != other.VertexCount)
                return false;

            var thisOrderedVertices = this._vertices.GroupBy(v => v.Edges.Count());
            var otherOrderedVertices = other._vertices.GroupBy(v => v.Edges.Count());

            var otherGraphVisitedVertices = new HashSet<Vertex<TVertex, TEdge>>();

            foreach (var vertexGroupPair in thisOrderedVertices.Zip(otherOrderedVertices, (first, second) => (first, second)))
            {
                foreach (var originVertex in vertexGroupPair.first)
                {
                    var matchingDestinationVertex =
                        vertexGroupPair.second.FirstOrDefault(destinationVertex => destinationVertex.Info.Equals(originVertex.Info)
                                                              && !otherGraphVisitedVertices.Contains(destinationVertex)
                                                              && originVertex.Edges.Count() == destinationVertex.Edges.Count());

                    if (matchingDestinationVertex != null)
                    {

                        otherGraphVisitedVertices.Add(matchingDestinationVertex);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return other._vertices.Count == otherGraphVisitedVertices.Count;
        }

        #region Private Methods

        private void ClearVisitedVertices()
        {
            foreach (var vertex in _vertices)
            {
                vertex.Visited = false;
            }
        }

        #endregion

        #region Traversal

        /// <summary>
        /// Dijkstra's Algorithm to find shortest path in a directed weighted graph
        /// Implementation guide and explanation: https://www.redblobgames.com/pathfinding/a-star/introduction.html
        /// </summary>
        /// <param name="graph">Reference to the graph with non-generic objects (needed to retrieve distances from edges)</param>
        /// <param name="start">The start vertex</param>
        /// <param name="target">The target vertex</param>
        /// <returns></returns>
        public Tuple<List<Vertex<VertexInfo, EdgeInfo>>, double> FindShortestPath(Graph<VertexInfo, EdgeInfo> graph, Vertex<TVertex, TEdge> start, Vertex<TVertex, TEdge> target)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var frontier = new SimplePriorityQueue<Guid, double>();
            frontier.Enqueue(start.Id, 0d);

            var cameFrom = new Dictionary<Guid, Guid>
            {
                { start.Id, Guid.Empty }
            };

            var costSoFar = new Dictionary<Guid, double>
            {
                { start.Id, 0d }
            };

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(target.Id))
                    break;

                var enumerable = graph._vertices.Find(v => v.Id.Equals(current))?.Edges;
                if (enumerable != null)
                    foreach (var edge in enumerable)
                    {
                        var newCost = costSoFar[current] + edge.Info.Distance;
                        if (!costSoFar.ContainsKey(edge.Destination.Id) || newCost < costSoFar[edge.Destination.Id])
                        {
                            costSoFar[edge.Destination.Id] = newCost;
                            frontier.Enqueue(edge.Destination.Id, newCost);
                            cameFrom[edge.Destination.Id] = current;
                        }
                    }
            }

            // Get shortest path
            var pfCurrent = target.Id;
            var pfPath = new List<Guid>();

            while (!pfCurrent.Equals(start.Id))
            {
                pfPath.Add(pfCurrent);
                pfCurrent = cameFrom[pfCurrent];
            }
            pfPath.Add(start.Id);
            pfPath.Reverse();

            var shortestPath = new List<Vertex<VertexInfo, EdgeInfo>>();
            shortestPath.AddRange(pfPath.Select(x => graph._vertices.Find(v => v.Id.Equals(x))).ToList());

            return Tuple.Create(shortestPath, costSoFar[target.Id]);
        }

        /// <summary>
        /// Depth-first traversal
        /// <paramref name="initialVertex">Initial vertex of the traversal</paramref>
        /// <paramref name="action">Action to be executed foreach node</paramref>
        /// </summary>
        public void DepthFirstTraversal(Vertex<TVertex, TEdge> initialVertex, Action<Vertex<TVertex, TEdge>> action)
        {
            if (initialVertex == null)
                throw new ArgumentNullException(nameof(initialVertex));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var stack = new Stack<Vertex<TVertex, TEdge>>();

            ClearVisitedVertices();

            stack.Push(initialVertex);

            while (stack.Count > 0)
            {
                var currentVertex = stack.Pop();

                if (!currentVertex.Visited)
                {
                    currentVertex.Visited = true;

                    action(currentVertex);

                    foreach (var edge in currentVertex.Edges)
                    {
                        stack.Push(edge.Destination);
                    }
                }
            }
        }
        
        /// <summary>
        /// Depth-first traversal
        /// <paramref name="initialVertex">Initial vertex of the traversal</paramref>
        /// <paramref name="action">Action to be executed foreach node</paramref>
        /// </summary>
        public float DepthFirstFind(TVertex start, TVertex end)
        {
            return 100;
            // Vertex<TVertex, TEdge> initialVertex = FirstOrDefault(v => ((IEquatable<TVertex>)(v)).Equals(start));
            // Vertex<TVertex, TEdge> endVertex = FirstOrDefault(v => ((IEquatable<TVertex>)(v)).Equals(end));
            //
            // if (initialVertex == null)
            //     throw new ArgumentNullException(nameof(initialVertex));
            //
            // if (action == null)
            //     throw new ArgumentNullException(nameof(action));
            //
            // var stack = new Stack<Vertex<TVertex, TEdge>>();
            //
            // ClearVisitedVertices();
            //
            // stack.Push(initialVertex);
            //
            // while (stack.Count > 0)
            // {
            //     var currentVertex = stack.Pop();
            //     currentVertex.Info
            //     if (!currentVertex.Visited && !((IEquatable<TVertex>) currentVertex.Info).Equals(end))
            //     {
            //         currentVertex.Visited = true;
            //
            //         action(currentVertex);
            //
            //         foreach (var edge in currentVertex.Edges)
            //         {
            //             stack.Push(edge.Destination);
            //         }
            //     }
            // }
        }

        /// <summary>
        /// Breadth-first traversal
        /// <paramref name="initialVertex">Initial vertex of the traversal</paramref>
        /// <paramref name="action">Action to be executed foreach node</paramref>
        /// </summary>
        public void BreadthFirstTraversal(Vertex<TVertex, TEdge> initialVertex, Action<Vertex<TVertex, TEdge>> action)
        {
            if (initialVertex == null)
                throw new ArgumentNullException(nameof(initialVertex));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var queue = new Queue<Vertex<TVertex, TEdge>>();

            ClearVisitedVertices();

            queue.Enqueue(initialVertex);

            while (queue.Count > 0)
            {
                var currentVertex = queue.Dequeue();

                if (!currentVertex.Visited)
                {
                    currentVertex.Visited = true;

                    action(currentVertex);

                    foreach (var edge in currentVertex.Edges)
                    {
                        queue.Enqueue(edge.Destination);
                    }
                }
            }
        }

        public bool BidirectionalSearch(Vertex<TVertex, TEdge> initialVertex, Vertex<TVertex, TEdge> targetVertex)//, out List<Vertex<TVertex, TEdge>> _vertices)
        {
            if (initialVertex == null)
                throw new ArgumentNullException(nameof(initialVertex));

            if (targetVertex == null)
                throw new ArgumentNullException(nameof(targetVertex));

            var queueA = new Queue<Vertex<TVertex, TEdge>>();
            var queueB = new Queue<Vertex<TVertex, TEdge>>();
            var visitedA = new HashSet<Vertex<TVertex, TEdge>>();
            var visitedB = new HashSet<Vertex<TVertex, TEdge>>();

            visitedA.Add(initialVertex);
            visitedB.Add(targetVertex);

            queueA.Enqueue(initialVertex);
            queueB.Enqueue(targetVertex);

            while (queueA.Count > 0 || queueB.Count > 0)
            {
                if (PathExistsBidirectionalHelper(queueA, visitedA, visitedB))
                {
                    return true;
                }
                if (PathExistsBidirectionalHelper(queueB, visitedB, visitedA))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool PathExistsBidirectionalHelper(Queue<Vertex<TVertex, TEdge>> queue, HashSet<Vertex<TVertex, TEdge>> visitedFromThisSide, HashSet<Vertex<TVertex, TEdge>> visitedFromThatSide)
        {
            if (queue.Count > 0)
            {
                var next = queue.Dequeue();

                foreach (var adjacent in next.Edges.Select(s => s.Destination))
                {
                    if (visitedFromThatSide.Contains(adjacent))
                    {
                        return true;
                    }

                    if (visitedFromThisSide.Add(adjacent))
                    {
                        queue.Enqueue(adjacent);
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
