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
    public class S2Test : GeometryTestCase
    {
        // Test helper methods for testing the traversal order.
        private static int swapAxes(int ij)
        {
            return ((ij >> 1) & 1) + ((ij & 1) << 1);
        }

        private static int invertBits(int ij)
        {
            return ij ^ 3;
        }

        // Note: obviously, I could have defined a bundle of metrics like this in the
        // S2 class itself rather than just for testing. However, it's not clear that
        // this is useful other than for testing purposes, and I find
        // S2.kMinWidth.GetMaxLevel(width) to be slightly more readable than
        // than S2.kWidth.min().GetMaxLevel(width). Also, there is no fundamental
        // reason that we need to analyze the minimum, maximum, and average values of
        // every metric; it would be perfectly reasonable to just define one of these.

        public class MetricBundle
        {
            public S2CellMetric avg_;
            public S2CellMetric max_;
            public S2CellMetric min_;

            public MetricBundle(S2CellMetric min, S2CellMetric max, S2CellMetric avg)
            {
                min_ = min;
                max_ = max;
                avg_ = avg;
            }
        }

        public void testMinMaxAvg(MetricBundle bundle)
        {
            assertTrue(bundle.min_.Deriv() < bundle.avg_.Deriv());
            assertTrue(bundle.avg_.Deriv() < bundle.max_.Deriv());
        }

        public void testLessOrEqual(MetricBundle a, MetricBundle b)
        {
            assertTrue(a.min_.Deriv() <= b.min_.Deriv());
            assertTrue(a.max_.Deriv() <= b.max_.Deriv());
            assertTrue(a.avg_.Deriv() <= b.avg_.Deriv());
        }

        [TestMethod]
        public void testAngleArea()
        {
            var pz = new S2Point(0, 0, 1);
            var p000 = new S2Point(1, 0, 0);
            var p045 = new S2Point(1, 1, 0);
            var p090 = new S2Point(0, 1, 0);
            var p180 = new S2Point(-1, 0, 0);
            assertDoubleNear(S2.Angle(p000, pz, p045), S2.PiOver4);
            assertDoubleNear(S2.Angle(p045, pz, p180), 3*S2.PiOver4);
            assertDoubleNear(S2.Angle(p000, pz, p180), S2.Pi);
            assertDoubleNear(S2.Angle(pz, p000, pz), 0);
            assertDoubleNear(S2.Angle(pz, p000, p045), S2.PiOver2);

            assertDoubleNear(S2.Area(p000, p090, pz), S2.PiOver2);
            assertDoubleNear(S2.Area(p045, pz, p180), 3*S2.PiOver4);

            // Make sure that area() has good *relative* accuracy even for
            // very small areas.
            var eps = 1e-10;
            var pepsx = new S2Point(eps, 0, 1);
            var pepsy = new S2Point(0, eps, 1);
            var expected1 = 0.5*eps*eps;
            assertDoubleNear(S2.Area(pepsx, pepsy, pz), expected1, 1e-14*expected1);

            // Make sure that it can handle degenerate triangles.
            var pr = new S2Point(0.257, -0.5723, 0.112);
            var pq = new S2Point(-0.747, 0.401, 0.2235);
            assertEquals(S2.Area(pr, pr, pr), 0.0);
            // TODO: The following test is not exact in optimized mode because the
            // compiler chooses to mix 64-bit and 80-bit intermediate results.
            assertDoubleNear(S2.Area(pr, pq, pr), 0);
            assertEquals(S2.Area(p000, p045, p090), 0.0);

            double maxGirard = 0;
            for (var i = 0; i < 10000; ++i)
            {
                var p0 = randomPoint();
                var d1 = randomPoint();
                var d2 = randomPoint();
                var p1 = p0 + (d1 * 1e-15);
                var p2 = p0 + (d2 * 1e-15);
                // The actual displacement can be as much as 1.2e-15 due to roundoff.
                // This yields a maximum triangle area of about 0.7e-30.
                assertTrue(S2.Area(p0, p1, p2) < 0.7e-30);
                maxGirard = Math.Max(maxGirard, S2.GirardArea(p0, p1, p2));
            }
            Console.WriteLine("Worst case Girard for triangle area 1e-30: " + maxGirard);

            // Try a very long and skinny triangle.
            var p045eps = new S2Point(1, 1, eps);
            var expected2 = 5.8578643762690495119753e-11; // Mathematica.
            assertDoubleNear(S2.Area(p000, p045eps, p090), expected2, 1e-9*expected2);

            // Triangles with near-180 degree edges that sum to a quarter-sphere.
            var eps2 = 1e-10;
            var p000eps2 = new S2Point(1, 0.1*eps2, eps2);
            var quarterArea1 =
                S2.Area(p000eps2, p000, p090) + S2.Area(p000eps2, p090, p180) + S2.Area(p000eps2, p180, pz)
                + S2.Area(p000eps2, pz, p000);
            assertDoubleNear(quarterArea1, S2.Pi);

            // Four other triangles that sum to a quarter-sphere.
            var p045eps2 = new S2Point(1, 1, eps2);
            var quarterArea2 =
                S2.Area(p045eps2, p000, p090) + S2.Area(p045eps2, p090, p180) + S2.Area(p045eps2, p180, pz)
                + S2.Area(p045eps2, pz, p000);
            assertDoubleNear(quarterArea2, S2.Pi);
        }

        [TestMethod]
        public void testCCW()
        {
            var a = new S2Point(0.72571927877036835, 0.46058825605889098, 0.51106749730504852);
            var b = new S2Point(0.7257192746638208, 0.46058826573818168, 0.51106749441312738);
            var c = new S2Point(0.72571927671709457, 0.46058826089853633, 0.51106749585908795);
            assertTrue(S2.RobustCcw(a, b, c) != 0);
        }

        [TestMethod]
        public void testExp()
        {
            for (var i = 0; i < 10; ++i)
            {
                assertEquals(i + 1, S2.Exp(Math.Pow(2, i)));
            }

            for (var i = 0; i < 10; ++i)
            {
                assertEquals(i + 1, S2.Exp(-Math.Pow(2, i)));
            }

            assertEquals(0, S2.Exp(0));
            assertEquals(2, S2.Exp(3));
            assertEquals(3, S2.Exp(5));
        }

        [TestMethod]
        public void testFaceUVtoXYZ()
        {
            // Check that each face appears exactly once.
            var sum = new S2Point();
            for (var face = 0; face < 6; ++face)
            {
                var center = S2Projections.FaceUvToXyz(face, 0, 0);
                assertEquals(S2Projections.GetNorm(face), center);
                assertEquals(Math.Abs(center[center.LargestAbsComponent]), 1.0);
                sum = sum + S2Point.Fabs(center);
            }
            assertEquals(sum, new S2Point(2, 2, 2));

            // Check that each face has a right-handed coordinate system.
            for (var face = 0; face < 6; ++face)
            {
                assertEquals(
                    S2Point.CrossProd(S2Projections.GetUAxis(face), S2Projections.GetVAxis(face)).DotProd(
                        S2Projections.FaceUvToXyz(face, 0, 0)), 1.0);
            }

            // Check that the Hilbert curves on each face combine to form a
            // continuous curve over the entire cube.
            for (var face = 0; face < 6; ++face)
            {
                // The Hilbert curve on each face starts at (-1,-1) and terminates
                // at either (1,-1) (if axes not swapped) or (-1,1) (if swapped).
                var sign = ((face & S2.SwapMask) != 0) ? -1 : 1;
                assertEquals(S2Projections.FaceUvToXyz(face, sign, -sign),
                             S2Projections.FaceUvToXyz((face + 1)%6, -1, -1));
            }
        }

        [TestMethod]
        public void testMetrics()
        {
            var angleSpan = new MetricBundle(
                S2Projections.MinAngleSpan, S2Projections.MaxAngleSpan, S2Projections.AvgAngleSpan);
            var width =
                new MetricBundle(S2Projections.MinWidth, S2Projections.MaxWidth, S2Projections.AvgWidth);
            var edge =
                new MetricBundle(S2Projections.MinEdge, S2Projections.MaxEdge, S2Projections.AvgEdge);
            var diag =
                new MetricBundle(S2Projections.MinDiag, S2Projections.MaxDiag, S2Projections.AvgDiag);
            var area =
                new MetricBundle(S2Projections.MinArea, S2Projections.MaxArea, S2Projections.AvgArea);

            // First, check that min <= avg <= max for each metric.
            testMinMaxAvg(angleSpan);
            testMinMaxAvg(width);
            testMinMaxAvg(edge);
            testMinMaxAvg(diag);
            testMinMaxAvg(area);

            // Check that the maximum aspect ratio of an individual cell is consistent
            // with the global minimums and maximums.
            assertTrue(S2Projections.MaxEdgeAspect >= 1.0);
            assertTrue(S2Projections.MaxEdgeAspect
                       < S2Projections.MaxEdge.Deriv()/S2Projections.MinEdge.Deriv());
            assertTrue(S2Projections.MaxDiagAspect >= 1);
            assertTrue(S2Projections.MaxDiagAspect
                       < S2Projections.MaxDiag.Deriv()/S2Projections.MinDiag.Deriv());

            // Check various conditions that are provable mathematically.
            testLessOrEqual(width, angleSpan);
            testLessOrEqual(width, edge);
            testLessOrEqual(edge, diag);

            assertTrue(S2Projections.MinArea.Deriv()
                       >= S2Projections.MinWidth.Deriv()*S2Projections.MinEdge.Deriv() - 1e-15);
            assertTrue(S2Projections.MaxArea.Deriv()
                       < S2Projections.MaxWidth.Deriv()*S2Projections.MaxEdge.Deriv() + 1e-15);

            // GetMinLevelForLength() and friends have built-in assertions, we just need
            // to call these functions to test them.
            //
            // We don't actually check that the metrics are correct here, e.g. that
            // GetMinWidth(10) is a lower bound on the width of cells at level 10.
            // It is easier to check these properties in s2cell_unittest, since
            // S2Cell has methods to compute the cell vertices, etc.

            for (var level = -2; level <= S2CellId.MaxLevel + 3; ++level)
            {
                var dWidth = (2*S2Projections.MinWidth.Deriv())*Math.Pow(2, -level);
                if (level >= S2CellId.MaxLevel + 3)
                {
                    dWidth = 0;
                }

                // Check boundary cases (exactly equal to a threshold value).
                var expectedLevel = Math.Max(0, Math.Min(S2CellId.MaxLevel, level));
                assertEquals(S2Projections.MinWidth.GetMinLevel(dWidth), expectedLevel);
                assertEquals(S2Projections.MinWidth.GetMaxLevel(dWidth), expectedLevel);
                assertEquals(S2Projections.MinWidth.GetClosestLevel(dWidth), expectedLevel);

                // Also check non-boundary cases.
                assertEquals(S2Projections.MinWidth.GetMinLevel(1.2*dWidth), expectedLevel);
                assertEquals(S2Projections.MinWidth.GetMaxLevel(0.8*dWidth), expectedLevel);
                assertEquals(S2Projections.MinWidth.GetClosestLevel(1.2*dWidth), expectedLevel);
                assertEquals(S2Projections.MinWidth.GetClosestLevel(0.8*dWidth), expectedLevel);

                // Same thing for area1.
                var area1 = (4*S2Projections.MinArea.Deriv())*Math.Pow(4, -level);
                if (level <= -3)
                {
                    area1 = 0;
                }
                assertEquals(S2Projections.MinArea.GetMinLevel(area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetMaxLevel(area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetClosestLevel(area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetMinLevel(1.2*area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetMaxLevel(0.8*area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetClosestLevel(1.2*area1), expectedLevel);
                assertEquals(S2Projections.MinArea.GetClosestLevel(0.8*area1), expectedLevel);
            }
        }

        [TestMethod]
        public void testSTUV()
        {
            // Check boundary conditions.
            for (double x = -1; x <= 1; ++x)
            {
                assertEquals(S2Projections.StToUv(x), x);
                assertEquals(S2Projections.UvToSt(x), x);
            }
            // Check that UVtoST and STtoUV are inverses.
            for (double x = -1; x <= 1; x += 0.0001)
            {
                assertDoubleNear(S2Projections.UvToSt(S2Projections.StToUv(x)), x);
                assertDoubleNear(S2Projections.StToUv(S2Projections.UvToSt(x)), x);
            }
        }

        [TestMethod]
        public void testTraversalOrder()
        {
            for (var r = 0; r < 4; ++r)
            {
                for (var i = 0; i < 4; ++i)
                {
                    // Check consistency with respect to swapping axes.
                    assertEquals(S2.IjToPos(r, i),
                                 S2.IjToPos(r ^ S2.SwapMask, swapAxes(i)));
                    assertEquals(S2.PosToIj(r, i),
                                 swapAxes(S2.PosToIj(r ^ S2.SwapMask, i)));

                    // Check consistency with respect to reversing axis directions.
                    assertEquals(S2.IjToPos(r, i),
                                 S2.IjToPos(r ^ S2.InvertMask, invertBits(i)));
                    assertEquals(S2.PosToIj(r, i),
                                 invertBits(S2.PosToIj(r ^ S2.InvertMask, i)));

                    // Check that the two tables are inverses of each other.
                    assertEquals(S2.IjToPos(r, S2.PosToIj(r, i)), i);
                    assertEquals(S2.PosToIj(r, S2.IjToPos(r, i)), i);
                }
            }
        }

        [TestMethod]
        public void testUVAxes()
        {
            // Check that axes are consistent with FaceUVtoXYZ.
            for (var face = 0; face < 6; ++face)
            {
                assertEquals(S2Projections.GetUAxis(face), 
                    S2Projections.FaceUvToXyz(face, 1, 0) - S2Projections.FaceUvToXyz(face, 0, 0));
                assertEquals(S2Projections.GetVAxis(face), 
                    S2Projections.FaceUvToXyz(face, 0, 1) - S2Projections.FaceUvToXyz(face, 0, 0));
            }
        }

        [TestMethod]
        public void testUVNorms()
        {
            // Check that GetUNorm and GetVNorm compute right-handed normals for
            // an edge in the increasing U or V direction.
            for (var face = 0; face < 6; ++face)
            {
                for (double x = -1; x <= 1; x += 1/1024.0)
                {
                    assertDoubleNear(
                        S2Point.CrossProd(
                            S2Projections.FaceUvToXyz(face, x, -1), S2Projections.FaceUvToXyz(face, x, 1))
                               .Angle(S2Projections.GetUNorm(face, x)), 0);
                    assertDoubleNear(
                        S2Point.CrossProd(
                            S2Projections.FaceUvToXyz(face, -1, x), S2Projections.FaceUvToXyz(face, 1, x))
                               .Angle(S2Projections.GetVNorm(face, x)), 0);
                }
            }
        }
    }
}