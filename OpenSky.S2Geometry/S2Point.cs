namespace OpenSky.S2Geometry
{
    using System;

    public struct S2Point : IEquatable<S2Point>, IComparable<S2Point>
    {
// coordinates of the points
        private readonly double x;
        private readonly double y;
        private readonly double z;


        public S2Point(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double X
        {
            get { return this.x; }
        }

        public double Y
        {
            get { return this.y; }
        }

        public double Z
        {
            get { return this.z; }
        }

        public double Norm2
        {
            get { return this.x*this.x + this.y*this.y + this.z*this.z; }
        }

        public double Norm
        {
            get { return Math.Sqrt(this.Norm2); }
        }

        public S2Point Ortho
        {
            get
            {
                var k = this.LargestAbsComponent;
                S2Point temp;
                if (k == 1)
                {
                    temp = new S2Point(1, 0, 0);
                }
                else if (k == 2)
                {
                    temp = new S2Point(0, 1, 0);
                }
                else
                {
                    temp = new S2Point(0, 0, 1);
                }
                return Normalize(CrossProd(this, temp));
            }
        }

        /** Return the index of the largest component fabs */

        public int LargestAbsComponent
        {
            get
            {
                var temp = Fabs(this);
                if (temp.x > temp.y)
                {
                    if (temp.x > temp.z)
                    {
                        return 0;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    if (temp.y > temp.z)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }
        }

        public double this[int axis]
        {
            get { return (axis == 0) ? this.x : (axis == 1) ? this.y : this.z; }
        }

        public int CompareTo(S2Point other)
        {
            return this < other ? -1 : (this.Equals(other) ? 0 : 1);
        }

        public bool Equals(S2Point other)
        {
            return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((S2Point)obj);
        }

        /**
   * Calcualates hashcode based on stored coordinates. Since we want +0.0 and
   * -0.0 to be treated the same, we ignore the sign of the coordinates.
   */

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Math.Abs(this.x).GetHashCode();
                hashCode = (hashCode*397) ^ Math.Abs(this.y).GetHashCode();
                hashCode = (hashCode*397) ^ Math.Abs(this.z).GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(S2Point left, S2Point right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(S2Point left, S2Point right)
        {
            return !Equals(left, right);
        }

        public static S2Point operator -(S2Point p1, S2Point p2)
        {
            return new S2Point(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        public static S2Point operator -(S2Point p)
        {
            return new S2Point(-p.x, -p.y, -p.z);
        }

        public static S2Point CrossProd(S2Point p1, S2Point p2)
        {
            return new S2Point(
                p1.y*p2.z - p1.z*p2.y, p1.z*p2.x - p1.x*p2.z, p1.x*p2.y - p1.y*p2.x);
        }

        public static S2Point operator +(S2Point p1, S2Point p2)
        {
            return new S2Point(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public double DotProd(S2Point that)
        {
            return this.x*that.x + this.y*that.y + this.z*that.z;
        }

        public static S2Point operator *(S2Point p, double m)
        {
            return new S2Point(m*p.x, m*p.y, m*p.z);
        }

        public static S2Point operator /(S2Point p, double m)
        {
            return new S2Point(p.x/m, p.y/m, p.z/m);
        }


        /** return a vector orthogonal to this one */

        public static S2Point Fabs(S2Point p)
        {
            return new S2Point(Math.Abs(p.x), Math.Abs(p.y), Math.Abs(p.z));
        }

        public static S2Point Normalize(S2Point p)
        {
            var norm = p.Norm;
            if (norm != 0)
            {
                norm = 1.0/norm;
            }
            return p*norm;
        }


        /** Return the angle between two vectors in radians */

        public double Angle(S2Point va)
        {
            return Math.Atan2(CrossProd(this, va).Norm, this.DotProd(va));
        }

        /**
   * Compare two vectors, return true if all their components are within a
   * difference of margin.
   */

        public bool ApproxEquals(S2Point that, double margin)
        {
            return (Math.Abs(this.x - that.x) < margin) && (Math.Abs(this.y - that.y) < margin)
                                                          && (Math.Abs(this.z - that.z) < margin);
        }


        public static bool operator <(S2Point x, S2Point y)
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
            if (y.y < x.y)
            {
                return false;
            }
            if (x.z < y.z)
            {
                return true;
            }
            return false;
        }

        public static bool operator >(S2Point x, S2Point y)
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
            if (y.y > x.y)
            {
                return false;
            }
            if (x.z > y.z)
            {
                return true;
            }
            return false;
        }


        public override string ToString()
        {
            return "(" + this.x + ", " + this.y + ", " + this.z + ")";
        }

        public string ToDegreesString()
        {
            var s2LatLng = new S2LatLng(this);
            return "(" + s2LatLng.LatDegrees + ", "
                   + s2LatLng.LngDegrees + ")";
        }
    }
}