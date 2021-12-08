namespace OpenSky.S2Geometry.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenSky.S2Geometry;

    [TestClass]
    public class S2CellTest : GeometryTestCase
    {
        public const bool DEBUG_MODE = true;

        private class LevelStats
        {
            public double avgAngleSpan;
            public double avgArea;
            public double avgDiag;
            public double avgEdge;
            public double avgWidth;
            public double count;
            public double maxAngleSpan;
            public double maxApproxRatio;
            public double maxArea;
            public double maxDiag;
            public double maxDiagAspect;
            public double maxEdge;
            public double maxEdgeAspect;
            public double maxWidth;
            public double minAngleSpan;
            public double minApproxRatio;
            public double minArea;
            public double minDiag;
            public double minEdge;
            public double minWidth;

            public LevelStats()
            {
                this.count = 0;
                this.minArea = 100;
                this.maxArea = 0;
                this.avgArea = 0;
                this.minWidth = 100;
                this.maxWidth = 0;
                this.avgWidth = 0;
                this.minEdge = 100;
                this.maxEdge = 0;
                this.avgEdge = 0;
                this.maxEdgeAspect = 0;
                this.minDiag = 100;
                this.maxDiag = 0;
                this.avgDiag = 0;
                this.maxDiagAspect = 0;
                this.minAngleSpan = 100;
                this.maxAngleSpan = 0;
                this.avgAngleSpan = 0;
                this.minApproxRatio = 100;
                this.maxApproxRatio = 0;
            }
        }

        private static readonly List<LevelStats> levelStats = new List<LevelStats>(
            S2CellId.MaxLevel + 1);

        static S2CellTest()
        {
            for (var i = 0; i < S2CellId.MaxLevel + 1; ++i)
            {
                levelStats.Add(new LevelStats());
            }
        }

        private static void gatherStats(S2Cell cell)
        {
            var s = levelStats[cell.Level];
            var exactArea = cell.ExactArea();
            var approxArea = cell.ApproxArea();
            double minEdge = 100, maxEdge = 0, avgEdge = 0;
            double minDiag = 100, maxDiag = 0;
            double minWidth = 100, maxWidth = 0;
            double minAngleSpan = 100, maxAngleSpan = 0;
            for (var i = 0; i < 4; ++i)
            {
                var edge = cell.GetVertexRaw(i).Angle(cell.GetVertexRaw((i + 1) & 3));
                minEdge = Math.Min(edge, minEdge);
                maxEdge = Math.Max(edge, maxEdge);
                avgEdge += 0.25*edge;
                var mid = cell.GetVertexRaw(i) + cell.GetVertexRaw((i + 1) & 3);
                var width = S2.PiOver2 - mid.Angle(cell.GetEdgeRaw(i ^ 2));
                minWidth = Math.Min(width, minWidth);
                maxWidth = Math.Max(width, maxWidth);
                if (i < 2)
                {
                    var diag = cell.GetVertexRaw(i).Angle(cell.GetVertexRaw(i ^ 2));
                    minDiag = Math.Min(diag, minDiag);
                    maxDiag = Math.Max(diag, maxDiag);
                    var angleSpan = cell.GetEdgeRaw(i).Angle(
                        -cell.GetEdgeRaw(i ^ 2));
                    minAngleSpan = Math.Min(angleSpan, minAngleSpan);
                    maxAngleSpan = Math.Max(angleSpan, maxAngleSpan);
                }
            }
            s.count += 1;
            s.minArea = Math.Min(exactArea, s.minArea);
            s.maxArea = Math.Max(exactArea, s.maxArea);
            s.avgArea += exactArea;
            s.minWidth = Math.Min(minWidth, s.minWidth);
            s.maxWidth = Math.Max(maxWidth, s.maxWidth);
            s.avgWidth += 0.5*(minWidth + maxWidth);
            s.minEdge = Math.Min(minEdge, s.minEdge);
            s.maxEdge = Math.Max(maxEdge, s.maxEdge);
            s.avgEdge += avgEdge;
            s.maxEdgeAspect = Math.Max(maxEdge/minEdge, s.maxEdgeAspect);
            s.minDiag = Math.Min(minDiag, s.minDiag);
            s.maxDiag = Math.Max(maxDiag, s.maxDiag);
            s.avgDiag += 0.5*(minDiag + maxDiag);
            s.maxDiagAspect = Math.Max(maxDiag/minDiag, s.maxDiagAspect);
            s.minAngleSpan = Math.Min(minAngleSpan, s.minAngleSpan);
            s.maxAngleSpan = Math.Max(maxAngleSpan, s.maxAngleSpan);
            s.avgAngleSpan += 0.5*(minAngleSpan + maxAngleSpan);
            var approxRatio = approxArea/exactArea;
            s.minApproxRatio = Math.Min(approxRatio, s.minApproxRatio);
            s.maxApproxRatio = Math.Max(approxRatio, s.maxApproxRatio);
        }

        public void testSubdivide(S2Cell cell)
        {
            gatherStats(cell);
            if (cell.IsLeaf)
            {
                return;
            }

            var children = new S2Cell[4];
            for (var i = 0; i < children.Length; ++i)
            {
                children[i] = new S2Cell();
            }
            Assert.IsTrue(cell.Subdivide(children));
            var childId = cell.Id.ChildBegin;
            double exactArea = 0;
            double approxArea = 0;
            double averageArea = 0;
            for (var i = 0; i < 4; ++i, childId = childId.Next)
            {
                exactArea += children[i].ExactArea();
                approxArea += children[i].ApproxArea();
                averageArea += children[i].AverageArea();

                // Check that the child geometry is consistent with its cell id.
                JavaAssert.Equal(children[i].Id, childId);
                Assert.IsTrue(children[i].Center.ApproxEquals(childId.ToPoint(), 1e-15));
                var direct = new S2Cell(childId);
                JavaAssert.Equal(children[i].Face, direct.Face);
                JavaAssert.Equal(children[i].Level, direct.Level);
                JavaAssert.Equal(children[i].Orientation, direct.Orientation);
                JavaAssert.Equal(children[i].CenterRaw, direct.CenterRaw);
                for (var k = 0; k < 4; ++k)
                {
                    JavaAssert.Equal(children[i].GetVertexRaw(k), direct.GetVertexRaw(k));
                    JavaAssert.Equal(children[i].GetEdgeRaw(k), direct.GetEdgeRaw(k));
                }

                // Test Contains() and MayIntersect().
                Assert.IsTrue(cell.Contains(children[i]));
                Assert.IsTrue(cell.MayIntersect(children[i]));
                Assert.IsTrue(!children[i].Contains(cell));
                Assert.IsTrue(cell.Contains(children[i].CenterRaw));
                for (var j = 0; j < 4; ++j)
                {
                    Assert.IsTrue(cell.Contains(children[i].GetVertexRaw(j)));
                    if (j != i)
                    {
                        Assert.IsTrue(!children[i].Contains(children[j].CenterRaw));
                        Assert.IsTrue(!children[i].MayIntersect(children[j]));
                    }
                }

                // Test GetCapBound and GetRectBound.
                var parentCap = cell.CapBound;
                var parentRect = cell.RectBound;
                if (cell.Contains(new S2Point(0, 0, 1))
                    || cell.Contains(new S2Point(0, 0, -1)))
                {
                    Assert.IsTrue(parentRect.Lng.IsFull);
                }
                var childCap = children[i].CapBound;
                var childRect = children[i].RectBound;
                Assert.IsTrue(childCap.Contains(children[i].Center));
                Assert.IsTrue(childRect.Contains(children[i].CenterRaw));
                Assert.IsTrue(parentCap.Contains(children[i].Center));
                Assert.IsTrue(parentRect.Contains(children[i].CenterRaw));
                for (var j = 0; j < 4; ++j)
                {
                    Assert.IsTrue(childCap.Contains(children[i].GetVertex(j)));
                    Assert.IsTrue(childRect.Contains(children[i].GetVertex(j)));
                    Assert.IsTrue(childRect.Contains(children[i].GetVertexRaw(j)));
                    Assert.IsTrue(parentCap.Contains(children[i].GetVertex(j)));
                    if (!parentRect.Contains(children[i].GetVertex(j)))
                    {
                        Console.WriteLine("cell: " + cell + " i: " + i + " j: " + j);
                        Console.WriteLine("Children " + i + ": " + children[i]);
                        Console.WriteLine("Parent rect: " + parentRect);
                        Console.WriteLine("Vertex raw(j) " + children[i].GetVertex(j));
                        Console.WriteLine("Latlng of vertex: " + new S2LatLng(children[i].GetVertex(j)));
                        Console.WriteLine("RectBound: " + cell.RectBound);
                    }
                    Assert.IsTrue(parentRect.Contains(children[i].GetVertex(j)));
                    if (!parentRect.Contains(children[i].GetVertexRaw(j)))
                    {
                        Console.WriteLine("cell: " + cell + " i: " + i + " j: " + j);
                        Console.WriteLine("Children " + i + ": " + children[i]);
                        Console.WriteLine("Parent rect: " + parentRect);
                        Console.WriteLine("Vertex raw(j) " + children[i].GetVertexRaw(j));
                        Console.WriteLine("Latlng of vertex: " + new S2LatLng(children[i].GetVertexRaw(j)));
                        Console.WriteLine("RectBound: " + cell.RectBound);
                    }
                    Assert.IsTrue(parentRect.Contains(children[i].GetVertexRaw(j)));
                    if (j != i)
                    {
                        // The bounding caps and rectangles should be tight enough so that
                        // they exclude at least two vertices of each adjacent cell.
                        var capCount = 0;
                        var rectCount = 0;
                        for (var k = 0; k < 4; ++k)
                        {
                            if (childCap.Contains(children[j].GetVertex(k)))
                            {
                                ++capCount;
                            }
                            if (childRect.Contains(children[j].GetVertexRaw(k)))
                            {
                                ++rectCount;
                            }
                        }
                        Assert.IsTrue(capCount <= 2);
                        if (childRect.LatLo.Radians > -S2.PiOver2
                            && childRect.LatHi.Radians < S2.PiOver2)
                        {
                            // Bounding rectangles may be too large at the poles because the
                            // pole itself has an arbitrary fixed longitude.
                            Assert.IsTrue(rectCount <= 2);
                        }
                    }
                }

                // Check all children for the first few levels, and then sample randomly.
                // Also subdivide one corner cell, one edge cell, and one center cell
                // so that we have a better chance of sample the minimum metric values.
                var forceSubdivide = false;
                var center = S2Projections.GetNorm(children[i].Face);
                var edge = center + S2Projections.GetUAxis(children[i].Face);
                var corner = edge + S2Projections.GetVAxis(children[i].Face);
                for (var j = 0; j < 4; ++j)
                {
                    var p = children[i].GetVertexRaw(j);
                    if (p.Equals(center) || p.Equals(edge) || p.Equals(corner))
                    {
                        forceSubdivide = true;
                    }
                }
                if (forceSubdivide || cell.Level < (DEBUG_MODE ? 5 : 6)
                    || this.random(DEBUG_MODE ? 10 : 4) == 0)
                {
                    this.testSubdivide(children[i]);
                }
            }

            // Check sum of child areas equals parent area.
            //
            // For ExactArea(), the best relative error we can expect is about 1e-6
            // because the precision of the unit vector coordinates is only about 1e-15
            // and the edge length of a leaf cell is about 1e-9.
            //
            // For ApproxArea(), the areas are accurate to within a few percent.
            //
            // For AverageArea(), the areas themselves are not very accurate, but
            // the average area of a parent is exactly 4 times the area of a child.

            Assert.IsTrue(Math.Abs(Math.Log(exactArea/cell.ExactArea())) <= Math
                                                                              .Abs(Math.Log(1 + 1e-6)));
            Assert.IsTrue(Math.Abs(Math.Log(approxArea/cell.ApproxArea())) <= Math
                                                                                .Abs(Math.Log(1.03)));
            Assert.IsTrue(Math.Abs(Math.Log(averageArea/cell.AverageArea())) <= Math
                                                                                  .Abs(Math.Log(1 + 1e-15)));
        }

        public void testMinMaxAvg(String label, int level, double count,
                                  double absError, double minValue, double maxValue, double avgValue,
                                  S2CellMetric minMetric, S2CellMetric maxMetric, S2CellMetric avgMetric)
        {
            // All metrics are minimums, maximums, or averages of differential
            // quantities, and therefore will not be exact for cells at any finite
            // level. The differential minimum is always a lower bound, and the maximum
            // is always an upper bound, but these minimums and maximums may not be
            // achieved for two different reasons. First, the cells at each level are
            // sampled and we may miss the most extreme examples. Second, the actual
            // metric for a cell is obtained by integrating the differential quantity,
            // which is not constant across the cell. Therefore cells at low levels
            // (bigger cells) have smaller variations.
            //
            // The "tolerance" below is an attempt to model both of these effects.
            // At low levels, error is dominated by the variation of differential
            // quantities across the cells, while at high levels error is dominated by
            // the effects of random sampling.
            var tolerance = (maxMetric.GetValue(level) - minMetric.GetValue(level))
                            /Math.Sqrt(Math.Min(count, 0.5*(1L << level)))*10;
            if (tolerance == 0)
            {
                tolerance = absError;
            }

            var minError = minValue - minMetric.GetValue(level);
            var maxError = maxMetric.GetValue(level) - maxValue;
            var avgError = Math.Abs(avgMetric.GetValue(level) - avgValue);
            Console.WriteLine(
                "%-10s (%6.0f samples, tolerance %8.3g) - Min (%9.3g : %9.3g) "
                + "Max (%9.3g : %9.3g), avg (%9.3g : %9.3g)\n", label, count,
                tolerance, minError/minValue, minError/tolerance, maxError
                                                                  /maxValue, maxError/tolerance, avgError/avgValue, avgError
                                                                                                                    /tolerance);

            Assert.IsTrue(minMetric.GetValue(level) <= minValue + absError);
            Assert.IsTrue(minMetric.GetValue(level) >= minValue - tolerance);
            Console.WriteLine("Level: " + maxMetric.GetValue(level) + " Max " + (maxValue + tolerance));
            Assert.IsTrue(maxMetric.GetValue(level) <= maxValue + tolerance);
            Assert.IsTrue(maxMetric.GetValue(level) >= maxValue - absError);
            this.assertDoubleNear(avgMetric.GetValue(level), avgValue, 10*tolerance);
        }

        private const int MAX_LEVEL = DEBUG_MODE ? 6 : 10;

        public void expandChildren1(S2Cell cell)
        {
            var children = new S2Cell[4];
            Assert.IsTrue(cell.Subdivide(children));
            if (children[0].Level < MAX_LEVEL)
            {
                for (var pos = 0; pos < 4; ++pos)
                {
                    this.expandChildren1(children[pos]);
                }
            }
        }

        public void expandChildren2(S2Cell cell)
        {
            var id = cell.Id.ChildBegin;
            for (var pos = 0; pos < 4; ++pos, id = id.Next)
            {
                var child = new S2Cell(id);
                if (child.Level < MAX_LEVEL)
                {
                    this.expandChildren2(child);
                }
            }
        }

        [TestMethod]
        public void testFaces()
        {
            IDictionary<S2Point, int> edgeCounts = new Dictionary<S2Point, int>();
            IDictionary<S2Point, int> vertexCounts = new Dictionary<S2Point, int>();
            for (var face = 0; face < 6; ++face)
            {
                var id = S2CellId.FromFacePosLevel(face, 0, 0);
                var cell = new S2Cell(id);
                JavaAssert.Equal(cell.Id, id);
                JavaAssert.Equal(cell.Face, face);
                JavaAssert.Equal(cell.Level, (byte)0);
                // Top-level faces have alternating orientations to get RHS coordinates.
                JavaAssert.Equal(cell.Orientation, (byte)(face & S2.SwapMask));
                Assert.IsTrue(!cell.IsLeaf);
                for (var k = 0; k < 4; ++k)
                {
                    if (edgeCounts.ContainsKey(cell.GetEdgeRaw(k)))
                    {
                        edgeCounts[cell.GetEdgeRaw(k)] = edgeCounts[cell
                                                                        .GetEdgeRaw(k)] + 1;
                    }
                    else
                    {
                        edgeCounts[cell.GetEdgeRaw(k)] = 1;
                    }

                    if (vertexCounts.ContainsKey(cell.GetVertexRaw(k)))
                    {
                        vertexCounts[cell.GetVertexRaw(k)] = vertexCounts[cell
                                                                              .GetVertexRaw(k)] + 1;
                    }
                    else
                    {
                        vertexCounts[cell.GetVertexRaw(k)] = 1;
                    }
                    this.assertDoubleNear(cell.GetVertexRaw(k).DotProd(cell.GetEdgeRaw(k)), 0);
                    this.assertDoubleNear(cell.GetVertexRaw((k + 1) & 3).DotProd(
                        cell.GetEdgeRaw(k)), 0);
                    this.assertDoubleNear(S2Point.Normalize(
                        S2Point.CrossProd(cell.GetVertexRaw(k), cell
                                                                    .GetVertexRaw((k + 1) & 3))).DotProd(cell.GetEdge(k)), 1.0);
                }
            }
            // Check that edges have multiplicity 2 and vertices have multiplicity 3.
            foreach (var i in edgeCounts.Values)
            {
                JavaAssert.Equal(i, 2);
            }
            foreach (var i in vertexCounts.Values)
            {
                JavaAssert.Equal(i, 3);
            }
        }

        [TestMethod]
        public void testSubdivide()
        {
            for (var face = 0; face < 6; ++face)
            {
                this.testSubdivide(S2Cell.FromFacePosLevel(face, (byte)0, 0));
            }

            // The maximum edge *ratio* is the ratio of the longest edge of any cell to
            // the shortest edge of any cell at the same level (and similarly for the
            // maximum diagonal ratio).
            //
            // The maximum edge *aspect* is the maximum ratio of the longest edge of a
            // cell to the shortest edge of that same cell (and similarly for the
            // maximum diagonal aspect).

            Console
                .WriteLine("Level    Area      Edge          Diag          Approx       Average\n");
            Console
                .WriteLine("        Ratio  Ratio Aspect  Ratio Aspect    Min    Max    Min    Max\n");
            for (var i = 0; i <= S2CellId.MaxLevel; ++i)
            {
                var s = levelStats[i];
                if (s.count > 0)
                {
                    s.avgArea /= s.count;
                    s.avgWidth /= s.count;
                    s.avgEdge /= s.count;
                    s.avgDiag /= s.count;
                    s.avgAngleSpan /= s.count;
                }
                Console.WriteLine(
                    "%5d  %6.3f %6.3f %6.3f %6.3f %6.3f %6.3f %6.3f %6.3f %6.3f\n", i,
                    s.maxArea/s.minArea, s.maxEdge/s.minEdge, s.maxEdgeAspect,
                    s.maxDiag/s.minDiag, s.maxDiagAspect, s.minApproxRatio,
                    s.maxApproxRatio, S2Cell.AverageArea(i)/s.maxArea, S2Cell
                                                                           .AverageArea(i)
                                                                       /s.minArea);
            }

            // Now check the validity of the S2 length and area metrics.
            for (var i = 0; i <= S2CellId.MaxLevel; ++i)
            {
                var s = levelStats[i];
                if (s.count == 0)
                {
                    continue;
                }

                Console.WriteLine(
                    "Level {0} - metric (error/actual : error/tolerance)\n", i);

                // The various length calculations are only accurate to 1e-15 or so,
                // so we need to allow for this amount of discrepancy with the theoretical
                // minimums and maximums. The area calculation is accurate to about 1e-15
                // times the cell width.
                this.testMinMaxAvg("area", i, s.count, 1e-15*s.minWidth, s.minArea,
                              s.maxArea, s.avgArea, S2Projections.MinArea, S2Projections.MaxArea,
                              S2Projections.AvgArea);
                this.testMinMaxAvg("width", i, s.count, 1e-15, s.minWidth, s.maxWidth,
                              s.avgWidth, S2Projections.MinWidth, S2Projections.MaxWidth,
                              S2Projections.AvgWidth);
                this.testMinMaxAvg("edge", i, s.count, 1e-15, s.minEdge, s.maxEdge,
                              s.avgEdge, S2Projections.MinEdge, S2Projections.MaxEdge,
                              S2Projections.AvgEdge);
                this.testMinMaxAvg("diagonal", i, s.count, 1e-15, s.minDiag, s.maxDiag,
                              s.avgDiag, S2Projections.MinDiag, S2Projections.MaxDiag,
                              S2Projections.AvgDiag);
                this.testMinMaxAvg("angle span", i, s.count, 1e-15, s.minAngleSpan,
                              s.maxAngleSpan, s.avgAngleSpan, S2Projections.MinAngleSpan,
                              S2Projections.MaxAngleSpan, S2Projections.AvgAngleSpan);

                // The aspect ratio calculations are ratios of lengths and are therefore
                // less accurate at higher subdivision levels.
                Assert.IsTrue(s.maxEdgeAspect <= S2Projections.MaxEdgeAspect + 1e-15
                            *(1 << i));
                Assert.IsTrue(s.maxDiagAspect <= S2Projections.MaxDiagAspect + 1e-15
                            *(1 << i));
            }
        }
    }
}