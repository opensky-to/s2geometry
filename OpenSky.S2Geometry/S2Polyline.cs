namespace OpenSky.S2Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /**
 * An S2Polyline represents a sequence of zero or more vertices connected by
 * straight edges (geodesics). Edges of length 0 and 180 degrees are not
 * allowed, i.e. adjacent vertices should not be identical or antipodal.
 *
 * <p>Note: Polylines do not have a Contains(S2Point) method, because
 * "containment" is not numerically well-defined except at the polyline
 * vertices.
 *
 */

    public struct S2Polyline : IS2Region, IEquatable<S2Polyline>
    {
        private readonly int numVertices;
        private readonly S2Point[] vertices;

        /**
   * Create a polyline that connects the given vertices. Empty polylines are
   * allowed. Adjacent vertices should not be identical or antipodal. All
   * vertices should be unit length.
   */

        public S2Polyline(IEnumerable<S2Point> vertices)
        {
            // assert isValid(vertices);
            this.vertices = vertices.ToArray();
            this.numVertices = this.vertices.Length;
        }

        /**
   * Copy constructor.
   *
   * TODO(dbeaumont): Now that S2Polyline is immutable, remove this.
   */

        public S2Polyline(S2Polyline src)
        {
            this.numVertices = src.NumVertices;
            this.vertices = (S2Point[])src.vertices.Clone();
        }

        public int NumVertices
        {
            get { return this.numVertices; }
        }

        public S1Angle ArcLengthAngle
        {
            get
            {
                double lengthSum = 0;
                for (var i = 1; i < this.NumVertices; ++i)
                {
                    lengthSum += this.Vertex(i - 1).Angle(this.Vertex(i));
                }
                return S1Angle.FromRadians(lengthSum);
            }
        }

        public bool Equals(S2Polyline other)
        {
            if (this.numVertices != other.numVertices)
            {
                return false;
            }

            for (var i = 0; i < this.vertices.Length; i++)
            {
                if (!this.vertices[i].Equals(other.vertices[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public S2Cap CapBound
        {
            get { return this.RectBound.CapBound; }
        }


        /** Return a bounding latitude-longitude rectangle. */

        public S2LatLngRect RectBound
        {
            get
            {
                var bounder = new RectBounder();
                for (var i = 0; i < this.NumVertices; ++i)
                {
                    bounder.AddPoint(this.Vertex(i));
                }
                return bounder.Bound;
            }
        }

        /**
   * If this method returns true, the region completely contains the given cell.
   * Otherwise, either the region does not contain the cell or the containment
   * relationship could not be determined.
   */

        public bool Contains(S2Cell cell)
        {
            throw new NotSupportedException(
                "'containment' is not numerically well-defined " + "except at the polyline vertices");
        }

        /**
   * If this method returns false, the region does not intersect the given cell.
   * Otherwise, either region intersects the cell, or the intersection
   * relationship could not be determined.
   */

        public bool MayIntersect(S2Cell cell)
        {
            if (this.NumVertices == 0)
            {
                return false;
            }

            // We only need to check whether the cell contains vertex 0 for correctness,
            // but these tests are cheap compared to edge crossings so we might as well
            // check all the vertices.
            for (var i = 0; i < this.NumVertices; ++i)
            {
                if (cell.Contains(this.Vertex(i)))
                {
                    return true;
                }
            }
            var cellVertices = new S2Point[4];
            for (var i = 0; i < 4; ++i)
            {
                cellVertices[i] = cell.GetVertex(i);
            }
            for (var j = 0; j < 4; ++j)
            {
                var crosser =
                    new EdgeCrosser(cellVertices[j], cellVertices[(j + 1) & 3], this.Vertex(0));
                for (var i = 1; i < this.NumVertices; ++i)
                {
                    if (crosser.RobustCrossing(this.Vertex(i)) >= 0)
                    {
                        // There is a proper crossing, or two vertices were the same.
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((S2Polyline)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                unchecked
                {
                    var code = (this.numVertices*397);
                    foreach (var v in this.vertices)
                    {
                        code ^= v.GetHashCode();
                    }

                    return code;
                }
            }
        }

        public static bool operator ==(S2Polyline left, S2Polyline right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(S2Polyline left, S2Polyline right)
        {
            return !Equals(left, right);
        }

        /**
   * Return true if the given vertices form a valid polyline.
   */

        public bool IsValidPolyline(IReadOnlyList<S2Point> vertices)
        {
            // All vertices must be unit length.
            var n = vertices.Count;
            for (var i = 0; i < n; ++i)
            {
                if (!S2.IsUnitLength(vertices[i]))
                {
                    Debug.WriteLine("Vertex " + i + " is not unit length");
                    return false;
                }
            }

            // Adjacent vertices must not be identical or antipodal.
            for (var i = 1; i < n; ++i)
            {
                if (vertices[i - 1].Equals(vertices[i])
                    || vertices[i - 1].Equals(-vertices[i]))
                {
                    Debug.WriteLine("Vertices " + (i - 1) + " and " + i + " are identical or antipodal");
                    return false;
                }
            }

            return true;
        }

        public S2Point Vertex(int k)
        {
            // assert (k >= 0 && k < numVertices);
            return this.vertices[k];
        }

        /**
   * Return the angle corresponding to the total arclength of the polyline on a
   * unit sphere.
   */

        /**
   * Return the point whose distance from vertex 0 along the polyline is the
   * given fraction of the polyline's total length. Fractions less than zero or
   * greater than one are clamped. The return value is unit length. This cost of
   * this function is currently linear in the number of vertices.
   */

        public S2Point Interpolate(double fraction)
        {
            // We intentionally let the (fraction >= 1) case fall through, since
            // we need to handle it in the loop below in any case because of
            // possible roundoff errors.
            if (fraction <= 0)
            {
                return this.Vertex(0);
            }

            double lengthSum = 0;
            for (var i = 1; i < this.NumVertices; ++i)
            {
                lengthSum += this.Vertex(i - 1).Angle(this.Vertex(i));
            }
            var target = fraction*lengthSum;
            for (var i = 1; i < this.NumVertices; ++i)
            {
                var length = this.Vertex(i - 1).Angle(this.Vertex(i));
                if (target < length)
                {
                    // This code interpolates with respect to arc length rather than
                    // straight-line distance, and produces a unit-length result.
                    var f = Math.Sin(target)/Math.Sin(length);
                    return (this.Vertex(i - 1)*(Math.Cos(target) - f*Math.Cos(length))) + (this.Vertex(i)*f);
                }
                target -= length;
            }
            return this.Vertex(this.NumVertices - 1);
        }

        // S2Region interface (see {@code S2Region} for details):

        /** Return a bounding spherical cap. */

        /**
   * Given a point, returns the index of the start point of the (first) edge on
   * the polyline that is closest to the given point. The polyline must have at
   * least one vertex. Throws IllegalStateException if this is not the case.
   */

        public int GetNearestEdgeIndex(S2Point point)
        {
            Preconditions.CheckState(this.NumVertices > 0, "Empty polyline");

            if (this.NumVertices == 1)
            {
                // If there is only one vertex, the "edge" is trivial, and it's the only one
                return 0;
            }

            // Initial value larger than any possible distance on the unit sphere.
            var minDistance = S1Angle.FromRadians(10);
            var minIndex = -1;

            // Find the line segment in the polyline that is closest to the point given.
            for (var i = 0; i < this.NumVertices - 1; ++i)
            {
                var distanceToSegment = S2EdgeUtil.GetDistance(point, this.Vertex(i), this.Vertex(i + 1));
                if (distanceToSegment < minDistance)
                {
                    minDistance = distanceToSegment;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /**
   * Given a point p and the index of the start point of an edge of this polyline,
   * returns the point on that edge that is closest to p.
   */

        public S2Point ProjectToEdge(S2Point point, int index)
        {
            Preconditions.CheckState(this.NumVertices > 0, "Empty polyline");
            Preconditions.CheckState(this.NumVertices == 1 || index < this.NumVertices - 1, "Invalid edge index");
            if (this.NumVertices == 1)
            {
                // If there is only one vertex, it is always closest to any given point.
                return this.Vertex(0);
            }
            return S2EdgeUtil.GetClosestPoint(point, this.Vertex(index), this.Vertex(index + 1));
        }
    }
}