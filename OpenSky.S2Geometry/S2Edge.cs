namespace OpenSky.S2Geometry
{
    using System;

    /**
 * An abstract directed edge from one S2Point to another S2Point.
 *
 * @author kirilll@google.com (Kirill Levin)
 */

    public struct S2Edge : IEquatable<S2Edge>
    {
        private readonly S2Point end;
        private readonly S2Point start;

        public S2Edge(S2Point start, S2Point end)
        {
            this.start = start;
            this.end = end;
        }

        public S2Point Start
        {
            get { return this.start; }
        }

        public S2Point End
        {
            get { return this.end; }
        }

        public bool Equals(S2Edge other)
        {
            return this.end.Equals(other.end) && this.start.Equals(other.start);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is S2Edge && this.Equals((S2Edge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.end.GetHashCode()*397) ^ this.start.GetHashCode();
            }
        }


        public static bool operator ==(S2Edge left, S2Edge right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(S2Edge left, S2Edge right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("Edge: ({0} -> {1})\n   or [{2} -> {3}]",
                                 this.start.ToDegreesString(), this.end.ToDegreesString(), this.start, this.end);
        }
    }
}