﻿namespace OpenSky.S2Geometry
{
    using System;
    using System.Diagnostics;

    public struct S1Interval : IEquatable<S1Interval>
    {
        public static readonly S1Interval Empty = new S1Interval(S2.Pi, -S2.Pi, true);

        public static readonly S1Interval Full = new S1Interval(-S2.Pi, S2.Pi, true);
        private readonly double hi;
        private readonly double lo;

        public S1Interval(double lo, double hi) : this(lo, hi, false)
        {
        }

        /**
   * Internal constructor that assumes that both arguments are in the correct
   * range, i.e. normalization from -Pi to Pi is already done.
   */

        private S1Interval(double lo, double hi, bool @checked)
        {
            var newLo = lo;
            var newHi = hi;
            if (!@checked)
            {
                if (lo == -S2.Pi && hi != S2.Pi)
                {
                    newLo = S2.Pi;
                }
                if (hi == -S2.Pi && lo != S2.Pi)
                {
                    newHi = S2.Pi;
                }
            }
            this.lo = newLo;
            this.hi = newHi;
        }

        public double Lo
        {
            get { return this.lo; }
        }

        public double Hi
        {
            get { return this.hi; }
        }

        /**
   * An interval is valid if neither bound exceeds Pi in absolute value, and the
   * value -Pi appears only in the Empty() and Full() intervals.
   */

        public bool IsValid
        {
            get
            {
                return (Math.Abs(this.Lo) <= S2.Pi && Math.Abs(this.Hi) <= S2.Pi
                                                   && !(this.Lo == -S2.Pi && this.Hi != S2.Pi) && !(this.Hi == -S2.Pi && this.Lo != S2.Pi));
            }
        }

        /** Return true if the interval contains all points on the unit circle. */

        public bool IsFull
        {
            get { return this.Hi - this.Lo == 2*S2.Pi; }
        }


        /** Return true if the interval is empty, i.e. it contains no points. */

        public bool IsEmpty
        {
            get { return this.Lo - this.Hi == 2*S2.Pi; }
        }


        /* Return true if lo() > hi(). (This is true for empty intervals.) */

        public bool IsInverted
        {
            get { return this.Lo > this.Hi; }
        }

        /**
   * Return the midpoint of the interval. For full and empty intervals, the
   * result is arbitrary.
   */

        public double Center
        {
            get
            {
                var center = 0.5*(this.Lo + this.Hi);
                if (!this.IsInverted)
                {
                    return center;
                }
                // Return the center in the range (-Pi, Pi].
                return (center <= 0) ? (center + S2.Pi) : (center - S2.Pi);
            }
        }

        /**
   * Return the length of the interval. The length of an empty interval is
   * negative.
   */

        public double Length
        {
            get
            {
                var length = this.Hi - this.Lo;
                if (length >= 0)
                {
                    return length;
                }
                length += 2*S2.Pi;
                // Empty intervals have a negative length.
                return (length > 0) ? length : -1;
            }
        }

        /**
   * Return the complement of the interior of the interval. An interval and its
   * complement have the same boundary but do not share any interior values. The
   * complement operator is not a bijection, since the complement of a singleton
   * interval (containing a single value) is the same as the complement of an
   * empty interval.
   */

        public S1Interval Complement
        {
            get
            {
                if (this.Lo == this.Hi)
                {
                    return Full; // Singleton.
                }
                return new S1Interval(this.Hi, this.Lo, true); // Handles
                // empty and
                // full.
            }
        }

        public bool Equals(S1Interval other)
        {
            return this.lo.Equals(other.lo) && this.hi.Equals(other.hi);
        }

        public override bool Equals(object obj)
        {
            return obj is S1Interval && this.Equals((S1Interval)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.lo.GetHashCode()*397) ^ this.hi.GetHashCode();
            }
        }

        public static bool operator ==(S1Interval left, S1Interval right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(S1Interval left, S1Interval right)
        {
            return !Equals(left, right);
        }

        public static S1Interval FromPoint(double p)
        {
            if (p == -S2.Pi)
            {
                p = S2.Pi;
            }
            return new S1Interval(p, p, true);
        }

        /**
   * Convenience method to construct the minimal interval containing the two
   * given points. This is equivalent to starting with an empty interval and
   * calling AddPoint() twice, but it is more efficient.
   */

        public static S1Interval FromPointPair(double p1, double p2)
        {
            // assert (Math.Abs(p1) <= S2.M_PI && Math.Abs(p2) <= S2.M_PI);
            if (p1 == -S2.Pi)
            {
                p1 = S2.Pi;
            }
            if (p2 == -S2.Pi)
            {
                p2 = S2.Pi;
            }
            if (PositiveDistance(p1, p2) <= S2.Pi)
            {
                return new S1Interval(p1, p2, true);
            }
            else
            {
                return new S1Interval(p2, p1, true);
            }
        }

        /** Return true if the interval (which is closed) contains the point 'p'. */

        public bool Contains(double p)
        {
            // Works for empty, full, and singleton intervals.
            // assert (Math.Abs(p) <= S2.M_PI);
            if (p == -S2.Pi)
            {
                p = S2.Pi;
            }
            return this.FastContains(p);
        }

        /**
   * Return true if the interval (which is closed) contains the point 'p'. Skips
   * the normalization of 'p' from -Pi to Pi.
   *
   */

        public bool FastContains(double p)
        {
            if (this.IsInverted)
            {
                return (p >= this.Lo || p <= this.Hi) && !this.IsEmpty;
            }
            else
            {
                return p >= this.Lo && p <= this.Hi;
            }
        }

        /** Return true if the interior of the interval contains the point 'p'. */

        public bool InteriorContains(double p)
        {
            // Works for empty, full, and singleton intervals.
            // assert (Math.Abs(p) <= S2.M_PI);
            if (p == -S2.Pi)
            {
                p = S2.Pi;
            }

            if (this.IsInverted)
            {
                return p > this.Lo || p < this.Hi;
            }
            else
            {
                return (p > this.Lo && p < this.Hi) || this.IsFull;
            }
        }

        /**
   * Return true if the interval contains the given interval 'y'. Works for
   * empty, full, and singleton intervals.
   */

        public bool Contains(S1Interval y)
        {
            // It might be helpful to compare the structure of these tests to
            // the simpler Contains(double) method above.

            if (this.IsInverted)
            {
                if (y.IsInverted)
                {
                    return y.Lo >= this.Lo && y.Hi <= this.Hi;
                }
                return (y.Lo >= this.Lo || y.Hi <= this.Hi) && !this.IsEmpty;
            }
            else
            {
                if (y.IsInverted)
                {
                    return this.IsFull || y.IsEmpty;
                }
                return y.Lo >= this.Lo && y.Hi <= this.Hi;
            }
        }

        /**
   * Returns true if the interior of this interval contains the entire interval
   * 'y'. Note that x.InteriorContains(x) is true only when x is the empty or
   * full interval, and x.InteriorContains(S1Interval(p,p)) is equivalent to
   * x.InteriorContains(p).
   */

        public bool InteriorContains(S1Interval y)
        {
            if (this.IsInverted)
            {
                if (!y.IsInverted)
                {
                    return y.Lo > this.Lo || y.Hi < this.Hi;
                }
                return (y.Lo > this.Lo && y.Hi < this.Hi) || y.IsEmpty;
            }
            else
            {
                if (y.IsInverted)
                {
                    return this.IsFull || y.IsEmpty;
                }
                return (y.Lo > this.Lo && y.Hi < this.Hi) || this.IsFull;
            }
        }

        /**
   * Return true if the two intervals contain any points in common. Note that
   * the point +/-Pi has two representations, so the intervals [-Pi,-3] and
   * [2,Pi] intersect, for example.
   */

        public bool Intersects(S1Interval y)
        {
            if (this.IsEmpty || y.IsEmpty)
            {
                return false;
            }
            if (this.IsInverted)
            {
                // Every non-empty inverted interval contains Pi.
                return y.IsInverted || y.Lo <= this.Hi || y.Hi >= this.Lo;
            }
            else
            {
                if (y.IsInverted)
                {
                    return y.Lo <= this.Hi || y.Hi >= this.Lo;
                }
                return y.Lo <= this.Hi && y.Hi >= this.Lo;
            }
        }

        /**
   * Return true if the interior of this interval contains any point of the
   * interval 'y' (including its boundary). Works for empty, full, and singleton
   * intervals.
   */

        public bool InteriorIntersects(S1Interval y)
        {
            if (this.IsEmpty || y.IsEmpty || this.Lo == this.Hi)
            {
                return false;
            }
            if (this.IsInverted)
            {
                return y.IsInverted || y.Lo < this.Hi || y.Hi > this.Lo;
            }
            else
            {
                if (y.IsInverted)
                {
                    return y.Lo < this.Hi || y.Hi > this.Lo;
                }
                return (y.Lo < this.Hi && y.Hi > this.Lo) || this.IsFull;
            }
        }

        /**
   * Expand the interval by the minimum amount necessary so that it contains the
   * given point "p" (an angle in the range [-Pi, Pi]).
   */

        public S1Interval AddPoint(double p)
        {
            Debug.Assert(Math.Abs(p) <= S2.Pi);

            if (p == -S2.Pi)
            {
                p = S2.Pi;
            }

            if (this.FastContains(p))
            {
                return this;
            }

            if (this.IsEmpty)
            {
                return FromPoint(p);
            }
            else
            {
                // Compute distance from p to each endpoint.
                var dlo = PositiveDistance(p, this.Lo);
                var dhi = PositiveDistance(this.Hi, p);
                if (dlo < dhi)
                {
                    return new S1Interval(p, this.Hi);
                }
                else
                {
                    return new S1Interval(this.Lo, p);
                }
                // Adding a point can never turn a non-full interval into a full one.
            }
        }

        /**
   * Return an interval that contains all points within a distance "radius" of
   * a point in this interval. Note that the expansion of an empty interval is
   * always empty. The radius must be non-negative.
   */

        public S1Interval Expanded(double radius)
        {
            // assert (radius >= 0);
            if (this.IsEmpty)
            {
                return this;
            }

            // Check whether this interval will be full after expansion, allowing
            // for a 1-bit rounding error when computing each endpoint.
            if (this.Length + 2*radius >= 2*S2.Pi - 1e-15)
            {
                return Full;
            }

            // NOTE(dbeaumont): Should this remainder be 2 * M_PI or just M_PI ??
            var lo = Math.IEEERemainder(this.Lo - radius, 2*S2.Pi);
            var hi = Math.IEEERemainder(this.Hi + radius, 2*S2.Pi);
            if (lo == -S2.Pi)
            {
                lo = S2.Pi;
            }
            return new S1Interval(lo, hi);
        }

        /**
   * Return the smallest interval that contains this interval and the given
   * interval "y".
   */

        public S1Interval Union(S1Interval y)
        {
            // The y.is_full() case is handled correctly in all cases by the code
            // below, but can follow three separate code paths depending on whether
            // this interval is inverted, is non-inverted but contains Pi, or neither.

            if (y.IsEmpty)
            {
                return this;
            }
            if (this.FastContains(y.Lo))
            {
                if (this.FastContains(y.Hi))
                {
                    // Either this interval contains y, or the union of the two
                    // intervals is the Full() interval.
                    if (this.Contains(y))
                    {
                        return this; // is_full() code path
                    }
                    return Full;
                }
                return new S1Interval(this.Lo, y.Hi, true);
            }
            if (this.FastContains(y.Hi))
            {
                return new S1Interval(y.Lo, this.Hi, true);
            }

            // This interval contains neither endpoint of y. This means that either y
            // contains all of this interval, or the two intervals are disjoint.
            if (this.IsEmpty || y.FastContains(this.Lo))
            {
                return y;
            }

            // Check which pair of endpoints are closer together.
            var dlo = PositiveDistance(y.Hi, this.Lo);
            var dhi = PositiveDistance(this.Hi, y.Lo);
            if (dlo < dhi)
            {
                return new S1Interval(y.Lo, this.Hi, true);
            }
            else
            {
                return new S1Interval(this.Lo, y.Hi, true);
            }
        }

        /**
   * Return the smallest interval that contains the intersection of this
   * interval with "y". Note that the region of intersection may consist of two
   * disjoint intervals.
   */

        public S1Interval Intersection(S1Interval y)
        {
            // The y.is_full() case is handled correctly in all cases by the code
            // below, but can follow three separate code paths depending on whether
            // this interval is inverted, is non-inverted but contains Pi, or neither.

            if (y.IsEmpty)
            {
                return Empty;
            }
            if (this.FastContains(y.Lo))
            {
                if (this.FastContains(y.Hi))
                {
                    // Either this interval contains y, or the region of intersection
                    // consists of two disjoint subintervals. In either case, we want
                    // to return the shorter of the two original intervals.
                    if (y.Length < this.Length)
                    {
                        return y; // is_full() code path
                    }
                    return this;
                }
                return new S1Interval(y.Lo, this.Hi, true);
            }
            if (this.FastContains(y.Hi))
            {
                return new S1Interval(this.Lo, y.Hi, true);
            }

            // This interval contains neither endpoint of y. This means that either y
            // contains all of this interval, or the two intervals are disjoint.

            if (y.FastContains(this.Lo))
            {
                return this; // is_empty() okay here
            }
            // assert (!intersects(y));
            return Empty;
        }

        /**
   * Return true if the length of the symmetric difference between the two
   * intervals is at most the given tolerance.
   */

        public bool ApproxEquals(S1Interval y, double maxError)
        {
            if (this.IsEmpty)
            {
                return y.Length <= maxError;
            }
            if (y.IsEmpty)
            {
                return this.Length <= maxError;
            }
            return (Math.Abs(Math.IEEERemainder(y.Lo - this.Lo, 2*S2.Pi))
                    + Math.Abs(Math.IEEERemainder(y.Hi - this.Hi, 2*S2.Pi))) <= maxError;
        }

        public bool ApproxEquals(S1Interval y)
        {
            return this.ApproxEquals(y, 1e-9);
        }


        public override string ToString()
        {
            return "[" + this.Lo + ", " + this.Hi + "]";
        }


        /**
   * Compute the distance from "a" to "b" in the range [0, 2*Pi). This is
   * equivalent to (drem(b - a - S2.M_PI, 2 * S2.M_PI) + S2.M_PI), except that
   * it is more numerically stable (it does not lose precision for very small
   * positive distances).
   */

        public static double PositiveDistance(double a, double b)
        {
            var d = b - a;
            if (d >= 0)
            {
                return d;
            }
            // We want to ensure that if b == Pi and a == (-Pi + eps),
            // the return result is approximately 2*Pi and not zero.
            return (b + S2.Pi) - (a - S2.Pi);
        }
    }
}