namespace OpenSky.S2Geometry.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenSky.S2Geometry;

    [TestClass]
    public class S2PolygonTest : GeometryTestCase
    {
        // A set of nested loops around the point 0:0 (lat:lng).
        // Every vertex of NEAR0 is a vertex of NEAR1.
        private static readonly String NEAR0 = "-1:0, 0:1, 1:0, 0:-1;";
        private static readonly String NEAR1 = "-1:-1, -1:0, -1:1, 0:1, 1:1, 1:0, 1:-1, 0:-1;";
        private static readonly String NEAR2 = "5:-2, -2:5, -1:-2;";
        private static readonly String NEAR3 = "6:-3, -3:6, -2:-2;";
        private static readonly String NEAR_HEMI = "0:-90, -90:0, 0:90, 90:0;";

        // A set of nested loops around the point 0:180 (lat:lng).
        // Every vertex of FAR0 and FAR2 belongs to FAR1, and all
        // the loops except FAR2 are non-convex.
        private static readonly String FAR0 = "0:179, 1:180, 0:-179, 2:-180;";

        private static readonly String FAR1 =
            "0:179, -1:179, 1:180, -1:-179, 0:-179, 3:-178, 2:-180, 3:178;";

        private static readonly String FAR2 = "-1:-179, -1:179, 3:178, 3:-178;"; // opposite
        // direction
        private static readonly String FAR3 = "-3:-178, -2:179, -3:178, 4:177, 4:-177;";
        private static readonly String FAR_HEMI = "0:-90, 60:90, -60:90;";

        // A set of nested loops around the point -90:0 (lat:lng).
        private static readonly String SOUTH0a = "-90:0, -89.99:0, -89.99:0.01;";
        private static readonly String SOUTH0b = "-90:0, -89.99:0.02, -89.99:0.03;";
        private static readonly String SOUTH0c = "-90:0, -89.99:0.04, -89.99:0.05;";
        private static readonly String SOUTH1 = "-90:0, -89.9:-0.1, -89.9:0.1;";
        private static readonly String SOUTH2 = "-90:0, -89.8:-0.2, -89.8:0.2;";
        private static readonly String SOUTH_HEMI = "0:-180, 0:60, 0:-60;";

        // Two different loops that surround all the Near and Far loops except
        // for the hemispheres.
        private static readonly String NEAR_FAR1 =
            "-1:-9, -9:-9, -9:9, 9:9, 9:-9, 1:-9, " + "1:-175, 9:-175, 9:175, -9:175, -9:-175, -1:-175;";

        private static readonly String NEAR_FAR2 =
            "-8:-4, 8:-4, 2:15, 2:170, 8:-175, -8:-175, -2:170, -2:15;";

        // Two rectangles that are "adjacent", but rather than having common edges,
        // those edges are slighly off. A third rectangle that is not adjacent to
        // either of the first two.
        private static readonly String ADJACENT0 = "0:1, 1:1, 2:1, 2:0, 1:0, 0:0;";
        private static readonly String ADJACENT1 = "0:2, 1:2, 2:2, 2:1.01, 1:0.99, 0:1.01;";
        private static readonly String UN_ADJACENT = "10:10, 11:10, 12:10, 12:9, 11:9, 10:9;";

        // Shapes used to test comparison functions for polygons.
        private static readonly String RECTANGLE1 = "0:1, 1:1, 2:1, 2:0, 1:0, 0:0;";
        private static readonly String RECTANGLE2 = "5:1, 6:1, 7:1, 7:0, 6:0, 5:0;";
        private static readonly String TRIANGLE = "15:0, 17:0, 16:2;";
        private static readonly String TRIANGLE_ROT = "17:0, 16:2, 15:0;";

        private void assertContains(String aStr, String bStr)
        {
            var a = makePolygon(aStr);
            var b = makePolygon(bStr);
            assertTrue(a.Contains(b));
        }

        // Make sure we've set things up correctly.
        public void testInit()
        {
            this.assertContains(NEAR1, NEAR0);
            this.assertContains(NEAR2, NEAR1);
            this.assertContains(NEAR3, NEAR2);
            this.assertContains(NEAR_HEMI, NEAR3);
            this.assertContains(FAR1, FAR0);
            this.assertContains(FAR2, FAR1);
            this.assertContains(FAR3, FAR2);
            this.assertContains(FAR_HEMI, FAR3);
            this.assertContains(SOUTH1, SOUTH0a);
            this.assertContains(SOUTH1, SOUTH0b);
            this.assertContains(SOUTH1, SOUTH0c);
            this.assertContains(SOUTH_HEMI, SOUTH2);
            this.assertContains(NEAR_FAR1, NEAR3);
            this.assertContains(NEAR_FAR1, FAR3);
            this.assertContains(NEAR_FAR2, NEAR3);
            this.assertContains(NEAR_FAR2, FAR3);
        }

        private readonly S2Polygon near10 = makePolygon(NEAR0 + NEAR1);
        private readonly S2Polygon near30 = makePolygon(NEAR3 + NEAR0);
        private readonly S2Polygon near32 = makePolygon(NEAR2 + NEAR3);
        private readonly S2Polygon near3210 = makePolygon(NEAR0 + NEAR2 + NEAR3 + NEAR1);
        private readonly S2Polygon nearH3210 = makePolygon(NEAR0 + NEAR2 + NEAR3 + NEAR_HEMI + NEAR1);

        private readonly S2Polygon far10 = makePolygon(FAR0 + FAR1);
        private readonly S2Polygon far21 = makePolygon(FAR2 + FAR1);
        private readonly S2Polygon far321 = makePolygon(FAR2 + FAR3 + FAR1);
        private readonly S2Polygon farH20 = makePolygon(FAR2 + FAR_HEMI + FAR0);
        private readonly S2Polygon farH3210 = makePolygon(FAR2 + FAR_HEMI + FAR0 + FAR1 + FAR3);

        private readonly S2Polygon south0ab = makePolygon(SOUTH0a + SOUTH0b);
        private readonly S2Polygon south2 = makePolygon(SOUTH2);
        private readonly S2Polygon south210b = makePolygon(SOUTH2 + SOUTH0b + SOUTH1);
        private readonly S2Polygon southH21 = makePolygon(SOUTH2 + SOUTH_HEMI + SOUTH1);
        private readonly S2Polygon southH20abc = makePolygon(SOUTH2 + SOUTH0b + SOUTH_HEMI + SOUTH0a + SOUTH0c);

        private readonly S2Polygon nf1n10f2s10abc =
            makePolygon(SOUTH0c + FAR2 + NEAR1 + NEAR_FAR1 + NEAR0 + SOUTH1 + SOUTH0b + SOUTH0a);

        private readonly S2Polygon nf2n2f210s210ab =
            makePolygon(FAR2 + SOUTH0a + FAR1 + SOUTH1 + FAR0 + SOUTH0b + NEAR_FAR2 + SOUTH2 + NEAR2);

        private readonly S2Polygon f32n0 = makePolygon(FAR2 + NEAR0 + FAR3);
        private readonly S2Polygon n32s0b = makePolygon(NEAR3 + SOUTH0b + NEAR2);

        private readonly S2Polygon adj0 = makePolygon(ADJACENT0);
        private readonly S2Polygon adj1 = makePolygon(ADJACENT1);
        private readonly S2Polygon unAdj = makePolygon(UN_ADJACENT);

        private void assertRelation(S2Polygon a, S2Polygon b, int contains, bool intersects)
        {
            assertEquals(a.Contains(b), contains > 0);
            assertEquals(b.Contains(a), contains < 0);
            assertEquals(a.Intersects(b), intersects);
        }

        private void assertPointApproximatelyEquals(
            S2Loop s2Loop, int vertexIndex, double lat, double lng, double error)
        {
            var latLng = new S2LatLng(s2Loop.Vertex(vertexIndex));
            this.assertDoubleNear(latLng.LatDegrees, lat, error);
            this.assertDoubleNear(latLng.LngDegrees, lng, error);
        }

        private void checkEqual(S2Polygon a, S2Polygon b)
        {
            var MAX_ERROR = 1e-31;

            if (a.IsNormalized && b.IsNormalized)
            {
                var r = a.BoundaryApproxEquals(b, MAX_ERROR);
                assertTrue(r);
            }
            else
            {
                var builder = new S2PolygonBuilder(S2PolygonBuilderOptions.UndirectedXor);
                var a2 = new S2Polygon();
                var b2 = new S2Polygon();
                builder.AddPolygon(a);
                assertTrue(builder.AssemblePolygon(a2, null));
                builder.AddPolygon(b);
                assertTrue(builder.AssemblePolygon(b2, null));
                assertTrue(a2.BoundaryApproxEquals(b2, MAX_ERROR));
            }
        }

        public void tryUnion(S2Polygon a, S2Polygon b)
        {
            var union = new S2Polygon();
            union.InitToUnion(a, b);

            var polygons = new List<S2Polygon>();
            polygons.Add(new S2Polygon(a));
            polygons.Add(new S2Polygon(b));
            var destructiveUnion = S2Polygon.DestructiveUnion(polygons);

            this.checkEqual(union, destructiveUnion);
        }

        [TestMethod]
        public void testCompareTo()
        {
            // Polygons with same loops, but in different order:
            var p1 = makePolygon(RECTANGLE1 + RECTANGLE2);
            var p2 = makePolygon(RECTANGLE2 + RECTANGLE1);
            assertEquals(0, p1.CompareTo(p2));

            // Polygons with same loops, but in different order and containins a
            // different number of points.
            var p3 = makePolygon(RECTANGLE1 + TRIANGLE);
            var p4 = makePolygon(TRIANGLE + RECTANGLE1);
            assertEquals(0, p3.CompareTo(p4));

            // Polygons with same logical loop (but loop is reordered).
            var p5 = makePolygon(TRIANGLE);
            var p6 = makePolygon(TRIANGLE_ROT);
            assertEquals(0, p5.CompareTo(p6));

            // Polygons with a differing number of loops
            var p7 = makePolygon(RECTANGLE1 + RECTANGLE2);
            var p8 = makePolygon(TRIANGLE);
            assertTrue(0 > p8.CompareTo(p7));
            assertTrue(0 < p7.CompareTo(p8));

            // Polygons with a differing number of loops (one a subset of the other)
            var p9 = makePolygon(RECTANGLE1 + RECTANGLE2 + TRIANGLE);
            var p10 = makePolygon(RECTANGLE1 + RECTANGLE2);
            assertTrue(0 < p9.CompareTo(p10));
            assertTrue(0 > p10.CompareTo(p9));
        }

        [TestMethod]
        public void testDisjoint()
        {
            var builder = new S2PolygonBuilder(S2PolygonBuilderOptions.UndirectedXor);
            builder.AddPolygon(this.adj0);
            builder.AddPolygon(this.unAdj);
            var ab = new S2Polygon();
            assertTrue(builder.AssemblePolygon(ab, null));

            var union = new S2Polygon();
            union.InitToUnion(this.adj0, this.unAdj);
            assertEquals(2, union.NumLoops);

            this.checkEqual(ab, union);
            this.tryUnion(this.adj0, this.unAdj);
        }

        [TestMethod]
        public void testGetDistance()
        {
            // Error margin since we're doing numerical computations
            var epsilon = 1e-15;

            // A rectangle with (lat,lng) vertices (3,1), (3,-1), (-3,-1) and (-3,1)
            var inner = "3:1, 3:-1, -3:-1, -3:1;";
            // A larger rectangle with (lat,lng) vertices (4,2), (4,-2), (-4,-2) and
            // (-4,s)
            var outer = "4:2, 4:-2, -4:-2, -4:2;";


            var rect = makePolygon(inner);
            var shell = makePolygon(inner + outer);

            // All of the vertices of a polygon should be distance 0
            for (var i = 0; i < shell.NumLoops; i++)
            {
                for (var j = 0; j < shell.Loop(i).NumVertices; j++)
                {
                    assertEquals(0d, shell.GetDistance(shell.Loop(i).Vertex(j)).Radians, epsilon);
                }
            }

            // A non-vertex point on an edge should be distance 0
            assertEquals(0d, rect.GetDistance(
                S2Point.Normalize(rect.Loop(0).Vertex(0) + rect.Loop(0).Vertex(1))).Radians,
                         epsilon);

            var origin = S2LatLng.FromDegrees(0, 0).ToPoint();
            // rect contains the origin
            assertEquals(0d, rect.GetDistance(origin).Radians, epsilon);

            // shell does NOT contain the origin, since it has a hole. The shortest
            // distance is to (1,0) or (-1,0), and should be 1 degree
            assertEquals(1d, shell.GetDistance(origin).Degrees, epsilon);
        }

        [TestMethod]
        public void testRelations()
        {
            this.assertRelation(this.near10, this.near30, -1, true);
            this.assertRelation(this.near10, this.near32, 0, false);
            this.assertRelation(this.near10, this.near3210, -1, true);
            this.assertRelation(this.near10, this.nearH3210, 0, false);
            this.assertRelation(this.near30, this.near32, 1, true);
            this.assertRelation(this.near30, this.near3210, 1, true);
            this.assertRelation(this.near30, this.nearH3210, 0, true);
            this.assertRelation(this.near32, this.near3210, -1, true);
            this.assertRelation(this.near32, this.nearH3210, 0, false);
            this.assertRelation(this.near3210, this.nearH3210, 0, false);

            this.assertRelation(this.far10, this.far21, 0, false);
            this.assertRelation(this.far10, this.far321, -1, true);
            this.assertRelation(this.far10, this.farH20, 0, false);
            this.assertRelation(this.far10, this.farH3210, 0, false);
            this.assertRelation(this.far21, this.far321, 0, false);
            this.assertRelation(this.far21, this.farH20, 0, false);
            this.assertRelation(this.far21, this.farH3210, -1, true);
            this.assertRelation(this.far321, this.farH20, 0, true);
            this.assertRelation(this.far321, this.farH3210, 0, true);
            this.assertRelation(this.farH20, this.farH3210, 0, true);

            this.assertRelation(this.south0ab, this.south2, -1, true);
            this.assertRelation(this.south0ab, this.south210b, 0, true);
            this.assertRelation(this.south0ab, this.southH21, -1, true);
            this.assertRelation(this.south0ab, this.southH20abc, -1, true);
            this.assertRelation(this.south2, this.south210b, 1, true);
            this.assertRelation(this.south2, this.southH21, 0, true);
            this.assertRelation(this.south2, this.southH20abc, 0, true);
            this.assertRelation(this.south210b, this.southH21, 0, true);
            this.assertRelation(this.south210b, this.southH20abc, 0, true);
            this.assertRelation(this.southH21, this.southH20abc, 1, true);

            this.assertRelation(this.nf1n10f2s10abc, this.nf2n2f210s210ab, 0, true);
            this.assertRelation(this.nf1n10f2s10abc, this.near32, 1, true);
            this.assertRelation(this.nf1n10f2s10abc, this.far21, 0, false);
            this.assertRelation(this.nf1n10f2s10abc, this.south0ab, 0, false);
            this.assertRelation(this.nf1n10f2s10abc, this.f32n0, 1, true);

            this.assertRelation(this.nf2n2f210s210ab, this.near10, 0, false);
            this.assertRelation(this.nf2n2f210s210ab, this.far10, 1, true);
            this.assertRelation(this.nf2n2f210s210ab, this.south210b, 1, true);
            this.assertRelation(this.nf2n2f210s210ab, this.south0ab, 1, true);
            this.assertRelation(this.nf2n2f210s210ab, this.n32s0b, 1, true);
        }

        [TestMethod]
        public void testUnionSloppyFailure()
        {
            var polygons = new List<S2Polygon>();
            polygons.Add(this.adj0);
            polygons.Add(this.unAdj);
            // The polygons are sufficiently far apart that this angle will not
            // bring them together:
            var union = S2Polygon.DestructiveUnionSloppy(polygons, S1Angle.FromDegrees(0.1));

            assertEquals(2, union.NumLoops);
        }

        [TestMethod]
        public void testUnionSloppySuccess()
        {
            var polygons = new List<S2Polygon>();
            polygons.Add(this.adj0);
            polygons.Add(this.adj1);
            var union = S2Polygon.DestructiveUnionSloppy(polygons, S1Angle.FromDegrees(0.1));

            assertEquals(1, union.NumLoops);
            if (union.NumLoops != 1)
            {
                return;
            }
            var s2Loop = union.Loop(0);
            assertEquals(8, s2Loop.NumVertices);
            if (s2Loop.NumVertices != 8)
            {
                return;
            }
            this.assertPointApproximatelyEquals(s2Loop, 0, 2.0, 0.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 1, 1.0, 0.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 2, 0.0, 0.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 3, 0.0, 1.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 4, 0.0, 2.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 5, 1.0, 2.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 6, 2.0, 2.0, 0.01);
            this.assertPointApproximatelyEquals(s2Loop, 7, 2.0, 1.0, 0.01);
        }
    }
}