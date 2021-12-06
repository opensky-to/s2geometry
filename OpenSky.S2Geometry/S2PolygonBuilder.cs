﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Common.Geometry.DataStructures;

namespace Google.Common.Geometry
{
    /**
 * This is a simple class for assembling polygons out of edges. It requires that
 * no two edges cross. It can handle both directed and undirected edges, and
 * optionally it can also remove duplicate edge pairs (consisting of two
 * identical edges or an edge and its reverse edge). This is useful for
 * computing seamless unions of polygons that have been cut into pieces.
 *
 *  Here are some of the situations this class was designed to handle:
 *
 *  1. Computing the union of disjoint polygons that may share part of their
 * boundaries. For example, reassembling a lake that has been split into two
 * loops by a state boundary.
 *
 *  2. Constructing polygons from input data that does not follow S2
 * conventions, i.e. where loops may have repeated vertices, or distinct loops
 * may share edges, or shells and holes have opposite or unspecified
 * orientations.
 *
 *  3. Computing the symmetric difference of a set of polygons whose edges
 * intersect only at vertices. This can be used to implement a limited form of
 * polygon intersection or subtraction as well as unions.
 *
 *  4. As a tool for implementing other polygon operations by generating a
 * collection of directed edges and then assembling them into loops.
 *
 */

    public sealed class S2PolygonBuilder
    {
        /**
   * The current set of edges, grouped by origin. The set of destination
   * vertices is a multiset so that the same edge can be present more than once.
   */
        private readonly System.Collections.Generic.IDictionary<S2Point, HashBag<S2Point>> _edges;
        private readonly S2PolygonBuilderOptions _options;

        /**
   * Default constructor for well-behaved polygons. Uses the DIRECTED_XOR
   * options.
   */

        public S2PolygonBuilder() : this(S2PolygonBuilderOptions.DirectedXor)
        {
        }

        public S2PolygonBuilder(S2PolygonBuilderOptions options)
        {
            _options = options;
            _edges = new Dictionary<S2Point, HashBag<S2Point>>();
        }

        public S2PolygonBuilderOptions Options
        {
            get { return _options; }
        }

        /**
   * Add the given edge to the polygon builder. This method should be used for
   * input data that may not follow S2 polygon conventions. Note that edges are
   * not allowed to cross each other. Also note that as a convenience, edges
   * where v0 == v1 are ignored.
   */

        public void AddEdge(S2Point v0, S2Point v1)
        {
            // If xor_edges is true, we look for an existing edge in the opposite
            // direction. We either delete that edge or insert a new one.

            if (v0.Equals(v1))
            {
                return;
            }

            if (_options.XorEdges)
            {
                HashBag<S2Point> candidates;
                _edges.TryGetValue(v1, out candidates);
                if (candidates != null && candidates.Any(c => c.Equals(v0)))
                {
                    EraseEdge(v1, v0);
                    return;
                }
            }

            if (!_edges.ContainsKey(v0))
            {
                _edges[v0] = new HashBag<S2Point>();
            }

            _edges[v0].Add(v1);
            if (_options.UndirectedEdges)
            {
                if (!_edges.ContainsKey(v1))
                {
                    _edges[v1] = new HashBag<S2Point>();
                }
                _edges[v1].Add(v0);
            }
        }

        /**
   * Add all edges in the given loop. If the sign() of the loop is negative
   * (i.e. this loop represents a hole), the reverse edges are added instead.
   * This implies that "shells" are CCW and "holes" are CW, as required for the
   * directed edges convention described above.
   *
   * This method does not take ownership of the loop.
   */

        public void AddLoop(S2Loop loop)
        {
            var sign = loop.Sign;
            for (var i = loop.NumVertices; i > 0; --i)
            {
                // Vertex indices need to be in the range [0, 2*num_vertices()-1].
                AddEdge(loop.Vertex(i), loop.Vertex(i + sign));
            }
        }

