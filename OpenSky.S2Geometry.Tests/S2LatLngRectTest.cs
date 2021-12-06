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
    public class S2LatLngRectTest : GeometryTestCase
    {
        public void testIntervalOps(S2LatLngRect x, S2LatLngRect y, String expectedRelation,
                                    S2LatLngRect expectedUnion, S2LatLngRect expectedIntersection)
        {
            // Test all of the interval operations on the given pair of intervals.
            // "expected_relation" is a sequence of "T" and "F" characters corresponding
            // to the expected results of Contains(), InteriorContains(), Intersects(),
            // and InteriorIntersects() respectively.

            assertEquals(x.Contains(y), expectedRelation[0] == 'T');
            assertEquals(x.InteriorContains(y), expectedRelation[1] == 'T');
            assertEquals(x.Intersects(y), expectedRelation[2] == 'T');
            assertEquals(x.InteriorIntersects(y), expectedRelation[3] == 'T');

            assertEquals(x.Contains(y), x.Union(y).Equals(x));
            assertEquals(x.Intersects(y), !x.Intersection(y).IsEmpty);

            assertTrue(x.Union(y).Equals(expectedUnion));
            assertTrue(x.Intersection(y).Equals(expectedIntersection));

            if (y.Size == S2LatLng.FromRadians(0, 0))
            {
                var r = x.AddPoint(y.Lo);
                assertTrue(r == expectedUnion);
            }
        }

        public void testCellOps(S2LatLngRect r, S2Cell cell, int level)
        {
            // Test the relationship between the given rectangle and cell:
            // 0 == no intersection, 1 == MayIntersect, 2 == Intersects,
            // 3 == Vertex Containment, 4 == Contains

            var vertexContained = false;
            for (var i = 0; i < 4; ++i)
            {
                if (r.Contains(cell.GetVertexRaw(i))
                    || (!r.IsEmpty && cell.Contains(r.GetVertex(i).ToPoint())))
                {
                    vertexContained = true;
                }
            }
            assertEquals(r.MayIntersect(cell), level >= 1);
            assertEquals(r.Intersects(cell), level >= 2);
            assertEquals(vertexContained, level >= 3);
            assertEquals(r.Contains(cell), level >= 4);
        }

        private static S1Angle bruteForceDistance(S2LatLngRect a, S2LatLngRect b)
        {
            if (a.Intersects(b))
            {
                return S1Angle.FromRadians(0);
            }

            // Compare every point in 'a' against every latitude edge and longitude edge
            // in 'b', and vice-versa, for a total of 16 point-vs-latitude-edge tests
            // and 16 point-vs-longitude-edge tests.
            S2LatLng[] pntA =
            {
                new S2LatLng(a.LatLo, a.LngLo), new S2LatLng(a.LatLo, a.LngHi),
                new S2LatLng(a.LatHi, a.LngHi), new S2LatLng(a.LatHi, a.LngLo)
            };
            S2LatLng[] pntB =
            {
                new S2LatLng(b.LatLo, b.LngLo), new S2LatLng(b.LatLo, b.LngHi),
                new S2LatLng(b.LatHi, b.LngHi), new S2LatLng(b.LatHi, b.LngLo)
            };

            // Make arrays containing the lo/hi latitudes and the lo/hi longitude edges.
            S1Angle[] latA = {a.LatLo, a.LatHi};
            S1Angle[] latB = {b.LatLo, b.LatHi};
            S2Point[][] lng_edge_a =
            {new[] {pntA[0].ToPoint(), pntA[3].ToPoint()}, new[] {pntA[1].ToPoint(), pntA[2].ToPoint()}};
            S2Point[][] lng_edge_b =
            {new[] {pntB[0].ToPoint(), pntB[3].ToPoint()}, new[] {pntB[1].ToPoint(), pntB[2].ToPoint()}};

            var minDistance = S1Angle.FromDegrees(180.0);
            for (var i = 0; i < 4; ++i)
            {
                // For each point in a and b.
                var currentA = pntA[i];
                var currentB = pntB[i];

                for (var j = 0; j < 2; ++j)
                {
                    // Get distances to latitude and longitude edges.
                    var aToLat = getDistance(currentA, latB[j], b.Lng);
                    var bToLat = getDistance(currentB, latA[j], a.Lng);
                    var aToLng =
                        S2EdgeUtil.GetDistance(currentA.ToPoint(), lng_edge_b[j][0], lng_edge_b[j][1]);
                    var bToLng =
                        S2EdgeUtil.GetDistance(currentB.ToPoint(), lng_edge_a[j][0], lng_edge_a[j][1]);

                    minDistance = S1Angle.Min(
                        minDistance, S1Angle.Min(aToLat, S1Angle.Min(bToLat, S1Angle.Min(aToLng, bToLng))));
                }
            }
            return minDistance;
        }

        private static S1Angle bruteForceRectPointDistance(S2LatLngRect a, S2LatLng b)
        {
            if (a.Contains(b))
            {
                return S1Angle.FromRadians(0);
            }

            var bToLoLat = getDistance(b, a.LatLo, a.Lng);
            var bToHiLat = getDistance(b, a.LatHi, a.Lng);
            var bToLoLng =
                S2EdgeUtil.GetDistance(b.ToPoint(), new S2LatLng(a.LatLo, a.LngLo).ToPoint(),
                                       new S2LatLng(a.LatHi, a.LngLo).ToPoint());
            var bToHiLng =
                S2EdgeUtil.GetDistance(b.ToPoint(), new S2LatLng(a.LatLo, a.LngHi).ToPoint(),
                                       new S2LatLng(a.LatHi, a.LngHi).ToPoint());
            return S1Angle.Min(bToLoLat, S1Angle.Min(bToHiLat, S1Angle.Min(bToLoLng, bToHiLng)));
        }

        /**
         * Returns the minimum distance from X to the latitude line segment defined by
         * the given latitude and longitude interval.
         */

        private static S1Angle getDistance(S2LatLng x, S1Angle lat, S1Interval interval)
        {
            assertTrue(x.IsValid);
            assertTrue(interval.IsValid);

            // Is X inside the longitude interval?
            if (interval.Contains(x.Lng.Radians))
                return S1Angle.FromRadians(Math.Abs(x.Lat.Radians - lat.Radians));

            // Return the distance to the closer endpoint.
            return S1Angle.Min(x.GetDistance(new S2LatLng(lat, S1Angle.FromRadians(interval.Lo))),
                               x.GetDistance(new S2LatLng(lat, S1Angle.FromRadians(interval.Hi))));
        }

        private static S2LatLngRect getEdgeBound(double x1,
                                                 double y1,
                                                 double z1,
                                                 double x2,
                                                 double y2,
                                                 double z2)
        {
            return S2LatLngRect.FromEdge(
                S2Point.Normalize(new S2Point(x1, y1, z1)), S2Point.Normalize(new S2Point(x2, y2, z2)));
        }

        private static S2LatLngRect pointRectFromDegrees(double lat, double lng)
        {
            return S2LatLngRect.FromPoint(S2LatLng.FromDegrees(lat, lng).Normalized);
        }

        private static S2LatLngRect rectFromDegrees(
            double latLo, double lngLo, double latHi, double lngHi)
        {
            // Convenience method to construct a rectangle. This method is
            // intentionally *not* in the S2LatLngRect interface because the
            // argument order is ambiguous, but hopefully it's not too confusing
            // within the context of this unit test.

            return new S2LatLngRect(S2LatLng.FromDegrees(latLo, lngLo).Normalized,
                                    S2LatLng.FromDegrees(latHi, lngHi).Normalized);
        }

        /**
         * This method verifies a.getDistance(b), where b is a S2LatLng, by comparing
         * its result against a.getDistance(c), c being the point rectangle created
         * from b.
         */

        private static void verifyGetRectPointDistance(S2LatLngRect a, S2LatLng p)
        {
            var distance1 = bruteForceRectPointDistance(a, p.Normalized);
            var distance2 = a.GetDistance(p.Normalized);
            assertEquals(distance1.Radians, distance2.Radians, 1e-10);
        }

        /**
         * This method verifies a.getDistance(b) by comparing its result against a
         * brute-force implementation. The correctness of the brute-force version is
         * much easier to verify by inspection.
         */

        private static void verifyGetDistance(S2LatLngRect a, S2LatLngRect b)
        {
            var distance1 = bruteForceDistance(a, b);
            var distance2 = a.GetDistance(b);
            assertEquals(distance1.Radians, distance2.Radians, 1e-10);
        }

        [TestMethod]
        public void testArea()
        {
            assertEquals(0.0, S2LatLngRect.Empty.Area);
            assertDoubleNear(4*Math.PI, S2LatLngRect.Full.Area);
            assertDoubleNear(Math.PI/2, rectFromDegrees(0, 0, 90, 90).Area);
        }

        [TestMethod]
        public void testBasic()
        {
            // Most of the S2LatLngRect methods have trivial implementations that
            // use the R1Interval and S1Interval classes, so most of the testing
            // is done in those unit tests.

            // Test basic properties of empty and full caps.
            var empty = S2LatLngRect.Empty;
            var full = S2LatLngRect.Full;
            assertTrue(empty.IsValid);
            assertTrue(empty.IsEmpty);
            assertTrue(full.IsValid);
            assertTrue(full.IsFull);

            // assertTrue various constructors and accessor methods.
            var d1 = rectFromDegrees(-90, 0, -45, 180);
            assertDoubleNear(d1.LatLo.Degrees, -90);
            assertDoubleNear(d1.LatHi.Degrees, -45);
            assertDoubleNear(d1.LngLo.Degrees, 0);
            assertDoubleNear(d1.LngHi.Degrees, 180);
            assertTrue(d1.Lat.Equals(new R1Interval(-S2.PiOver2, -S2.PiOver4)));
            assertTrue(d1.Lng.Equals(new S1Interval(0, S2.Pi)));

            // FromCenterSize()
            assertTrue(
                S2LatLngRect.FromCenterSize(S2LatLng.FromDegrees(80, 170), S2LatLng.FromDegrees(40, 60))
                            .ApproxEquals(rectFromDegrees(60, 140, 90, -160)));
            assertTrue(S2LatLngRect
                           .FromCenterSize(S2LatLng.FromDegrees(10, 40), S2LatLng.FromDegrees(210, 400)).IsFull);
            assertTrue(
                S2LatLngRect.FromCenterSize(S2LatLng.FromDegrees(-90, 180), S2LatLng.FromDegrees(20, 50))
                            .ApproxEquals(rectFromDegrees(-90, 155, -80, -155)));

            // FromPoint(), FromPointPair()
            assertEquals(S2LatLngRect.FromPoint(d1.Lo), new S2LatLngRect(d1.Lo, d1.Lo));
            assertEquals(
                S2LatLngRect.FromPointPair(S2LatLng.FromDegrees(-35, -140), S2LatLng.FromDegrees(15, 155)),
                rectFromDegrees(-35, 155, 15, -140));
            assertEquals(
                S2LatLngRect.FromPointPair(S2LatLng.FromDegrees(25, -70), S2LatLng.FromDegrees(-90, 80)),
                rectFromDegrees(-90, -70, 25, 80));

            // GetCenter(), GetVertex(), Contains(S2LatLng), InteriorContains(S2LatLng).
            var eqM180 = S2LatLng.FromRadians(0, -S2.Pi);
            var northPole = S2LatLng.FromRadians(S2.PiOver2, 0);
            var r1 = new S2LatLngRect(eqM180, northPole);

            assertEquals(r1.Center, S2LatLng.FromRadians(S2.PiOver4, -S2.PiOver2));
            assertEquals(r1.GetVertex(0), S2LatLng.FromRadians(0, S2.Pi));
            assertEquals(r1.GetVertex(1), S2LatLng.FromRadians(0, 0));
            assertEquals(r1.GetVertex(2), S2LatLng.FromRadians(S2.PiOver2, 0));
            assertEquals(r1.GetVertex(3), S2LatLng.FromRadians(S2.PiOver2, S2.Pi));
            assertTrue(r1.Contains(S2LatLng.FromDegrees(30, -45)));
            assertTrue(!r1.Contains(S2LatLng.FromDegrees(30, 45)));
            assertTrue(!r1.InteriorContains(eqM180) && !r1.InteriorContains(northPole));
            assertTrue(r1.Contains(new S2Point(0.5, -0.3, 0.1)));
            assertTrue(!r1.Contains(new S2Point(0.5, 0.2, 0.1)));

            // Make sure that GetVertex() returns vertices in CCW order.
            for (var i = 0; i < 4; ++i)
            {
                var lat = S2.PiOver4*(i - 2);
                var lng = S2.PiOver2*(i - 2) + 0.2;
                var r = new S2LatLngRect(new R1Interval(lat, lat + S2.PiOver4), new S1Interval(
                                                                                   Math.IEEERemainder(lng, 2*S2.Pi), Math.IEEERemainder(lng + S2.PiOver2, 2*S2.Pi)));
                for (var k = 0; k < 4; ++k)
                {
                    assertTrue(
                        S2.SimpleCcw(r.GetVertex((k - 1) & 3).ToPoint(), r.GetVertex(k).ToPoint(),
                                     r.GetVertex((k + 1) & 3).ToPoint()));
                }
            }

            // Contains(S2LatLngRect), InteriorContains(S2LatLngRect),
            // Intersects(), InteriorIntersects(), Union(), Intersection().
            //
            // Much more testing of these methods is done in s1interval_unittest
            // and r1interval_unittest.

            var r1Mid = rectFromDegrees(45, -90, 45, -90);
            var reqM180 = new S2LatLngRect(eqM180, eqM180);
            var rNorthPole = new S2LatLngRect(northPole, northPole);

            testIntervalOps(r1, r1Mid, "TTTT", r1, r1Mid);
            testIntervalOps(r1, reqM180, "TFTF", r1, reqM180);
            testIntervalOps(r1, rNorthPole, "TFTF", r1, rNorthPole);

            assertTrue(r1.Equals(rectFromDegrees(0, -180, 90, 0)));
            testIntervalOps(r1, rectFromDegrees(-10, -1, 1, 20), "FFTT", rectFromDegrees(-10, -180, 90, 20),
                            rectFromDegrees(0, -1, 1, 0));
            testIntervalOps(r1, rectFromDegrees(-10, -1, 0, 20), "FFTF", rectFromDegrees(-10, -180, 90, 20),
                            rectFromDegrees(0, -1, 0, 0));
            testIntervalOps(r1, rectFromDegrees(-10, 0, 1, 20), "FFTF", rectFromDegrees(-10, -180, 90, 20),
                            rectFromDegrees(0, 0, 1, 0));

            testIntervalOps(rectFromDegrees(-15, -160, -15, -150), rectFromDegrees(20, 145, 25, 155),
                            "FFFF", rectFromDegrees(-15, 145, 25, -150), empty);
            testIntervalOps(rectFromDegrees(70, -10, 90, -140), rectFromDegrees(60, 175, 80, 5), "FFTT",
                            rectFromDegrees(60, -180, 90, 180), rectFromDegrees(70, 175, 80, 5));

            // assertTrue that the intersection of two rectangles that overlap in
            // latitude
            // but not longitude is valid, and vice versa.
            testIntervalOps(rectFromDegrees(12, 30, 60, 60), rectFromDegrees(0, 0, 30, 18), "FFFF",
                            rectFromDegrees(0, 0, 60, 60), empty);
            testIntervalOps(rectFromDegrees(0, 0, 18, 42), rectFromDegrees(30, 12, 42, 60), "FFFF",
                            rectFromDegrees(0, 0, 42, 60), empty);

            // AddPoint()
            var p = S2LatLngRect.Empty;
            p = p.AddPoint(S2LatLng.FromDegrees(0, 0));
            p = p.AddPoint(S2LatLng.FromRadians(0, -S2.PiOver2));
            p = p.AddPoint(S2LatLng.FromRadians(S2.PiOver4, -S2.Pi));
            p = p.AddPoint(new S2Point(0, 0, 1));
            assertTrue(p.Equals(r1));

            // Expanded()
            assertTrue(
                rectFromDegrees(70, 150, 80, 170).Expanded(S2LatLng.FromDegrees(20, 30)).ApproxEquals(
                    rectFromDegrees(50, 120, 90, -160)));
            assertTrue(S2LatLngRect.Empty.Expanded(S2LatLng.FromDegrees(20, 30)).IsEmpty);
            assertTrue(S2LatLngRect.Full.Expanded(S2LatLng.FromDegrees(20, 30)).IsFull);
            assertTrue(
                rectFromDegrees(-90, 170, 10, 20).Expanded(S2LatLng.FromDegrees(30, 80)).ApproxEquals(
                    rectFromDegrees(-90, -180, 40, 180)));

            // ConvolveWithCap()
            var llr1 =
                new S2LatLngRect(S2LatLng.FromDegrees(0, 170), S2LatLng.FromDegrees(0, -170))
                    .ConvolveWithCap(S1Angle.FromDegrees(15));
            var llr2 =
                new S2LatLngRect(S2LatLng.FromDegrees(-15, 155), S2LatLng.FromDegrees(15, -155));
            assertTrue(llr1.ApproxEquals(llr2));

            llr1 = new S2LatLngRect(S2LatLng.FromDegrees(60, 150), S2LatLng.FromDegrees(80, 10))
                .ConvolveWithCap(S1Angle.FromDegrees(15));
            llr2 = new S2LatLngRect(S2LatLng.FromDegrees(45, -180), S2LatLng.FromDegrees(90, 180));
            assertTrue(llr1.ApproxEquals(llr2));

            // GetCapBound(), bounding cap at center is smaller:
            assertTrue(new S2LatLngRect(S2LatLng.FromDegrees(-45, -45), S2LatLng.FromDegrees(45, 45)).CapBound.ApproxEquals(S2Cap.FromAxisHeight(new S2Point(1, 0, 0), 0.5)));
            // GetCapBound(), bounding cap at north pole is smaller:
            assertTrue(new S2LatLngRect(S2LatLng.FromDegrees(88, -80), S2LatLng.FromDegrees(89, 80)).CapBound.ApproxEquals(S2Cap.FromAxisAngle(new S2Point(0, 0, 1), S1Angle.FromDegrees(2))));
            // GetCapBound(), longitude span > 180 degrees:
            assertTrue(
                new S2LatLngRect(S2LatLng.FromDegrees(-30, -150), S2LatLng.FromDegrees(-10, 50)).CapBound
                    .ApproxEquals(S2Cap.FromAxisAngle(new S2Point(0, 0, -1), S1Angle.FromDegrees(80))));

            // Contains(S2Cell), MayIntersect(S2Cell), Intersects(S2Cell)

            // Special cases.
            testCellOps(empty, S2Cell.FromFacePosLevel(3, (byte)0, 0), 0);
            testCellOps(full, S2Cell.FromFacePosLevel(2, (byte)0, 0), 4);
            testCellOps(full, S2Cell.FromFacePosLevel(5, (byte)0, 25), 4);

            // This rectangle includes the first quadrant of face 0. It's expanded
            // slightly because cell bounding rectangles are slightly conservative.
            var r4 = rectFromDegrees(-45.1, -45.1, 0.1, 0.1);
            testCellOps(r4, S2Cell.FromFacePosLevel(0, (byte)0, 0), 3);
            testCellOps(r4, S2Cell.FromFacePosLevel(0, (byte)0, 1), 4);
            testCellOps(r4, S2Cell.FromFacePosLevel(1, (byte)0, 1), 0);

            // This rectangle intersects the first quadrant of face 0.
            var r5 = rectFromDegrees(-10, -45, 10, 0);
            testCellOps(r5, S2Cell.FromFacePosLevel(0, (byte)0, 0), 3);
            testCellOps(r5, S2Cell.FromFacePosLevel(0, (byte)0, 1), 3);
            testCellOps(r5, S2Cell.FromFacePosLevel(1, (byte)0, 1), 0);

            // Rectangle consisting of a single point.
            testCellOps(rectFromDegrees(4, 4, 4, 4), S2Cell.FromFacePosLevel(0, (byte)0, 0), 3);

            // Rectangles that intersect the bounding rectangle of a face
            // but not the face itself.
            testCellOps(rectFromDegrees(41, -87, 42, -79), S2Cell.FromFacePosLevel(2, (byte)0, 0), 1);
            testCellOps(rectFromDegrees(-41, 160, -40, -160), S2Cell.FromFacePosLevel(5, (byte)0, 0), 1);
            {
                // This is the leaf cell at the top right hand corner of face 0.
                // It has two angles of 60 degrees and two of 120 degrees.
                var cell0tr = new S2Cell(new S2Point(1 + 1e-12, 1, 1));
                var bound0tr = cell0tr.RectBound;
                var v0 = new S2LatLng(cell0tr.GetVertexRaw(0));
                testCellOps(
                    rectFromDegrees(v0.Lat.Degrees - 1e-8, v0.Lng.Degrees - 1e-8,
                                    v0.Lat.Degrees - 2e-10, v0.Lng.Degrees + 1e-10), cell0tr, 1);
            }

            // Rectangles that intersect a face but where no vertex of one region
            // is contained by the other region. The first one passes through
            // a corner of one of the face cells.
            testCellOps(rectFromDegrees(-37, -70, -36, -20), S2Cell.FromFacePosLevel(5, (byte)0, 0), 2);
            {
                // These two intersect like a diamond and a square.
                var cell202 = S2Cell.FromFacePosLevel(2, (byte)0, 2);
                var bound202 = cell202.RectBound;
                testCellOps(
                    rectFromDegrees(bound202.Lo.Lat.Degrees + 3, bound202.Lo.Lng.Degrees + 3,
                                    bound202.Hi.Lat.Degrees - 3, bound202.Hi.Lng.Degrees - 3), cell202, 2);
            }
        }

        [TestMethod]
        public void testEdgeBound()
        {
            // assertTrue cases where min/max latitude is not at a vertex.
            assertDoubleNear(getEdgeBound(1, 1, 1, 1, -1, 1).Lat.Hi, S2.PiOver4); // Max,
            // CW
            assertDoubleNear(getEdgeBound(1, -1, 1, 1, 1, 1).Lat.Hi, S2.PiOver4); // Max,
            // CCW
            assertDoubleNear(getEdgeBound(1, -1, -1, -1, -1, -1).Lat.Lo, -S2.PiOver4); // Min,
            // CW
            assertDoubleNear(getEdgeBound(-1, 1, -1, -1, -1, -1).Lat.Lo, -S2.PiOver4); // Min,
            // CCW

            // assertTrue cases where the edge passes through one of the poles.
            assertDoubleNear(getEdgeBound(.3, .4, 1, -.3, -.4, 1).Lat.Hi, S2.PiOver2);
            assertDoubleNear(getEdgeBound(.3, .4, -1, -.3, -.4, -1).Lat.Lo, -S2.PiOver2);

            // assertTrue cases where the min/max latitude is attained at a vertex.
            var kCubeLat = Math.Asin(Math.Sqrt(1.0/3)); // 35.26 degrees
            assertTrue(
                getEdgeBound(1, 1, 1, 1, -1, -1).Lat.ApproxEquals(new R1Interval(-kCubeLat, kCubeLat)));
            assertTrue(
                getEdgeBound(1, -1, 1, 1, 1, -1).Lat.ApproxEquals(new R1Interval(-kCubeLat, kCubeLat)));
        }

        [TestMethod]
        public void testGetDistanceOverlapping()
        {
            // Check pairs of rectangles that overlap: (should all return 0):
            var a = rectFromDegrees(0, 0, 2, 2);
            var b = pointRectFromDegrees(0, 0);
            var zero = S1Angle.FromRadians(0);
            assertEquals(zero, a.GetDistance(a));
            assertEquals(zero, a.GetDistance(b));
            assertEquals(zero, b.GetDistance(b));
            assertEquals(zero, a.GetDistance(S2LatLng.FromDegrees(0, 0)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(0, 1, 2, 3)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(0, 2, 2, 4)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(1, 0, 3, 2)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(2, 0, 4, 2)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(1, 1, 3, 3)));
            assertEquals(zero, a.GetDistance(rectFromDegrees(2, 2, 4, 4)));
        }

        [TestMethod]
        public void testGetDistanceRandomPairs()
        {
            // Test random pairs.
            for (var i = 0; i < 10000; ++i)
            {
                var a =
                    S2LatLngRect.FromPointPair(new S2LatLng(randomPoint()), new S2LatLng(randomPoint()));
                var b =
                    S2LatLngRect.FromPointPair(new S2LatLng(randomPoint()), new S2LatLng(randomPoint()));
                verifyGetDistance(a, b);


                var c = new S2LatLng(randomPoint());
                verifyGetRectPointDistance(a, c);
                verifyGetRectPointDistance(b, c);
            }
        }

        [TestMethod]
        public void testGetDistanceRectVsPoint()
        {
            // Rect that spans 180.
            var a = rectFromDegrees(-1, -1, 2, 1);
            verifyGetDistance(a, pointRectFromDegrees(-2, -1));
            verifyGetDistance(a, pointRectFromDegrees(1, 2));

            verifyGetDistance(pointRectFromDegrees(-2, -1), a);
            verifyGetDistance(pointRectFromDegrees(1, 2), a);

            verifyGetRectPointDistance(a, S2LatLng.FromDegrees(-2, -1));
            verifyGetRectPointDistance(a, S2LatLng.FromDegrees(1, 2));

            // Tests near the north pole.
            var b = rectFromDegrees(86, 0, 88, 2);
            verifyGetDistance(b, pointRectFromDegrees(87, 3));
            verifyGetDistance(b, pointRectFromDegrees(87, -1));
            verifyGetDistance(b, pointRectFromDegrees(89, 1));
            verifyGetDistance(b, pointRectFromDegrees(89, 181));
            verifyGetDistance(b, pointRectFromDegrees(85, 1));
            verifyGetDistance(b, pointRectFromDegrees(85, 181));
            verifyGetDistance(b, pointRectFromDegrees(90, 0));

            verifyGetDistance(pointRectFromDegrees(87, 3), b);
            verifyGetDistance(pointRectFromDegrees(87, -1), b);
            verifyGetDistance(pointRectFromDegrees(89, 1), b);
            verifyGetDistance(pointRectFromDegrees(89, 181), b);
            verifyGetDistance(pointRectFromDegrees(85, 1), b);
            verifyGetDistance(pointRectFromDegrees(85, 181), b);
            verifyGetDistance(pointRectFromDegrees(90, 0), b);

            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(87, 3));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(87, -1));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(89, 1));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(89, 181));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(85, 1));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(85, 181));
            verifyGetRectPointDistance(b, S2LatLng.FromDegrees(90, 0));

            // Rect that touches the north pole.
            var c = rectFromDegrees(88, 0, 90, 2);
            verifyGetDistance(c, pointRectFromDegrees(89, 3));
            verifyGetDistance(c, pointRectFromDegrees(89, 90));
            verifyGetDistance(c, pointRectFromDegrees(89, 181));
            verifyGetDistance(pointRectFromDegrees(89, 3), c);
            verifyGetDistance(pointRectFromDegrees(89, 90), c);
            verifyGetDistance(pointRectFromDegrees(89, 181), c);
        }

        [TestMethod]
        public void testGetDistanceRectVsRect()
        {
            // Rect that spans 180.
            var a = rectFromDegrees(-1, -1, 2, 1);
            verifyGetDistance(a, rectFromDegrees(0, 2, 1, 3));
            verifyGetDistance(a, rectFromDegrees(-2, -3, -1, -2));

            // Tests near the south pole.
            var b = rectFromDegrees(-87, 0, -85, 3);
            verifyGetDistance(b, rectFromDegrees(-89, 1, -88, 2));
            verifyGetDistance(b, rectFromDegrees(-84, 1, -83, 2));
            verifyGetDistance(b, rectFromDegrees(-88, 90, -86, 91));
            verifyGetDistance(b, rectFromDegrees(-84, -91, -83, -90));
            verifyGetDistance(b, rectFromDegrees(-90, 181, -89, 182));
            verifyGetDistance(b, rectFromDegrees(-84, 181, -83, 182));
        }
    }
}