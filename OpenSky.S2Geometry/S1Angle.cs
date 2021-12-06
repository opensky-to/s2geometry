namespace OpenSky.S2Geometry
{
    using System;

    public struct S1Angle : IEquatable<S1Angle>, IComparable<S1Angle>
    {
        private readonly double radians;

        private S1Angle(double radians)
        {
            this.radians = radians;
        }


        /// <summary>
        ///     Return the angle between two points, which is also equal to the distance
        ///     between these points on the unit sphere. The points do not need to be
        ///     normalized.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public S1Angle(S2Point x, S2Point y)
        {
            this.radians = x.Angle(y);
        }

        public double Radians
        {
            get { return this.radians; }
        }

        public double Degrees
        {
            get { return this.radians*(180/Math.PI); }
        }


        public int CompareTo(S1Angle other)
        {
            return this.radians < other.radians ? -1 : this.radians > other.radians ? 1 : 0;
        }

        public bool Equals(S1Angle other)
        {
            return this.radians.Equals(other.radians);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is S1Angle && this.Equals((S1Angle)obj);
        }

        public override int GetHashCode()
        {
            return this.radians.GetHashCode();
        }

        public static bool operator ==(S1Angle left, S1Angle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(S1Angle left, S1Angle right)
        {
            return !Equals(left, right);
        }

        public long E5()
        {
            return (long)Math.Round(this.Degrees*1e5);
        }

        public long E6()
        {
            return (long)Math.Round(this.Degrees*1e6);
        }

        public long E7()
        {
            return (long)Math.Round(this.Degrees*1e7);
        }

        /**
   * The default constructor yields a zero angle.
   */

        public static bool operator <(S1Angle x, S1Angle y)
        {
            return x.Radians < y.Radians;
        }

        public static bool operator >(S1Angle x, S1Angle y)
        {
            return x.Radians > y.Radians;
        }

        public static bool operator <=(S1Angle x, S1Angle y)
        {
            return x.Radians <= y.Radians;
        }

        public static bool operator >=(S1Angle x, S1Angle y)
        {
            return x.Radians >= y.Radians;
        }

        public static S1Angle Max(S1Angle left, S1Angle right)
        {
            return right > left ? right : left;
        }

        public static S1Angle Min(S1Angle left, S1Angle right)
        {
            return right > left ? left : right;
        }

        public static S1Angle FromRadians(double radians)
        {
            return new S1Angle(radians);
        }

        public static S1Angle FromDegrees(double degrees)
        {
            return new S1Angle(degrees*(Math.PI/180));
        }

        public static S1Angle E5(long e5)
        {
            return FromDegrees(e5*1e-5);
        }

        public static S1Angle E6(long e6)
        {
            // Multiplying by 1e-6 isn't quite as accurate as dividing by 1e6,
            // but it's about 10 times faster and more than accurate enough.
            return FromDegrees(e6*1e-6);
        }

        public static S1Angle E7(long e7)
        {
            return FromDegrees(e7*1e-7);
        }

        /**
   * Writes the angle in degrees with a "d" suffix, e.g. "17.3745d". By default
   * 6 digits are printed; this can be changed using setprecision(). Up to 17
   * digits are required to distinguish one angle from another.
   */

        public override string ToString()
        {
            return this.Degrees + "d";
        }
    }
}