        /**
   * Add all loops in the given polygon. Shells and holes are added with
   * opposite orientations as described for AddLoop(). This method does not take
   * ownership of the polygon.
   */

        public void AddPolygon(S2Polygon polygon)
        {
            for (var i = 0; i < polygon.NumLoops; ++i)
            {
                AddLoop(polygon.Loop(i));
            }
        }

        /**
   * Assembles the given edges into as many non-crossing loops as possible. When
   * there is a choice about how to assemble the loops, then CCW loops are
   * preferred. Returns true if all edges were assembled. If "unused_edges" is
   * not NULL, it is initialized to the set of edges that could not be assembled
   * into loops.
   *
   *  Note that if xor_edges() is false and duplicate edge pairs may be present,
   * then undirected_edges() should be specified unless all loops can be
   * assembled in a counter-clockwise direction. Otherwise this method may not
   * be able to assemble all loops due to its preference for CCW loops.
   *
   * This method resets the S2PolygonBuilder state so that it can be reused.
   */

        public bool AssembleLoops(System.Collections.Generic.IList<S2Loop> loops, System.Collections.Generic.IList<S2Edge> unusedEdges)
        {
            if (_options.MergeDistance.Radians > 0)
            {
                MergeVertices();
            }

            var dummyUnusedEdges = new List<S2Edge>();
            if (unusedEdges == null)
            {
                unusedEdges = dummyUnusedEdges;
            }

            // We repeatedly choose an arbitrary edge and attempt to assemble a loop
            // starting from that edge. (This is always possible unless the input
            // includes extra edges that are not part of any loop.)

            unusedEdges.Clear();
            while (_edges.Any())
            {
                //Map.Entry<S2Point, Multiset<S2Point>> edge = edges.entrySet().iterator().next();
                var edge = _edges.First();

                var v0 = edge.Key;
                var v1 = edge.Value.First();

                var loop = AssembleLoop(v0, v1, unusedEdges);
                if (loop == null)
                {
                    continue;
                }

                // In the case of undirected edges, we may have assembled a clockwise
                // loop while trying to assemble a CCW loop. To fix this, we assemble
                // a new loop starting with an arbitrary edge in the reverse direction.
                // This is guaranteed to assemble a loop that is interior to the previous
                // one and will therefore eventually terminate.

                while (_options.UndirectedEdges && !loop.IsNormalized)
                {
                    loop = AssembleLoop(loop.Vertex(1), loop.Vertex(0), unusedEdges);
                }
                loops.Add(loop);
                EraseLoop(loop, loop.NumVertices);
            }
            return unusedEdges.Count == 0;
        }

        /**
   * Like AssembleLoops, but normalizes all the loops so that they enclose less
   * than half the sphere, and then assembles the loops into a polygon.
   *
   *  For this method to succeed, there should be no duplicate edges in the
   * input. If this is not known to be true, then the "xor_edges" option should
   * be set (which is true by default).
   *
   *  Note that S2Polygons cannot represent arbitrary regions on the sphere,
   * because of the limitation that no loop encloses more than half of the
   * sphere. For example, an S2Polygon cannot represent a 100km wide band around
   * the equator. In such cases, this method will return the *complement* of the
   * expected region. So for example if all the world's coastlines were
   * assembled, the output S2Polygon would represent the land area (irrespective
   * of the input edge or loop orientations).
   */

        public bool AssemblePolygon(S2Polygon polygon, System.Collections.Generic.IList<S2Edge> unusedEdges)
        {
            var loops = new List<S2Loop>();
            var success = AssembleLoops(loops, unusedEdges);

            // If edges are undirected, then all loops are already CCW. Otherwise we
            // need to make sure the loops are normalized.
            if (!_options.UndirectedEdges)
            {
                for (var i = 0; i < loops.Count; ++i)
                {
                    loops[i].Normalize();
                }
            }
            if (_options.Validate && !S2Polygon.IsValidPolygon(loops))
            {
                if (unusedEdges != null)
                {
                    foreach (var loop in loops)
                    {
                        RejectLoop(loop, loop.NumVertices, unusedEdges);
                    }
                }
                return false;
            }
            polygon.Init(loops);
            return success;
        }

