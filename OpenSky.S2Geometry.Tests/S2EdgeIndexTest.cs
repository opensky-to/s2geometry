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
    public class S2EdgeIndexTest : GeometryTestCase
    {
        public class EdgeVectorIndex : S2EdgeIndex
        {
            private readonly List<S2Edge> edges;

            public EdgeVectorIndex(List<S2Edge> edges)
            {
                this.edges = edges;
            }


            protected override int NumEdges
            {
                get { return edges.Count; }
            }


            protected override S2Point EdgeFrom(int index)
            {
                return edges[index].Start;
            }


            protected override S2Point EdgeTo(int index)
            {
                return edges[index].End;
            }
        }

        /**
   * Generates a random edge whose center is in the given cap.
   */

        private S2Edge randomEdgeCrossingCap(double maxLengthMeters, S2Cap cap)
        {
            // Pick the edge center at random.
            var edgeCenter = samplePoint(cap);
            // Pick two random points in a suitably sized cap about the edge center.
            var edgeCap = S2Cap.FromAxisAngle(
                edgeCenter, S1Angle.FromRadians(maxLengthMeters/S2LatLng.EarthRadiusMeters/2));
            var p1 = samplePoint(edgeCap);
            var p2 = samplePoint(edgeCap);
            return new S2Edge(p1, p2);
        }

        /*
   * Generates "numEdges" random edges, of length at most "edgeLengthMetersMax"
   * and each of whose center is in a randomly located cap with radius
   * "capSpanMeters", and puts results into "edges".
   */

        private void generateRandomEarthEdges(
            double edgeLengthMetersMax, double capSpanMeters, int numEdges, List<S2Edge> edges)
        {
            var cap = S2Cap.FromAxisAngle(
                randomPoint(), S1Angle.FromRadians(capSpanMeters/S2LatLng.EarthRadiusMeters));
            for (var i = 0; i < numEdges; ++i)
            {
                edges.Add(randomEdgeCrossingCap(edgeLengthMetersMax, cap));
            }
        }

        private void checkAllCrossings(
            List<S2Edge> allEdges, int minCrossings, int maxChecksCrossingsRatio)
        {
            var index = new EdgeVectorIndex(allEdges);
            index.ComputeIndex();
             var it = new S2EdgeIndex.DataEdgeIterator(index);
            double totalCrossings = 0;
            double totalIndexChecks = 0;

            for (var @in = 0; @in < allEdges.Count; ++@in)
            {
                var e = allEdges[@in];

                var candidateSet = new HashSet<int>();

                var sb = new StringBuilder();
                it.GetCandidates(e.Start, e.End); 
                foreach (var i in it)// it.GetCandidates(e.Start, e.End); it.HasNext; it.Next())
                {
                    candidateSet.Add(i);
                    sb.Append(i).Append("/");
                    ++totalIndexChecks;
                }

                for (var i = 0; i < allEdges.Count; ++i)
                {
                    var crossing = S2EdgeUtil.RobustCrossing(
                        e.Start, e.End, allEdges[i].Start, allEdges[i].End);
                    if (crossing >= 0)
                    {
                        var sbError = new StringBuilder();
                        sbError
                            .Append("\n==CHECK_ERROR===================================\n")
                            .Append("CandidateSet: ")
                            .Append(sb)
                            .Append("\nin=")
                            .Append(@in)
                            .Append(" i=")
                            .Append(i)
                            .Append(" robustCrossing=")
                            .Append(crossing)
                            .Append("\nfrom:\n")
                            .Append(e)
                            .Append("\nto:\n")
                            .Append(allEdges[i])
                            .Append("\n==================================================");
                        assertTrue(sbError.ToString(), candidateSet.Contains(i));
                        ++totalCrossings;
                    }
                }
            }

            Console.WriteLine(
                "Pairs/num crossings/check crossing ratio: "
                + (allEdges.Count*allEdges.Count) + "/"
                + totalCrossings + "/"
                + (totalIndexChecks/totalCrossings));
            assertTrue(minCrossings <= totalCrossings);
            assertTrue(totalCrossings*maxChecksCrossingsRatio >= totalIndexChecks);
        }

        /*
   * Generates random edges and tests, for each edge, that all those that cross
   * are candidates.
   */

        private void tryCrossingsRandomInCap(int numEdges, double edgeLengthMax, double capSpanMeters,
                                             int minCrossings, int maxChecksCrossingsRatio)
        {
            var allEdges = new List<S2Edge>();
            generateRandomEarthEdges(edgeLengthMax, capSpanMeters, numEdges, allEdges);
            checkAllCrossings(allEdges, minCrossings, maxChecksCrossingsRatio);
        }

        [TestMethod]
        public void testLoopCandidateOfItself()
        {
            var ps = new List<S2Point>(); // A diamond loop around 0,180.
            ps.Add(makePoint("0:178"));
            ps.Add(makePoint("-1:180"));
            ps.Add(makePoint("0:-179"));
            ps.Add(makePoint("1:-180"));
            var allEdges = new List<S2Edge>();
            for (var i = 0; i < 4; ++i)
            {
                allEdges.Add(new S2Edge(ps[i], ps[(i + 1)%4]));
            }
            checkAllCrossings(allEdges, 0, 16);
        }

        [TestMethod]
        public void testRandomEdgeCrossings()
        {
            tryCrossingsRandomInCap(2000, 30, 5000, 500, 2);
            tryCrossingsRandomInCap(1000, 100, 5000, 500, 3);
            tryCrossingsRandomInCap(1000, 1000, 5000, 1000, 40);
            tryCrossingsRandomInCap(500, 5000, 5000, 5000, 20);
        }

        [TestMethod]
        public void testRandomEdgeCrossingsSparse()
        {
            for (var i = 0; i < 5; ++i)
            {
                tryCrossingsRandomInCap(2000, 100, 5000, 500, 8);
                tryCrossingsRandomInCap(2000, 300, 50000, 1000, 10);
            }
        }

        [TestMethod]
        public void testSpecificEdges()
        {
            var ps = new List<S2Point>();
            ps.Add(new S2Point(0.8088625416501157, -0.40633615485481134, 0.4250086092929434));
            ps.Add(new S2Point(0.8088939911085784, -0.40631384442755236, 0.4249700824469155));
            ps.Add(new S2Point(0.8088088971141814, -0.40642839367135375, 0.425022503835579));
            ps.Add(new S2Point(0.8088643962606756, -0.406333410696549, 0.4250077032402616));
            var allEdges = new List<S2Edge>();
            allEdges.Add(new S2Edge(ps[0], ps[1]));
            allEdges.Add(new S2Edge(ps[2], ps[3]));
            checkAllCrossings(allEdges, 0, 16);
        }
    }
}