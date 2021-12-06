namespace OpenSky.S2Geometry
{
    using System;

    /// <summary>
    ///     An R1Interval represents a closed, bounded interval on the real line. It is
    ///     capable of representing the empty interval (containing no points) and
    ///     zero-length intervals (containing a single point).
    /// </summary>
    public struct R1Interval : IEquatable<R1Interval>
    {
        /// <summary>
        ///     Returns an empty interval. (Any interval where lo > hi is considered empty.)
        /// </summary>
        public static R1Interval Empty = new R1Interval(1, 0);

        private readonly double hi;
        private readonly double lo;

        public R1Interval(double lo, double hi)
        {
            this.lo = lo;
            this.hi = hi;
        }

        public double Lo
        {
            get { return this.lo; }
        }

        public double Hi
        {
            get { return this.hi; }
        }

        public double Center
        {
            get { return 0.5*(this.Lo + this.Hi); }
        }

        /**
   * Return the length of the interval. The length of an empty interval is
   * negative.
   */

        public double Length
        {
            get { return this.Hi - this.Lo; }
        }

        public bool IsEmpty
        {
            get { return this.Lo > this.Hi; }
        }

        public bool Equals(R1Interval other)
        {
            return (this.hi.Equals(other.hi) && this.lo.Equals(other.lo)) || (this.IsEmpty && other.IsEmpty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is R1Interval && this.Equals((R1Interval)obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return (this.hi.GetHashCode()*397) ^ this.lo.GetHashCode();
            }
        }

        public static bool operator ==(R1Interval left, R1Interval right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(R1Interval left, R1Interval right)
        {
            return !Equals(left, right);
        }


        /**
   * Convenience method to construct an interval containing a single point.
   */

        public static R1Interval FromPoint(double p)
        {
            return new R1Interval(p, p);
        }

        /**
   * Convenience method to construct the minimal interval containing the two
   * given points. This is equivalent to starting with an empty interval and
   * calling AddPoint() twice, but it is more efficient.
   */

        public static R1Interval FromPointPair(double p1, double p2)
        {
            if (p1 <= p2)
            {
                return new R1Interval(p1, p2);
            }
            else
            {
                return new R1Interval(p2, p1);
            }
        }

        /**
   * Return true if the interval is empty, i.e. it contains no points.
   */

        /**
   * Return the center of the interval. For empty intervals, the result is
   * arbitrary.
   */

        public bool Contains(double p)
        {
            return p >= this.Lo && p <= this.Hi;
        }

        public bool InteriorContains(double p)
        {
            return p > this.Lo && p < this.Hi;
        }

        /** Return true if this interval contains the interval 'y'. */

        public bool Contains(R1Interval y)
        {
            if (y.IsEmpty)
            {
                return true;
            }
            return y.Lo >= this.Lo && y.Hi <= this.Hi;
        }

        /**
   * Return true if the interior of this interval contains the entire interval
   * 'y' (including its boundary).
   */

        public bool InteriorContains(R1Interval y)
        {
            if (y.IsEmpty)
            {
                return true;
            }
            return y.Lo > this.Lo && y.Hi < this.Hi;
        }

        /**
   * Return true if this interval intersects the given interval, i.e. if they
   * have any points in common.
   */

        public bool Intersects(R1Interval y)
        {
            if (this.Lo <= y.Lo)
            {
                return y.Lo <= this.Hi && y.Lo <= y.Hi;
            }
            else
            {
                return this.Lo <= y.Hi && this.Lo <= this.Hi;
            }
        }

        /**
   * Return true if the interior of this interval intersects any point of the
   * given interval (including its boundary).
   */

        public bool InteriorIntersects(R1Interval y)
        {
            return y.Lo < this.Hi && this.Lo < y.Hi && this.Lo < this.Hi && y.Lo <= y.Hi;
        }

        /** Expand the interval so that it contains the given point "p". */

        public R1Interval AddPoint(double p)
        {
            if (this.IsEmpty)
            {
                return FromPoint(p);
            }
            else if (p < this.Lo)
            {
                return new R1Interval(p, this.Hi);
            }
            else if (p > this.Hi)
            {
                return new R1Interval(this.Lo, p);
            }
            else
            {
                return new R1Interval(this.Lo, this.Hi);
            }
        }

        /**
   * Return an interval that contains all points with a distance "radius" of a
   * point in this interval. Note that the expansion of an empty interval is
   * always empty.
   */

        public R1Interval Expanded(double radius)
        {
            // assert (radius >= 0);
            if (this.IsEmpty)
            {
                return this;
            }
            return new R1Interval(this.Lo - radius, this.Hi + radius);
        }

        /**
   * Return the smallest interval that contains this interval and the given
   * interval "y".
   */

        public R1Interval Union(R1Interval y)
        {
            if (this.IsEmpty)
            {
                return y;
            }
            if (y.IsEmpty)
            {
                return this;
            }
            return new R1Interval(Math.Min(this.Lo, y.Lo), Math.Max(this.Hi, y.Hi));
        }

        /**
   * Return the intersection of this interval with the given interval. Empty
   * intervals do not need to be special-cased.
   */

        public R1Interval Intersection(R1Interval y)
        {
            return new R1Interval(Math.Max(this.Lo, y.Lo), Math.Min(this.Hi, y.Hi));
        }

        public bool ApproxEquals(R1Interval y)
        {
            return this.ApproxEquals(y, 1e-15);
        }

        /**
   * Return true if length of the symmetric difference between the two intervals
   * is at most the given tolerance.
   *
   */

        public bool ApproxEquals(R1Interval y, double maxError)
        {
            if (this.IsEmpty)
            {
                return y.Length <= maxError;
            }
            if (y.IsEmpty)
            {
                return this.Length <= maxError;
            }
            return Math.Abs(y.Lo - this.Lo) + Math.Abs(y.Hi - this.Hi) <= maxError;
        }

        public override string ToString()
        {
            return "[" + this.Lo + ", " + this.Hi + "]";
        }
    }
}