        /**
   * Convenience method for when you don't care about unused edges.
   */

        public S2Polygon AssemblePolygon()
        {
            var polygon = new S2Polygon();
            var unusedEdges = new List<S2Edge>();

            AssemblePolygon(polygon, unusedEdges);

            return polygon;
        }

        // Debugging functions:

        private void DumpEdges(S2Point v0)
        {
            Debug.WriteLine(v0.ToString());
            var vset = _edges[v0];
            if (vset != null)
            {
                foreach (var v in vset)
                {
                    Debug.WriteLine("    " + v.ToString());
                }
            }
        }

        private void Dump()
        {
            foreach (var v in _edges.Keys)
            {
                DumpEdges(v);
            }
        }

        private void EraseEdge(S2Point v0, S2Point v1)
        {
            // Note that there may be more than one copy of an edge if we are not XORing
            // them, so a VertexSet is a multiset.

            var vset = _edges[v0];
            // assert (vset.count(v1) > 0);
            vset.Remove(v1);
            if (vset.Count == 0)
            {
                _edges.Remove(v0);
            }

            if (_options.UndirectedEdges)
            {
                vset = _edges[v1];
                // assert (vset.count(v0) > 0);
                vset.Remove(v0);
                if (vset.Count == 0)
                {
                    _edges.Remove(v1);
                }
            }
        }

        private void EraseLoop(System.Collections.Generic.IList<S2Point> v, int n)
        {
            for (int i = n - 1, j = 0; j < n; i = j++)
            {
                EraseEdge(v[i], v[j]);
            }
        }

        private void EraseLoop(S2Loop v, int n)
        {
            for (int i = n - 1, j = 0; j < n; i = j++)
            {
                EraseEdge(v.Vertex(i), v.Vertex(j));
            }
        }

        /**
   * We start at the given edge and assemble a loop taking left turns whenever
   * possible. We stop the loop as soon as we encounter any vertex that we have
   * seen before *except* for the first vertex (v0). This ensures that only CCW
   * loops are constructed when possible.
   */

        private S2Loop AssembleLoop(S2Point v0, S2Point v1, System.Collections.Generic.IList<S2Edge> unusedEdges)
        {
            // The path so far.
            var path = new List<S2Point>();

            // Maps a vertex to its index in "path".
            var index = new Dictionary<S2Point, int>();
            path.Add(v0);
            path.Add(v1);

            index.Add(v1, 1);

            while (path.Count >= 2)
            {
                // Note that "v0" and "v1" become invalid if "path" is modified.
                v0 = path[path.Count - 2];
                v1 = path[path.Count - 1];

                var v2 = default (S2Point);
                var v2Found = false;
                HashBag<S2Point> vset;
                _edges.TryGetValue(v1, out vset);
                if (vset != null)
                {
                    foreach (var v in vset)
                    {
                        // We prefer the leftmost outgoing edge, ignoring any reverse edges.
                        if (v.Equals(v0))
                        {
                            continue;
                        }
                        if (!v2Found || S2.OrderedCcw(v0, v2, v, v1))
                        {
                            v2 = v;
                        }
                        v2Found = true;
                    }
                }
                if (!v2Found)
                {
                    // We've hit a dead end. Remove this edge and backtrack.
                    unusedEdges.Add(new S2Edge(v0, v1));
                    EraseEdge(v0, v1);
                    index.Remove(v1);
                    path.RemoveAt(path.Count - 1);
                }
                else if (!index.ContainsKey(v2))
                {
                    // This is the first time we've visited this vertex.
                    index.Add(v2, path.Count);
                    path.Add(v2);
                }
                else
                {
                    // We've completed a loop. Throw away any initial vertices that
                    // are not part of the loop.
                    var start = index[v2];
                    path = path.GetRange(start, path.Count - start);

                    if (_options.Validate && !S2Loop.IsValidLoop(path))
                    {
                        // We've constructed a loop that crosses itself, which can only happen
                        // if there is bad input data. Throw away the whole loop.
                        RejectLoop(path, path.Count, unusedEdges);
                        EraseLoop(path, path.Count);
                        return null;
                    }
                    return new S2Loop(path);
                }
            }
            return null;
        }

