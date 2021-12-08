// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyS2Tests.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.S2Geometry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using GeoCoordinatePortable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OpenSky.S2Geometry.Extensions;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky S2 Tests.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [TestClass]
    public class OpenSkyS2Tests
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Test Cell ID methods and their performance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        [TestMethod]
        public void CellIDTests()
        {
            var lowwCell11 = new GeoCoordinate(48.11027908325195, 16.569721221923828).CellIDForCoordinates(11);
            Assert.AreEqual("476c544", lowwCell11.ToToken());

            var lowwCell8 = new GeoCoordinate(48.11027908325195, 16.569721221923828).CellIDForCoordinates(8);
            Assert.AreEqual("476c5", lowwCell8.ToToken());

            var random = new Random();
            var coordinates = new List<GeoCoordinate>();
            for (var i = 0; i < 1000; i++)
            {
                coordinates.Add(new GeoCoordinate(random.NextDouble() * 90 * (random.Next(1, 3) == 2 ? -1 : 1), random.NextDouble() * 180 * (random.Next(1, 3) == 2 ? -1 : 1)));
            }

            var cellIDs = new List<string>();
            var start = DateTime.Now;
            foreach (var coordinate in coordinates)
            {
                cellIDs.Add(coordinate.CellIDForCoordinates(11).ToToken());
            }

            var end = DateTime.Now;
            Assert.IsTrue((end - start).TotalSeconds < 0.001);

#if DEBUG
            Debug.WriteLine($"Calculated {cellIDs.Count} cell IDs in {(end - start).TotalSeconds} seconds.");
            Debug.WriteLine("First 10 entries:");
            for (var i = 0; i < 10; i++)
            {
                Debug.WriteLine($"{coordinates[i]} ==> {cellIDs[i]}");
            }
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Test doughnut coverage and its performance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        [TestMethod]
        public void DoughnutCoverageTests()
        {
            var loww = new GeoCoordinate(48.11027908325195, 16.569721221923828);

            var start = DateTime.Now;
            var doughnutCoverage = loww.DoughnutCoverage(600, 30);
            var end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"MEP LOWW 30-600nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(800, 50);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"SET LOWW 50-800nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(1100, 50);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"MET LOWW 50-1100nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(1800, 150);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"JET LOWW 150-1800nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(1200, 150);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"REG LOWW 150-1200nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(2000, 150);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"NBA LOWW 150-2000nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif

            start = DateTime.Now;
            doughnutCoverage = loww.DoughnutCoverage(7000, 1000);
            end = DateTime.Now;
            Assert.IsTrue(doughnutCoverage.IncludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.IncludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue(doughnutCoverage.ExcludeLevel is >= 3 and <= 9);
            Assert.IsTrue(doughnutCoverage.ExcludeCells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"WBA LOWW 1000-7000nm, level include {doughnutCoverage.IncludeLevel} ==> {doughnutCoverage.IncludeCells.Count} cells, level exclude {doughnutCoverage.ExcludeLevel} ==> {doughnutCoverage.ExcludeCells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(doughnutCoverage.IncludeCells);
            PrintCellIds(doughnutCoverage.ExcludeCells);
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Test circular coverage and its performance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        [TestMethod]
        public void CircularCoverageTests()
        {
            var loww = new GeoCoordinate(48.11027908325195, 16.569721221923828);
            var start = DateTime.Now;
            var circularCoverage = loww.CircularCoverage(150);
            var end = DateTime.Now;
            Assert.IsTrue(circularCoverage.Level is >= 3 and <= 9);
            Assert.IsTrue(circularCoverage.Cells?.Count is > 0 and <= 500);
            Assert.IsTrue((end - start).TotalSeconds < 0.1);

#if DEBUG
            Debug.WriteLine($"SEP LOWW 150nm, level {circularCoverage.Level} ==> {circularCoverage.Cells.Count} cells, {(end - start).TotalSeconds} seconds.");
            PrintCellIds(circularCoverage.Cells);
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Print cell ID tokens.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="cells">
        /// The cells.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void PrintCellIds(List<S2CellId> cells)
        {
            var ids = string.Empty;
            foreach (var id in cells)
            {
                ids += $"{id.ToToken()},";
            }
            Debug.WriteLine(ids);
        }
    }
}