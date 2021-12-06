namespace OpenSky.S2Geometry
{
    /**
 * The area of an interior, i.e. the region on the left side of an odd
 * number of loops and optionally a centroid.
 * The area is between 0 and 4*Pi. If it has a centroid, it is
 * the true centroid of the interiord multiplied by the area of the shape.
 * Note that the centroid may not be contained by the shape.
 *
 * @author dbentley@google.com (Daniel Bentley)
 */

    public struct S2AreaCentroid
    {
        private readonly double area;
        private readonly S2Point? centroid;

        public S2AreaCentroid(double area, S2Point? centroid = null)
        {
            this.area = area;
            this.centroid = centroid;
        }

        public double Area
        {
            get { return this.area; }
        }

        public S2Point? Centroid
        {
            get { return this.centroid; }
        }
    }
}