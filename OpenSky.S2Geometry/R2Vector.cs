namespace OpenSky.S2Geometry
{
    using System;
    using System.Collections.Generic;

    public struct R2Vector : IEquatable<R2Vector>
    {
        private readonly double x;
        private readonly double y;

        public R2Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        ///     Point as a list of 2; x is index 0, y is index 1
        /// </summary>
        /// <param name="coord"></param>
        public R2Vector(IList<double> coord)
        {
            if (coord.Count != 2)
            {
                throw new ArgumentException("Points must have exactly 2 coordinates", "coord");
            }
            this.x = coord[0];
            this.y = coord[1];
        }

        public double X
        {
            get { return this.x; }
        }

        public double Y
        {
            get { return this.y; }
        }

        public double this[int index]
        {
            get
            {
                if (index > 1)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return index == 0 ? this.x : this.y;
            }
        }

        public double Norm2
        {
            get { return (this.x*this.x) + (this.y*this.y); }
        }

        public bool Equals(R2Vector other)
        {
            return this.y.Equals(other.y) && this.x.Equals(other.x);
        }

        public override bool Equals(object obj)
        {
            return obj is R2Vector && this.Equals((R2Vector)obj);
        }


        /**
     * Calcualates hashcode based on stored coordinates. Since we want +0.0 and
     * -0.0 to be treated the same, we ignore the sign of the coordinates.
     */

        public override int GetHashCode()
        {
            unchecked
            {
                return (Math.Abs(this.y).GetHashCode()*397) ^ Math.Abs(this.x).GetHashCode();
            }
        }

        public static bool operator ==(R2Vector left, R2Vector right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(R2Vector left, R2Vector right)
        {
            return !Equals(left, right);
        }

        public static R2Vector operator +(R2Vector p1, R2Vector p2)
        {
            return new R2Vector(p1.x + p2.x, p1.y + p2.y);
        }


        public static R2Vector operator *(R2Vector p, double m)
        {
            return new R2Vector(m*p.x, m*p.y);
        }

        public static double DotProd(R2Vector p1, R2Vector p2)
        {
            return (p1.x*p2.x) + (p1.y*p2.y);
        }

        public double DotProd(R2Vector that)
        {
            return DotProd(this, that);
        }

        public double CrossProd(R2Vector that)
        {
            return this.x*that.y - this.y*that.x;
        }

        public static bool operator <(R2Vector x, R2Vector y)
        {
            if (x.x < y.x)
            {
                return true;
            }
            if (y.x < x.x)
            {
                return false;
            }
            if (x.y < y.y)
            {
                return true;
            }
            return false;
        }

        public static bool operator >(R2Vector x, R2Vector y)
        {
            if (x.x > y.x)
            {
                return true;
            }
            if (y.x > x.x)
            {
                return false;
            }
            if (x.y > y.y)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + this.x + ", " + this.y + ")";
        }
    }
}