        /** Erases all edges of the given loop and marks them as unused. */

        private void RejectLoop(S2Loop v, int n, System.Collections.Generic.IList<S2Edge> unusedEdges)
        {
            for (int i = n - 1, j = 0; j < n; i = j++)
            {
                unusedEdges.Add(new S2Edge(v.Vertex(i), v.Vertex(j)));
            }
        }

        /** Erases all edges of the given loop and marks them as unused. */

        private void RejectLoop(System.Collections.Generic.IList<S2Point> v, int n, System.Collections.Generic.IList<S2Edge> unusedEdges)
        {
            for (int i = n - 1, j = 0; j < n; i = j++)
            {
                unusedEdges.Add(new S2Edge(v[i], v[j]));
            }
        }

        /** Moves a set of vertices from old to new positions. */

        private void MoveVertices(System.Collections.Generic.IDictionary<S2Point, S2Point> mergeMap)
        {
            if (mergeMap.Count == 0)
            {
                return;
            }

            // We need to copy the set of edges affected by the move, since
            // this.edges_could be reallocated when we start modifying it.
            var edgesCopy = new List<S2Edge>();
            foreach (var edge in _edges)
            {
                var v0 = edge.Key;
                var vset = edge.Value;
                foreach (var v1 in vset)
                {
                    if (mergeMap.ContainsKey(v0) || mergeMap.ContainsKey(v1))
                    {
                        // We only need to modify one copy of each undirected edge.
                        if (!_options.UndirectedEdges || v0 < v1)
                        {
                            edgesCopy.Add(new S2Edge(v0, v1));
                        }
                    }
                }
            }

            // Now erase all the old edges, and add all the new edges. This will
            // automatically take care of any XORing that needs to be done, because
            // EraseEdge also erases the sibiling of undirected edges.
            for (var i = 0; i < edgesCopy.Count; ++i)
            {
                var v0 = edgesCopy[i].Start;
                var v1 = edgesCopy[i].End;
                EraseEdge(v0, v1);
                if (mergeMap.ContainsKey(v0))
                {
                    v0 = mergeMap[v0];
                }
                if (mergeMap.ContainsKey(v1))
                {
                    v1 = mergeMap[v1];
                }
                AddEdge(v0, v1);
            }
        }

        /**
   * Look for groups of vertices that are separated by at most merge_distance()
   * and merge them into a single vertex.
   */

        private void MergeVertices()
        {
            // The overall strategy is to start from each vertex and grow a maximal
            // cluster of mergable vertices. In graph theoretic terms, we find the
            // connected components of the undirected graph whose edges connect pairs of
            // vertices that are separated by at most merge_distance.
            //
            // We then choose a single representative vertex for each cluster, and
            // update all the edges appropriately. We choose an arbitrary existing
            // vertex rather than computing the centroid of all the vertices to avoid
            // creating new vertex pairs that need to be merged. (We guarantee that all
            // vertex pairs are separated by at least merge_distance in the output.)

            var index = new PointIndex(_options.MergeDistance.Radians);

            foreach (var edge in _edges)
            {
                index.Add(edge.Key);
                var vset = edge.Value;
                foreach (var v in vset)
                {
                    index.Add(v);
                }
            }

            // Next, we loop through all the vertices and attempt to grow a maximial
            // mergeable group starting from each vertex.

            var mergeMap = new Dictionary<S2Point, S2Point>();
            var frontier = new Stack<S2Point>();
            var mergeable = new List<S2Point>();

            foreach (var entry in index)
            {
                var point = entry.Value;
                if (point.IsMarked)
                {
                    continue; // Already processed.
                }

                point.Mark();

                // Grow a maximal mergeable component starting from "vstart", the
                // canonical representative of the mergeable group.
                var vstart = point.Point;
                frontier.Push(vstart);
                while (frontier.Any())
                {
                    var v0 = frontier.Pop();

                    index.Query(v0, mergeable);
                    foreach (var v1 in mergeable)
                    {
                        frontier.Push(v1);
                        mergeMap.Add(v1, vstart);
                    }
                }
            }

            // Finally, we need to replace vertices according to the merge_map.
            MoveVertices(mergeMap);
        }

