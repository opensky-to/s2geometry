using System;

namespace S2Geometry.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenSky.S2Geometry;

    [TestClass]
    public class S2CapTest : GeometryTestCase
    {
        public S2Point getLatLngPoint(double latDegrees, double lngDegrees)
        {
            return S2LatLng.FromDegrees(latDegrees, lngDegrees).ToPoint();
        }

        // About 9 times the double-precision roundoff relative error.
        public const double EPS = 1e-15;

        public void testRectBound()
        {
            // Empty and full caps.
            Assert.IsTrue(S2Cap.Empty.RectBound.IsEmpty);
            Assert.IsTrue(S2Cap.Full.RectBound.IsFull);

            var kDegreeEps = 1e-13;
            // Maximum allowable error for latitudes and longitudes measured in
            // degrees. (assertDoubleNear uses a fixed tolerance that is too small.)

            // Cap that includes the south pole.
            var rect =
                S2Cap.FromAxisAngle(getLatLngPoint(-45, 57), S1Angle.FromDegrees(50)).RectBound;
            assertDoubleNear(rect.LatLo.Degrees, -90, kDegreeEps);
            assertDoubleNear(rect.LatHi.Degrees, 5, kDegreeEps);
            Assert.IsTrue(rect.Lng.IsFull);

            // Cap that is tangent to the north pole.
            rect = S2Cap.FromAxisAngle(S2Point.Normalize(new S2Point(1, 0, 1)), S1Angle.FromRadians(S2.PiOver4)).RectBound;
            assertDoubleNear(rect.Lat.Lo, 0);
            assertDoubleNear(rect.Lat.Hi, S2.PiOver2);
            Assert.IsTrue(rect.Lng.IsFull);

            rect = S2Cap
                .FromAxisAngle(S2Point.Normalize(new S2Point(1, 0, 1)), S1Angle.FromDegrees(45)).RectBound;
            assertDoubleNear(rect.LatLo.Degrees, 0, kDegreeEps);
            assertDoubleNear(rect.LatHi.Degrees, 90, kDegreeEps);
            Assert.IsTrue(rect.Lng.IsFull);

            // The eastern hemisphere.
            rect = S2Cap
                .FromAxisAngle(new S2Point(0, 1, 0), S1Angle.FromRadians(S2.PiOver2 + 5e-16)).RectBound;
            assertDoubleNear(rect.LatLo.Degrees, -90, kDegreeEps);
            assertDoubleNear(rect.LatHi.Degrees, 90, kDegreeEps);
            Assert.IsTrue(rect.Lng.IsFull);

            // A cap centered on the equator.
            rect = S2Cap.FromAxisAngle(getLatLngPoint(0, 50), S1Angle.FromDegrees(20)).RectBound;
            assertDoubleNear(rect.LatLo.Degrees, -20, kDegreeEps);
            assertDoubleNear(rect.LatHi.Degrees, 20, kDegreeEps);
            assertDoubleNear(rect.LngLo.Degrees, 30, kDegreeEps);
            assertDoubleNear(rect.LngHi.Degrees, 70, kDegreeEps);

            // A cap centered on the north pole.
            rect = S2Cap.FromAxisAngle(getLatLngPoint(90, 123), S1Angle.FromDegrees(10)).RectBound;
            assertDoubleNear(rect.LatLo.Degrees, 80, kDegreeEps);
            assertDoubleNear(rect.LatHi.Degrees, 90, kDegreeEps);
            Assert.IsTrue(rect.Lng.IsFull);
        }

        public void testCells()
        {
            // For each cube face, we construct some cells on
            // that face and some caps whose positions are relative to that face,
            // and then check for the expected intersection/containment results.

            // The distance from the center of a face to one of its vertices.
            var kFaceRadius = Math.Atan(S2.Sqrt2);

            for (var face = 0; face < 6; ++face)
            {
                // The cell consisting of the entire face.
                var rootCell = S2Cell.FromFacePosLevel(face, (byte)0, 0);

                // A leaf cell at the midpoint of the v=1 edge.
                var edgeCell = new S2Cell(S2Projections.FaceUvToXyz(face, 0, 1 - EPS));

                // A leaf cell at the u=1, v=1 corner.
                var cornerCell = new S2Cell(S2Projections.FaceUvToXyz(face, 1 - EPS, 1 - EPS));

                // Quick check for full and empty caps.
                Assert.IsTrue(S2Cap.Full.Contains(rootCell));
                Assert.IsTrue(!S2Cap.Empty.MayIntersect(rootCell));

                // Check intersections with the bounding caps of the leaf cells that are
                // adjacent to 'corner_cell' along the Hilbert curve. Because this corner
                // is at (u=1,v=1), the curve stays locally within the same cube face.
                var first = cornerCell.Id.Previous.Previous.Previous;
                var last = cornerCell.Id.Next.Next.Next.Next;
                for (var id = first; id <last; id = id.Next)
                {
                    var cell = new S2Cell(id);
                    JavaAssert.Equal(cell.CapBound.Contains(cornerCell), id.Equals(cornerCell.Id));
                    JavaAssert.Equal(
                        cell.CapBound.MayIntersect(cornerCell), id.Parent.Contains(cornerCell.Id));
                }

                var antiFace = (face + 3)%6; // Opposite face.
                for (var capFace = 0; capFace < 6; ++capFace)
                {
                    // A cap that barely contains all of 'cap_face'.
                    var center = S2Projections.GetNorm(capFace);
                    var covering = S2Cap.FromAxisAngle(center, S1Angle.FromRadians(kFaceRadius + EPS));
                    JavaAssert.Equal(covering.Contains(rootCell), capFace == face);
                    JavaAssert.Equal(covering.MayIntersect(rootCell), capFace != antiFace);
                    JavaAssert.Equal(covering.Contains(edgeCell), center.DotProd(edgeCell.Center) > 0.1);
                    JavaAssert.Equal(covering.Contains(edgeCell), covering.MayIntersect(edgeCell));
                    JavaAssert.Equal(covering.Contains(cornerCell), capFace == face);
                    JavaAssert.Equal(
                        covering.MayIntersect(cornerCell), center.DotProd(cornerCell.Center) > 0);

                    // A cap that barely intersects the edges of 'cap_face'.
                    var bulging = S2Cap.FromAxisAngle(center, S1Angle.FromRadians(S2.PiOver4 + EPS));
                    Assert.IsTrue(!bulging.Contains(rootCell));
                    JavaAssert.Equal(bulging.MayIntersect(rootCell), capFace != antiFace);
                    JavaAssert.Equal(bulging.Contains(edgeCell), capFace == face);
                    JavaAssert.Equal(bulging.MayIntersect(edgeCell), center.DotProd(edgeCell.Center) > 0.1);
                    Assert.IsTrue(!bulging.Contains(cornerCell));
                    Assert.IsTrue(!bulging.MayIntersect(cornerCell));

                    // A singleton cap.
                    var singleton = S2Cap.FromAxisAngle(center, S1Angle.FromRadians(0));
                    JavaAssert.Equal(singleton.MayIntersect(rootCell), capFace == face);
                    Assert.IsTrue(!singleton.MayIntersect(edgeCell));
                    Assert.IsTrue(!singleton.MayIntersect(cornerCell));
                }
            }
        }

        [TestMethod]
        public void S2CapBasicTest()
        {
            // Test basic properties of empty and full caps.
            var empty = S2Cap.Empty;
            var full = S2Cap.Full;
            Assert.IsTrue(empty.IsValid);
            Assert.IsTrue(empty.IsEmpty);
            Assert.IsTrue(empty.Complement.IsFull);
            Assert.IsTrue(full.IsValid);
            Assert.IsTrue(full.IsFull);
            Assert.IsTrue(full.Complement.IsEmpty);
            JavaAssert.Equal(full.Height, 2.0);
            assertDoubleNear(full.Angle.Degrees, 180);

            // Containment and intersection of empty and full caps.
            Assert.IsTrue(empty.Contains(empty));
            Assert.IsTrue(full.Contains(empty));
            Assert.IsTrue(full.Contains(full));
            Assert.IsTrue(!empty.InteriorIntersects(empty));
            Assert.IsTrue(full.InteriorIntersects(full));
            Assert.IsTrue(!full.InteriorIntersects(empty));

            // Singleton cap containing the x-axis.
            var xaxis = S2Cap.FromAxisHeight(new S2Point(1, 0, 0), 0);
            Assert.IsTrue(xaxis.Contains(new S2Point(1, 0, 0)));
            Assert.IsTrue(!xaxis.Contains(new S2Point(1, 1e-20, 0)));
            JavaAssert.Equal(xaxis.Angle.Radians, 0.0);

            // Singleton cap containing the y-axis.
            var yaxis = S2Cap.FromAxisAngle(new S2Point(0, 1, 0), S1Angle.FromRadians(0));
            Assert.IsTrue(!yaxis.Contains(xaxis.Axis));
            JavaAssert.Equal(xaxis.Height, 0.0);

            // Check that the complement of a singleton cap is the full cap.
            var xcomp = xaxis.Complement;
            Assert.IsTrue(xcomp.IsValid);
            Assert.IsTrue(xcomp.IsFull);
            Assert.IsTrue(xcomp.Contains(xaxis.Axis));

            // Check that the complement of the complement is *not* the original.
            Assert.IsTrue(xcomp.Complement.IsValid);
            Assert.IsTrue(xcomp.Complement.IsEmpty);
            Assert.IsTrue(!xcomp.Complement.Contains(xaxis.Axis));

            // Check that very small caps can be represented accurately.
            // Here "kTinyRad" is small enough that unit vectors perturbed by this
            // amount along a tangent do not need to be renormalized.
            var kTinyRad = 1e-10;
            var tiny =
                S2Cap.FromAxisAngle(S2Point.Normalize(new S2Point(1, 2, 3)), S1Angle.FromRadians(kTinyRad));
            var tangent = S2Point.Normalize(S2Point.CrossProd(tiny.Axis, new S2Point(3, 2, 1)));
            Assert.IsTrue(tiny.Contains(tiny.Axis + (tangent* 0.99*kTinyRad)));
            Assert.IsTrue(!tiny.Contains(tiny.Axis + (tangent* 1.01*kTinyRad)));

            // Basic tests on a hemispherical cap.
            var hemi = S2Cap.FromAxisHeight(S2Point.Normalize(new S2Point(1, 0, 1)), 1);
            JavaAssert.Equal(hemi.Complement.Axis, -hemi.Axis);
            JavaAssert.Equal(hemi.Complement.Height, 1.0);
            Assert.IsTrue(hemi.Contains(new S2Point(1, 0, 0)));
            Assert.IsTrue(!hemi.Complement.Contains(new S2Point(1, 0, 0)));
            Assert.IsTrue(hemi.Contains(S2Point.Normalize(new S2Point(1, 0, -(1 - EPS)))));
            Assert.IsTrue(!hemi.InteriorContains(S2Point.Normalize(new S2Point(1, 0, -(1 + EPS)))));

            // A concave cap.
            var concave = S2Cap.FromAxisAngle(getLatLngPoint(80, 10), S1Angle.FromDegrees(150));
            Assert.IsTrue(concave.Contains(getLatLngPoint(-70*(1 - EPS), 10)));
            Assert.IsTrue(!concave.Contains(getLatLngPoint(-70*(1 + EPS), 10)));
            Assert.IsTrue(concave.Contains(getLatLngPoint(-50*(1 - EPS), -170)));
            Assert.IsTrue(!concave.Contains(getLatLngPoint(-50*(1 + EPS), -170)));

            // Cap containment tests.
            Assert.IsTrue(!empty.Contains(xaxis));
            Assert.IsTrue(!empty.InteriorIntersects(xaxis));
            Assert.IsTrue(full.Contains(xaxis));
            Assert.IsTrue(full.InteriorIntersects(xaxis));
            Assert.IsTrue(!xaxis.Contains(full));
            Assert.IsTrue(!xaxis.InteriorIntersects(full));
            Assert.IsTrue(xaxis.Contains(xaxis));
            Assert.IsTrue(!xaxis.InteriorIntersects(xaxis));
            Assert.IsTrue(xaxis.Contains(empty));
            Assert.IsTrue(!xaxis.InteriorIntersects(empty));
            Assert.IsTrue(hemi.Contains(tiny));
            Assert.IsTrue(hemi.Contains(
                S2Cap.FromAxisAngle(new S2Point(1, 0, 0), S1Angle.FromRadians(S2.PiOver4 - EPS))));
            Assert.IsTrue(!hemi.Contains(
                S2Cap.FromAxisAngle(new S2Point(1, 0, 0), S1Angle.FromRadians(S2.PiOver4 + EPS))));
            Assert.IsTrue(concave.Contains(hemi));
            Assert.IsTrue(concave.InteriorIntersects(hemi.Complement));
            Assert.IsTrue(!concave.Contains(S2Cap.FromAxisHeight(-concave.Axis, 0.1)));
        }
    }
}