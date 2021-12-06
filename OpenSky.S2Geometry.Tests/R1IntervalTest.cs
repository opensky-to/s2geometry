using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Common.Geometry;

namespace S2Geometry.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class R1IntervalTest : GeometryTestCase
    {
        /**
  * Test all of the interval operations on the given pair of intervals.
  * "expected_relation" is a sequence of "T" and "F" characters corresponding
  * to the expected results of contains(), interiorContains(), Intersects(),
  * and InteriorIntersects() respectively.
  */

        private void testIntervalOps(R1Interval x, R1Interval y, String expectedRelation)
        {
            JavaAssert.Equal(x.Contains(y), expectedRelation[0] == 'T');
            JavaAssert.Equal(x.InteriorContains(y), expectedRelation[1] == 'T');
            JavaAssert.Equal(x.Intersects(y), expectedRelation[2] == 'T');
            JavaAssert.Equal(x.InteriorIntersects(y), expectedRelation[3] == 'T');

            JavaAssert.Equal(x.Contains(y), x.Union(y).Equals(x));
            JavaAssert.Equal(x.Intersects(y), !x.Intersection(y).IsEmpty);
        }

        [TestMethod]
        public void R1IntervalBasicTest()
        {
            // Constructors and accessors.
            var unit = new R1Interval(0, 1);
            var negunit = new R1Interval(-1, 0);
            JavaAssert.Equal(unit.Lo, 0.0);
            JavaAssert.Equal(unit.Hi, 1.0);
            JavaAssert.Equal(negunit.Lo, -1.0);
            JavaAssert.Equal(negunit.Hi, 0.0);

            // is_empty()
            var half = new R1Interval(0.5, 0.5);
            Assert.IsTrue(!unit.IsEmpty);
            Assert.IsTrue(!half.IsEmpty);
            var empty = R1Interval.Empty;
            Assert.IsTrue(empty.IsEmpty);

            // GetCenter(), GetLength()
            JavaAssert.Equal(unit.Center, 0.5);
            JavaAssert.Equal(half.Center, 0.5);
            JavaAssert.Equal(negunit.Length, 1.0);
            JavaAssert.Equal(half.Length, 0.0);
            Assert.IsTrue(empty.Length < 0);

            // contains(double), interiorContains(double)
            Assert.IsTrue(unit.Contains(0.5));
            Assert.IsTrue(unit.InteriorContains(0.5));
            Assert.IsTrue(unit.Contains(0));
            Assert.IsTrue(!unit.InteriorContains(0));
            Assert.IsTrue(unit.Contains(1));
            Assert.IsTrue(!unit.InteriorContains(1));

            // contains(R1Interval), interiorContains(R1Interval)
            // Intersects(R1Interval), InteriorIntersects(R1Interval)
            testIntervalOps(empty, empty, "TTFF");
            testIntervalOps(empty, unit, "FFFF");
            testIntervalOps(unit, half, "TTTT");
            testIntervalOps(unit, unit, "TFTT");
            testIntervalOps(unit, empty, "TTFF");
            testIntervalOps(unit, negunit, "FFTF");
            testIntervalOps(unit, new R1Interval(0, 0.5), "TFTT");
            testIntervalOps(half, new R1Interval(0, 0.5), "FFTF");

            // addPoint()
            R1Interval r;
            r = empty.AddPoint(5);
            Assert.IsTrue(r.Lo == 5.0 && r.Hi == 5.0);
            r = r.AddPoint(-1);
            Assert.IsTrue(r.Lo == -1.0 && r.Hi == 5.0);
            r = r.AddPoint(0);
            Assert.IsTrue(r.Lo == -1.0 && r.Hi == 5.0);

            // fromPointPair()
            JavaAssert.Equal(R1Interval.FromPointPair(4, 4), new R1Interval(4, 4));
            JavaAssert.Equal(R1Interval.FromPointPair(-1, -2), new R1Interval(-2, -1));
            JavaAssert.Equal(R1Interval.FromPointPair(-5, 3), new R1Interval(-5, 3));

            // expanded()
            JavaAssert.Equal(empty.Expanded(0.45), empty);
            JavaAssert.Equal(unit.Expanded(0.5), new R1Interval(-0.5, 1.5));

            // union(), intersection()
            Assert.IsTrue(new R1Interval(99, 100).Union(empty).Equals(new R1Interval(99, 100)));
            Assert.IsTrue(empty.Union(new R1Interval(99, 100)).Equals(new R1Interval(99, 100)));
            Assert.IsTrue(new R1Interval(5, 3).Union(new R1Interval(0, -2)).IsEmpty);
            Assert.IsTrue(new R1Interval(0, -2).Union(new R1Interval(5, 3)).IsEmpty);
            Assert.IsTrue(unit.Union(unit).Equals(unit));
            Assert.IsTrue(unit.Union(negunit).Equals(new R1Interval(-1, 1)));
            Assert.IsTrue(negunit.Union(unit).Equals(new R1Interval(-1, 1)));
            Assert.IsTrue(half.Union(unit).Equals(unit));
            Assert.IsTrue(unit.Intersection(half).Equals(half));
            Assert.IsTrue(unit.Intersection(negunit).Equals(new R1Interval(0, 0)));
            Assert.IsTrue(negunit.Intersection(half).IsEmpty);
            Assert.IsTrue(unit.Intersection(empty).IsEmpty);
            Assert.IsTrue(empty.Intersection(unit).IsEmpty);
        }
    }
}