        /**
   * A PointIndex is a cheap spatial index to help us find mergeable vertices.
   * Given a set of points, it can efficiently find all of the points within a
   * given search radius of an arbitrary query location. It is essentially just
   * a hash map from cell ids at a given fixed level to the set of points
   * contained by that cell id.
   *
   *  This class is not suitable for general use because it only supports
   * fixed-radius queries and has various special-purpose operations to avoid
   * the need for additional data structures.
   */

        /**
   * An S2Point that can be marked. Used in PointIndex.
   */

        private sealed class MarkedS2Point
        {
            private readonly S2Point _point;
            private bool _mark;

            public MarkedS2Point(S2Point point)
            {
                _point = point;
                _mark = false;
            }

            public bool IsMarked
            {
                get { return _mark; }
            }

            public S2Point Point
            {
                get { return _point; }
            }

            public void Mark()
            {
                // assert (!isMarked());
                _mark = true;
            }
        }

        private sealed class PointIndex : IEnumerable<System.Collections.Generic.KeyValuePair<S2CellId, MarkedS2Point>>
        {
            // : ForwardingMultimap<S2CellId, MarkedS2Point> {
            private readonly MultiMap<S2CellId, MarkedS2Point> _delegate = new MultiMap<S2CellId, MarkedS2Point>();
            private readonly int _level;
            private readonly double _searchRadius;

            public PointIndex(double searchRadius)
            {
                _searchRadius = searchRadius;

                // We choose a cell level such that if dist(A,B) <= search_radius, the
                // S2CellId at that level containing A is a vertex neighbor of B (see
                // S2CellId.getVertexNeighbors). This turns out to be the highest
                // level such that a spherical cap (i.e. "disc") of the given radius
                // fits completely inside all cells at that level.
                _level =
                    Math.Min(S2Projections.MinWidth.GetMaxLevel(2*searchRadius), S2CellId.MaxLevel - 1);
            }


            //protected Multimap<S2CellId, MarkedS2Point> _delegate() {
            //  return _delegate;
            //}

            /** Add a point to the index if it does not already exist. */

            public IEnumerator<System.Collections.Generic.KeyValuePair<S2CellId, MarkedS2Point>> GetEnumerator()
            {
                return _delegate.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(S2Point p)
            {
                var id = S2CellId.FromPoint(p).ParentForLevel(_level);
                var pointSet = _delegate[id];
                foreach (var point in pointSet)
                {
                    if (point.Point.Equals(p))
                    {
                        return;
                    }
                }
                _delegate.Add(id, new MarkedS2Point(p));
            }

            /**
     * Return the set the unmarked points whose distance to "center" is less
     * than search_radius_, and mark these points. By construction, these points
     * will be contained by one of the vertex neighbors of "center".
     */

            public void Query(S2Point center, System.Collections.Generic.ICollection<S2Point> output)
            {
                output.Clear();

                var neighbors = new List<S2CellId>();
                S2CellId.FromPoint(center).GetVertexNeighbors(_level, neighbors);
                foreach (var id in neighbors)
                {
                    // Iterate over the points contained by each vertex neighbor.
                    foreach (var mp in _delegate[id])
                    {
                        if (mp.IsMarked)
                        {
                            continue;
                        }
                        var p = mp.Point;

                        if (center.Angle(p) <= _searchRadius)
                        {
                            output.Add(p);
                            mp.Mark();
                        }
                    }
                }
            }
        }
    }

