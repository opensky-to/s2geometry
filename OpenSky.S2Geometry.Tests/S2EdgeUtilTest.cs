﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Common.Geometry;

namespace S2Geometry.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class S2EdgeUtilTest : GeometryTestCase
    {
        public const int DEGENERATE = -2;

        private void compareResult(int actual, int expected)
        {
            // HACK ALERT: RobustCrossing() is allowed to return 0 or -1 if either edge
            // is degenerate. We use the value kDegen to represent this possibility.
            if (expected == DEGENERATE)
            {
                assertTrue(actual <= 0);
            }
            else
            {
                assertEquals(expected, actual);
            }
        }

        private void assertCrossing(S2Point a,
                                    S2Point b,
                                    S2Point c,
                                    S2Point d,
                                    int robust,
                                    bool edgeOrVertex,
                                    bool simple)
        {
            a = S2Point.Normalize(a);
            b = S2Point.Normalize(b);
            c = S2Point.Normalize(c);
            d = S2Point.Normalize(d);

            compareResult(S2EdgeUtil.RobustCrossing(a, b, c, d), robust);
            if (simple)
            {
                assertEquals(robust > 0, S2EdgeUtil.SimpleCrossing(a, b, c, d));
            }
            var crosser = new EdgeCrosser(a, b, c);
            compareResult(crosser.RobustCrossing(d), robust);
            compareResult(crosser.RobustCrossing(c), robust);

            assertEquals(S2EdgeUtil.EdgeOrVertexCrossing(a, b, c, d), edgeOrVertex);
            assertEquals(edgeOrVertex, crosser.EdgeOrVertexCrossing(d));
            assertEquals(edgeOrVertex, crosser.EdgeOrVertexCrossing(c));
        }

        private void assertCrossings(S2Point a,
                                     S2Point b,
                                     S2Point c,
                                     S2Point d,
                                     int robust,
                                     bool edgeOrVertex,
                                     bool simple)
        {
            assertCrossing(a, b, c, d, robust, edgeOrVertex, simple);
            assertCrossing(b, a, c, d, robust, edgeOrVertex, simple);
            assertCrossing(a, b, d, c, robust, edgeOrVertex, simple);
            assertCrossing(b, a, d, c, robust, edgeOrVertex, simple);
            assertCrossing(a, a, c, d, DEGENERATE, false, false);
            assertCrossing(a, b, c, c, DEGENERATE, false, false);
            assertCrossing(a, b, a, b, 0, true, false);
            assertCrossing(c, d, a, b, robust, (edgeOrVertex ^ (robust == 0)), simple);
        }

        private S2LatLngRect getEdgeBound(double x1,
                                          double y1,
                                          double z1,
                                          double x2,
                                          double y2,
                                          double z2)
        {
            var bounder = new RectBounder();
            var p1 = S2Point.Normalize(new S2Point(x1, y1, z1));
            var p2 = S2Point.Normalize(new S2Point(x2, y2, z2));
            bounder.AddPoint(p1);
            bounder.AddPoint(p2);
            return bounder.Bound;
        }

        // Produce a normalized S2Point for testing.
        private S2Point S2NP(double x, double y, double z)
        {
            return S2Point.Normalize(new S2Point(x, y, z));
        }

        private void assertWedge(S2Point a0,
                                 S2Point ab1,
                                 S2Point a2,
                                 S2Point b0,
                                 S2Point b2,
                                 bool contains,
                                 bool intersects,
                                 bool crosses)
        {
            a0 = S2Point.Normalize(a0);
            ab1 = S2Point.Normalize(ab1);
            a2 = S2Point.Normalize(a2);
            b0 = S2Point.Normalize(b0);
            b2 = S2Point.Normalize(b2);

            assertEquals(new WedgeContains().Test(a0, ab1, a2, b0, b2), contains ? 1 : 0);
            assertEquals(new WedgeIntersects().Test(a0, ab1, a2, b0, b2), intersects ? -1 : 0);
            assertEquals(new WedgeContainsOrIntersects().Test(a0, ab1, a2, b0, b2),
                         contains ? 1 : intersects ? -1 : 0);
            assertEquals(new WedgeContainsOrCrosses().Test(a0, ab1, a2, b0, b2),
                         contains ? 1 : crosses ? -1 : 0);
        }

        // Given a point X and an edge AB, check that the distance from X to AB is
        // "distanceRadians" and the closest point on AB is "expectedClosest".
        private static void checkDistance(
            S2Point x, S2Point a, S2Point b, double distanceRadians, S2Point expectedClosest)
        {
            var kEpsilon = 1e-10;
            x = S2Point.Normalize(x);
            a = S2Point.Normalize(a);
            b = S2Point.Normalize(b);
            expectedClosest = S2Point.Normalize(expectedClosest);

            assertEquals(distanceRadians, S2EdgeUtil.GetDistance(x, a, b).Radians, kEpsilon);

            var closest = S2EdgeUtil.GetClosestPoint(x, a, b);
            if (expectedClosest.Equals(new S2Point(0, 0, 0)))
            {
                // This special value says that the result should be A or B.
                assertTrue(closest == a || closest == b);
            }
            else
            {
                assertTrue(S2.ApproxEquals(closest, expectedClosest));
            }
        }

        [TestMethod]
        public void testCrossings()
        {
            // The real tests of edge crossings are in s2{loop,polygon}_unittest,
            // but we do a few simple tests here.

            // Two regular edges that cross.
            assertCrossings(new S2Point(1, 2, 1),
                            new S2Point(1, -3, 0.5),
                            new S2Point(1, -0.5, -3),
                            new S2Point(0.1, 0.5, 3),
                            1,
                            true,
                            true);

            // Two regular edges that cross antipodal points.
            assertCrossings(new S2Point(1, 2, 1),
                            new S2Point(1, -3, 0.5),
                            new S2Point(-1, 0.5, 3),
                            new S2Point(-0.1, -0.5, -3),
                            -1,
                            false,
                            true);

            // Two edges on the same great circle.
            assertCrossings(new S2Point(0, 0, -1),
                            new S2Point(0, 1, 0),
                            new S2Point(0, 1, 1),
                            new S2Point(0, 0, 1),
                            -1,
                            false,
                            true);

            // Two edges that cross where one vertex is S2.Origin().
            assertCrossings(new S2Point(1, 0, 0),
                            new S2Point(0, 1, 0),
                            new S2Point(0, 0, 1),
                            new S2Point(1, 1, -1),
                            1,
                            true,
                            true);

            // Two edges that cross antipodal points where one vertex is S2.Origin().
            assertCrossings(new S2Point(1, 0, 0),
                            new S2Point(0, 1, 0),
                            new S2Point(0, 0, -1),
                            new S2Point(-1, -1, 1),
                            -1,
                            false,
                            true);

            // Two edges that share an endpoint. The Ortho() direction is (-4,0,2),
            // and edge CD is further CCW around (2,3,4) than AB.
            assertCrossings(new S2Point(2, 3, 4),
                            new S2Point(-1, 2, 5),
                            new S2Point(7, -2, 3),
                            new S2Point(2, 3, 4),
                            0,
                            false,
                            true);

            // Two edges that barely cross edge other.
            assertCrossings(new S2Point(1, 1, 1),
                            new S2Point(1, 1 - 1e-15, -1),
                            new S2Point(-1, -1, 0),
                            new S2Point(1, 1, 0),
                            1,
                            true,
                            false);
        }

        [TestMethod]
        public void testGetClosestPoint()
        {
            var kMargin = 1e-6;

            var a = S2LatLng.FromDegrees(-0.5, 0).ToPoint();
            var b = S2LatLng.FromDegrees(+0.5, 0).ToPoint();

            // On edge at end points.
            assertEquals(a, S2EdgeUtil.GetClosestPoint(a, a, b));
            assertEquals(b, S2EdgeUtil.GetClosestPoint(b, a, b));

            // On edge in between.
            var mid = S2LatLng.FromDegrees(0, 0).ToPoint();
            assertEquals(mid, S2EdgeUtil.GetClosestPoint(mid, a, b));

            // End points are closest
            assertEquals(a, S2EdgeUtil.GetClosestPoint(S2LatLng.FromDegrees(-1, 0).ToPoint(), a, b));
            assertEquals(b, S2EdgeUtil.GetClosestPoint(S2LatLng.FromDegrees(+1, 0).ToPoint(), a, b));

            // Intermediate point is closest.
            var x = S2LatLng.FromDegrees(+0.1, 1).ToPoint();
            var expectedClosestPoint = S2LatLng.FromDegrees(+0.1, 0).ToPoint();

            assertTrue(expectedClosestPoint.ApproxEquals(S2EdgeUtil.GetClosestPoint(x, a, b), kMargin));
        }

        [TestMethod]
        public void testGetDistance()
        {
            checkDistance(
                new S2Point(1, 0, 0), new S2Point(1, 0, 0), new S2Point(0, 1, 0), 0, new S2Point(1, 0, 0));
            checkDistance(
                new S2Point(0, 1, 0), new S2Point(1, 0, 0), new S2Point(0, 1, 0), 0, new S2Point(0, 1, 0));
            checkDistance(
                new S2Point(1, 3, 0), new S2Point(1, 0, 0), new S2Point(0, 1, 0), 0, new S2Point(1, 3, 0));
            checkDistance(new S2Point(0, 0, 1), new S2Point(1, 0, 0), new S2Point(0, 1, 0), Math.PI/2,
                          new S2Point(1, 0, 0));
            checkDistance(new S2Point(0, 0, -1), new S2Point(1, 0, 0), new S2Point(0, 1, 0), Math.PI/2,
                          new S2Point(1, 0, 0));
            checkDistance(new S2Point(-1, -1, 0), new S2Point(1, 0, 0), new S2Point(0, 1, 0),
                          0.75*Math.PI, new S2Point(0, 0, 0));
            checkDistance(new S2Point(0, 1, 0), new S2Point(1, 0, 0), new S2Point(1, 1, 0), Math.PI/4,
                          new S2Point(1, 1, 0));
            checkDistance(new S2Point(0, -1, 0), new S2Point(1, 0, 0), new S2Point(1, 1, 0), Math.PI/2,
                          new S2Point(1, 0, 0));
            checkDistance(new S2Point(0, -1, 0), new S2Point(1, 0, 0), new S2Point(-1, 1, 0), Math.PI/2,
                          new S2Point(1, 0, 0));
            checkDistance(new S2Point(-1, -1, 0), new S2Point(1, 0, 0), new S2Point(-1, 1, 0), Math.PI/2,
                          new S2Point(-1, 1, 0));
            checkDistance(new S2Point(1, 1, 1), new S2Point(1, 0, 0), new S2Point(0, 1, 0),
                          Math.Asin(Math.Sqrt(1.0/3)), new S2Point(1, 1, 0));
            checkDistance(new S2Point(1, 1, -1), new S2Point(1, 0, 0), new S2Point(0, 1, 0),
                          Math.Asin(Math.Sqrt(1.0/3)), new S2Point(1, 1, 0));
            checkDistance(new S2Point(-1, 0, 0), new S2Point(1, 1, 0), new S2Point(1, 1, 0), 0.75*Math.PI,
                          new S2Point(1, 1, 0));
            checkDistance(new S2Point(0, 0, -1), new S2Point(1, 1, 0), new S2Point(1, 1, 0), Math.PI/2,
                          new S2Point(1, 1, 0));
            checkDistance(new S2Point(-1, 0, 0), new S2Point(1, 0, 0), new S2Point(1, 0, 0), Math.PI,
                          new S2Point(1, 0, 0));
        }

        [TestMethod]
        public void testIntersectionTolerance()
        {
            // We repeatedly construct two edges that cross near a random point "p",
            // and measure the distance from the actual intersection point "x" to the
            // the expected intersection point "p" and also to the edges that cross
            // near "p".
            //
            // Note that getIntersection() does not guarantee that "x" and "p" will be
            // close together (since the intersection point is numerically unstable
            // when the edges cross at a very small angle), but it does guarantee that
            // "x" will be close to both of the edges that cross.
            var maxPointDist = new S1Angle();
            var maxEdgeDist = new S1Angle();

            for (var i = 0; i < 1000; ++i)
            {
                // We construct two edges AB and CD that intersect near "p". The angle
                // between AB and CD (expressed as a slope) is chosen randomly between
                // 1e-15 and 1.0 such that its logarithm is uniformly distributed. This
                // implies that small values are much more likely to be chosen.
                //
                // Once the slope is chosen, the four points ABCD must be offset from P
                // by at least (1e-15 / slope) so that the points are guaranteed to have
                // the correct circular ordering around P. This is the distance from P
                // at which the two edges are separated by about 1e-15, which is
                // approximately the minimum distance at which we can expect computed
                // points on the two lines to be distinct and have the correct ordering.
                //
                // The actual offset distance from P is chosen randomly in the range
                // [1e-15 / slope, 1.0], again uniformly distributing the logarithm.
                // This ensures that we test both long and very short segments that
                // intersect at both large and very small angles.

                var points = getRandomFrame();
                var p = points[0];
                var d1 = points[1];
                var d2 = points[2];
                var slope = Math.Pow(1e-15, rand.NextDouble());
                d2 = d1 + (d2 * slope);
                var a = S2Point.Normalize(p + (d1 * Math.Pow(1e-15/slope, rand.NextDouble())));
                var b = S2Point.Normalize(p - (d1 * Math.Pow(1e-15/slope, rand.NextDouble())));
                var c = S2Point.Normalize(p + (d2 * Math.Pow(1e-15/slope, rand.NextDouble())));
                var d = S2Point.Normalize(p - (d2 * Math.Pow(1e-15/slope, rand.NextDouble())));
                var x = S2EdgeUtil.GetIntersection(a, b, c, d);
                var distAb = S2EdgeUtil.GetDistance(x, a, b);
                var distCd = S2EdgeUtil.GetDistance(x, c, d);

                assertTrue(distAb < S2EdgeUtil.DefaultIntersectionTolerance);
                assertTrue(distCd < S2EdgeUtil.DefaultIntersectionTolerance);

                // test getIntersection() post conditions
                assertTrue(S2.OrderedCcw(a, x, b, S2Point.Normalize(S2.RobustCrossProd(a, b))));
                assertTrue(S2.OrderedCcw(c, x, d, S2Point.Normalize(S2.RobustCrossProd(c, d))));

                maxEdgeDist = S1Angle.Max(maxEdgeDist, S1Angle.Max(distAb, distCd));
                maxPointDist = S1Angle.Max(maxPointDist, new S1Angle(p, x));
            }
        }

        [TestMethod]
        public void testLongitudePruner()
        {
            var pruner1 = new LongitudePruner(
                new S1Interval(0.75*S2.Pi, -0.75*S2.Pi), new S2Point(0, 1, 2));

            assertFalse(pruner1.Intersects(new S2Point(1, 1, 3)));
            assertTrue(pruner1.Intersects(new S2Point(-1 - 1e-15, -1, 0)));
            assertTrue(pruner1.Intersects(new S2Point(-1, 0, 0)));
            assertTrue(pruner1.Intersects(new S2Point(-1, 0, 0)));
            assertTrue(pruner1.Intersects(new S2Point(1, -1, 8)));
            assertFalse(pruner1.Intersects(new S2Point(1, 0, -2)));
            assertTrue(pruner1.Intersects(new S2Point(-1, -1e-15, 0)));

            var pruner2 = new LongitudePruner(
                new S1Interval(0.25*S2.Pi, 0.25*S2.Pi), new S2Point(1, 0, 0));

            assertFalse(pruner2.Intersects(new S2Point(2, 1, 2)));
            assertTrue(pruner2.Intersects(new S2Point(1, 2, 3)));
            assertFalse(pruner2.Intersects(new S2Point(0, 1, 4)));
            assertFalse(pruner2.Intersects(new S2Point(-1e-15, -1, -1)));
        }

        [TestMethod]
        public void testRectBounder()
        {
            // Check cases where min/max latitude is not at a vertex.
            // Max, CW
            assertDoubleNear(getEdgeBound(1, 1, 1, 1, -1, 1).Lat.Hi, S2.PiOver4);
            // Max, CCW
            assertDoubleNear(getEdgeBound(1, -1, 1, 1, 1, 1).Lat.Hi, S2.PiOver4);
            // Min, CW
            assertDoubleNear(getEdgeBound(1, -1, -1, -1, -1, -1).Lat.Lo, -S2.PiOver4);
            // Min, CCW
            assertDoubleNear(getEdgeBound(-1, 1, -1, -1, -1, -1).Lat.Lo, -S2.PiOver4);

            // Check cases where the edge passes through one of the poles.
            assertDoubleNear(getEdgeBound(.3, .4, 1, -.3, -.4, 1).Lat.Hi, S2.PiOver2);
            assertDoubleNear(getEdgeBound(.3, .4, -1, -.3, -.4, -1).Lat.Lo, -S2.PiOver2);

            // Check cases where the min/max latitude is attained at a vertex.
            var kCubeLat = Math.Asin(Math.Sqrt(1.0/3)); // 35.26 degrees
            assertTrue(
                getEdgeBound(1, 1, 1, 1, -1, -1).Lat.ApproxEquals(new R1Interval(-kCubeLat, kCubeLat)));
            assertTrue(
                getEdgeBound(1, -1, 1, 1, 1, -1).Lat.ApproxEquals(new R1Interval(-kCubeLat, kCubeLat)));
        }

        [TestMethod]
        public void testWedges()
        {
            // For simplicity, all of these tests use an origin of (0, 0, 1).
            // This shouldn't matter as long as the lower-level primitives are
            // implemented correctly.

            // Intersection in one wedge.
            assertWedge(new S2Point(-1, 0, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(1, 2, 10),
                        new S2Point(0, 1, 10),
                        new S2Point(1, -2, 10),
                        false,
                        true,
                        true);
            // Intersection in two wedges.
            assertWedge(new S2Point(-1, -1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(1, -1, 10),
                        new S2Point(1, 0, 10),
                        new S2Point(-1, 1, 10),
                        false,
                        true,
                        true);

            // Normal containment.
            assertWedge(new S2Point(-1, -1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(1, -1, 10),
                        new S2Point(-1, 0, 10),
                        new S2Point(1, 0, 10),
                        true,
                        true,
                        false);
            // Containment with equality on one side.
            assertWedge(new S2Point(2, 1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(-1, -1, 10),
                        new S2Point(2, 1, 10),
                        new S2Point(1, -5, 10),
                        true,
                        true,
                        false);
            // Containment with equality on the other side.
            assertWedge(new S2Point(2, 1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(-1, -1, 10),
                        new S2Point(1, -2, 10),
                        new S2Point(-1, -1, 10),
                        true,
                        true,
                        false);
            // Containment with equality on both sides.
            assertWedge(new S2Point(-2, 3, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(4, -5, 10),
                        new S2Point(-2, 3, 10),
                        new S2Point(4, -5, 10),
                        true,
                        true,
                        false);

            // Disjoint with equality on one side.
            assertWedge(new S2Point(-2, 3, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(4, -5, 10),
                        new S2Point(4, -5, 10),
                        new S2Point(-2, -3, 10),
                        false,
                        false,
                        false);
            // Disjoint with equality on the other side.
            assertWedge(new S2Point(-2, 3, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(0, 5, 10),
                        new S2Point(4, -5, 10),
                        new S2Point(-2, 3, 10),
                        false,
                        false,
                        false);
            // Disjoint with equality on both sides.
            assertWedge(new S2Point(-2, 3, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(4, -5, 10),
                        new S2Point(4, -5, 10),
                        new S2Point(-2, 3, 10),
                        false,
                        false,
                        false);

            // B contains A with equality on one side.
            assertWedge(new S2Point(2, 1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(1, -5, 10),
                        new S2Point(2, 1, 10),
                        new S2Point(-1, -1, 10),
                        false,
                        true,
                        false);
            // B contains A with equality on the other side.
            assertWedge(new S2Point(2, 1, 10),
                        new S2Point(0, 0, 1),
                        new S2Point(1, -5, 10),
                        new S2Point(-2, 1, 10),
                        new S2Point(1, -5, 10),
                        false,
                        true,
                        false);
        }

        [TestMethod]
        public void testXYZPruner()
        {
            var pruner = new XyzPruner();

            // We aren't actually normalizing these points but it doesn't
            // matter too much as long as we are reasonably close to unit vectors.
            // This is a simple triangle on the equator.
            pruner.AddEdgeToBounds(S2NP(0, 1, 0), S2NP(0.1, 1, 0));
            pruner.AddEdgeToBounds(S2NP(0.1, 1, 0), S2NP(0.1, 1, 0.1));
            pruner.AddEdgeToBounds(S2NP(0.1, 1, 0.1), S2NP(0, 1, 0));

            // try a loop around the triangle but far enough out to not overlap.
            pruner.SetFirstIntersectPoint(S2NP(-0.1, 1.0, 0.0));
            assertFalse(pruner.Intersects(S2NP(-0.1, 1.0, 0.2)));
            assertFalse(pruner.Intersects(S2NP(0.0, 1.0, 0.2)));
            assertFalse(pruner.Intersects(S2NP(0.2, 1.0, 0.2)));
            assertFalse(pruner.Intersects(S2NP(0.2, 1.0, 0.05)));
            assertFalse(pruner.Intersects(S2NP(0.2, 1.0, -0.1)));
            assertFalse(pruner.Intersects(S2NP(-0.1, 1.0, -0.1)));
            assertFalse(pruner.Intersects(S2NP(-0.1, 1.0, 0.0)));

            // now we go to a point in the bounding box of the triangle but well
            // out of the loop. This will be a hit even though it really does not
            // need to be.
            assertTrue(pruner.Intersects(S2NP(0.02, 1.0, 0.04)));

            // now we zoom out to do an edge *just* below the triangle. This should
            // be a hit because we are within the deformation zone.
            assertTrue(pruner.Intersects(S2NP(-0.1, 1.0, -0.03)));
            assertFalse(pruner.Intersects(S2NP(0.05, 1.0, -0.03))); // not close
            assertTrue(pruner.Intersects(S2NP(0.05, 1.0, -0.01))); // close
            assertTrue(pruner.Intersects(S2NP(0.05, 1.0, 0.13)));
            assertFalse(pruner.Intersects(S2NP(0.13, 1.0, 0.14)));

            // Create a new pruner with very small area and correspondingly narrow
            // deformation tolerances.
            var spruner = new XyzPruner();
            spruner.AddEdgeToBounds(S2NP(0, 1, 0.000), S2NP(0.001, 1, 0));
            spruner.AddEdgeToBounds(S2NP(0.001, 1, 0.000), S2NP(0.001, 1, 0.001));
            spruner.AddEdgeToBounds(S2NP(0.001, 1, 0.001), S2NP(0.000, 1, 0));

            spruner.SetFirstIntersectPoint(S2NP(0, 1.0, -0.1));
            assertFalse(spruner.Intersects(S2NP(0.0005, 1.0, -0.0005)));
            assertFalse(spruner.Intersects(S2NP(0.0005, 1.0, -0.0005)));
            assertFalse(spruner.Intersects(S2NP(0.0005, 1.0, -0.00001)));
            assertTrue(spruner.Intersects(S2NP(0.0005, 1.0, -0.0000001)));
        }
    }
}