    public sealed class S2PolygonBuilderOptions
    {
        /**
     * These are the options that should be used for assembling well-behaved
     * input data into polygons. All edges should be directed such that "shells"
     * and "holes" have opposite orientations (typically CCW shells and
     * clockwise holes), unless it is known that shells and holes do not share
     * any edges.
     */
        public static readonly S2PolygonBuilderOptions DirectedXor = new S2PolygonBuilderOptions(false, true);

        /**
     * These are the options that should be used for assembling polygons that do
     * not follow the conventions above, e.g. where edge directions may vary
     * within a single loop, or shells and holes are not oppositely oriented.
     */
        public static readonly S2PolygonBuilderOptions UndirectedXor = new S2PolygonBuilderOptions(true, true);

        /**
     * These are the options that should be used for assembling edges where the
     * desired output is a collection of loops rather than a polygon, and edges
     * may occur more than once. Edges are treated as undirected and are not
     * XORed together, in particular, adding edge A->B also adds B->A.
     */
        public static readonly S2PolygonBuilderOptions UndirectedUnion = new S2PolygonBuilderOptions(true, false);

        /**
     * Finally, select this option when the desired output is a collection of
     * loops rather than a polygon, but your input edges are directed and you do
     * not want reverse edges to be added implicitly as above.
     */
        public static readonly S2PolygonBuilderOptions DirectedUnion = new S2PolygonBuilderOptions(false, false);
        private S1Angle _mergeDistance;

        private S2PolygonBuilderOptions(bool undirectedEdges, bool xorEdges)
        {
            UndirectedEdges = undirectedEdges;
            XorEdges = xorEdges;
            Validate = false;
            _mergeDistance = S1Angle.FromRadians(0);
        }

        /**
     * If "undirected_edges" is false, then the input is assumed to consist of
     * edges that can be assembled into oriented loops without reversing any of
     * the edges. Otherwise, "undirected_edges" should be set to true.
     */

        internal bool UndirectedEdges { get; set; }

        /**
     * If "xor_edges" is true, then any duplicate edge pairs are removed. This
     * is useful for computing the union of a collection of polygons whose
     * interiors are disjoint but whose boundaries may share some common edges
     * (e.g. computing the union of South Africa, Lesotho, and Swaziland).
     *
     *  Note that for directed edges, a "duplicate edge pair" consists of an
     * edge and its corresponding reverse edge. This means that either (a)
     * "shells" and "holes" must have opposite orientations, or (b) shells and
     * holes do not share edges. Otherwise undirected_edges() should be
     * specified.
     *
     *  There are only two reasons to turn off xor_edges():
     *
     *  (1) assemblePolygon() will be called, and you want to assert that there
     * are no duplicate edge pairs in the input.
     *
     *  (2) assembleLoops() will be called, and you want to keep abutting loops
     * separate in the output rather than merging their regions together (e.g.
     * assembling loops for Kansas City, KS and Kansas City, MO simultaneously).
     */

        public bool XorEdges { get; internal set; }


        /**
     * If true, isValid() is called on all loops and polygons before
     * constructing them. If any loop is invalid (e.g. self-intersecting), it is
     * rejected and returned as a set of "unused edges". Any remaining valid
     * loops are kept. If the entire polygon is invalid (e.g. two loops
     * intersect), then all loops are rejected and returned as unused edges.
     */

        public bool Validate { get; set; }

        /**
     * If set to a positive value, all vertices that are separated by at most
     * this distance will be merged together. In addition, vertices that are
     * closer than this distance to a non-incident edge will be spliced into it
     * (TODO).
     *
     *  The merging is done in such a way that all vertex-vertex and vertex-edge
     * distances in the output are greater than 'merge_distance'.
     *
     *  This method is useful for assembling polygons out of input data where
     * vertices and/or edges may not be perfectly aligned.
     */

        public S1Angle MergeDistance
        {
            set { _mergeDistance = value; }
            get { return _mergeDistance; }
        }
    }
}