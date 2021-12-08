// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyS2.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.S2Geometry.Extensions
{
    using System.Collections.Generic;

    using GeoCoordinatePortable;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// S2 common methods for OpenSky.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class OpenSkyS2
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate S2 cell ID for the specified coordinates.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/12/2021.
        /// </remarks>
        /// <param name="latitude">
        /// The latitude.
        /// </param>
        /// <param name="longitude">
        /// The longitude.
        /// </param>
        /// <param name="level">
        /// The S2 cell level.
        /// </param>
        /// <returns>
        /// A S2CellId.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static S2CellId CellIDForCoordinates(double latitude, double longitude, int level)
        {
            var s2LatLng = S2LatLng.FromDegrees(latitude, longitude);
            var s2Cell = new S2Cell(s2LatLng);
            return s2Cell.Id.ParentForLevel(level);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate S2 cell ID for the specified coordinates.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/12/2021.
        /// </remarks>
        /// <param name="geoCoordinate">
        /// The geoCoordinate to act on.
        /// </param>
        /// <param name="level">
        /// The S2 cell level.
        /// </param>
        /// <returns>
        /// A S2CellId.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static S2CellId CellIDForCoordinates(this GeoCoordinate geoCoordinate, int level)
        {
            return CellIDForCoordinates(geoCoordinate.Latitude, geoCoordinate.Longitude, level);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate circular coverage around a location.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="latitude">
        /// The latitude.
        /// </param>
        /// <param name="longitude">
        /// The longitude.
        /// </param>
        /// <param name="radius">
        /// The radius in nautical miles.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=500.
        /// </param>
        /// <returns>
        /// A CircularCoverage object containing the chosen level and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static CircularCoverage CircularCoverage(double latitude, double longitude, double radius, int minLevel = 3, int maxLevel = 9, int maxCells = 500)
        {
            var s2LatLng = S2LatLng.FromDegrees(latitude, longitude);
            var s2Point = s2LatLng.ToPoint();
            var angle = S1Angle.FromRadians(((radius * 1.852) * 1000) / 6378137.0);
            var sphereCap = S2Cap.FromAxisAngle(s2Point, angle);

            var cells = new List<S2CellId>();
            var selectedLevel = minLevel;
            for (var level = minLevel; level <= maxLevel; level++)
            {
                var levelCells = new List<S2CellId>();
                S2RegionCoverer.GetSimpleCovering(sphereCap, s2Point, level, levelCells);
                if (levelCells.Count < maxCells)
                {
                    cells.Clear();
                    cells.AddRange(levelCells);
                    selectedLevel = level;
                }
                else
                {
                    break;
                }
            }

            return new CircularCoverage { Level = selectedLevel, Cells = cells };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate circular coverage around a location.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="geoCoordinate">
        /// The geoCoordinate to act on.
        /// </param>
        /// <param name="radius">
        /// The radius in nautical miles.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=500.
        /// </param>
        /// <returns>
        /// A CircularCoverage object containing the chosen level and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static CircularCoverage CircularCoverage(this GeoCoordinate geoCoordinate, double radius, int minLevel = 3, int maxLevel = 9, int maxCells = 500)
        {
            return CircularCoverage(geoCoordinate.Latitude, geoCoordinate.Longitude, radius, minLevel, maxLevel, maxCells);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate doughnut coverage around a location (outer radius includes, inner radius excludes).
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="latitude">
        /// The latitude.
        /// </param>
        /// <param name="longitude">
        /// The longitude.
        /// </param>
        /// <param name="outerRadius">
        /// The outer radius in nautical miles.
        /// </param>
        /// <param name="innerRadius">
        /// The inner radius in nautical miles.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=500.
        /// </param>
        /// <returns>
        /// A DoughnutCoverage object containing the chosen levels and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static DoughnutCoverage DoughnutCoverage(double latitude, double longitude, double outerRadius, double innerRadius, int minLevel = 3, int maxLevel = 9, int maxCells = 500)
        {
            var outer = CircularCoverage(latitude, longitude, outerRadius, minLevel, maxLevel, maxCells);
            var inner = CircularCoverage(latitude, longitude, innerRadius, minLevel, maxLevel, maxCells);

            return new DoughnutCoverage { IncludeLevel = outer.Level, IncludeCells = outer.Cells, ExcludeLevel = inner.Level, ExcludeCells = inner.Cells };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate doughnut coverage around a location (outer radius includes, inner radius excludes).
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="geoCoordinate">
        /// The geoCoordinate to act on.
        /// </param>
        /// <param name="outerRadius">
        /// The outer radius in nautical miles.
        /// </param>
        /// <param name="innerRadius">
        /// The inner radius in nautical miles.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=500.
        /// </param>
        /// <returns>
        /// A DoughnutCoverage object containing the chosen levels and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static DoughnutCoverage DoughnutCoverage(this GeoCoordinate geoCoordinate, double outerRadius, double innerRadius, int minLevel = 3, int maxLevel = 9, int maxCells = 500)
        {
            return DoughnutCoverage(geoCoordinate.Latitude, geoCoordinate.Longitude, outerRadius, innerRadius, minLevel, maxLevel, maxCells);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate rectangle coverage between two corner locations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="from">
        /// The from location.
        /// </param>
        /// <param name="to">
        /// The to location.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=300.
        /// </param>
        /// <returns>
        /// A RectangleCoverage object containing the chosen level and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static RectangleCoverage RectangleCoverage(GeoCoordinate from, GeoCoordinate to, int minLevel = 3, int maxLevel = 9, int maxCells = 300)
        {
            return RectangleCoverage(from.Latitude, from.Longitude, to.Latitude, to.Longitude, minLevel, maxLevel, maxCells);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate rectangle coverage between two corner locations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="fromLatitude">
        /// The from latitude.
        /// </param>
        /// <param name="fromLongitude">
        /// The from longitude.
        /// </param>
        /// <param name="toLatitude">
        /// The to latitude.
        /// </param>
        /// <param name="toLongitude">
        /// The to longitude.
        /// </param>
        /// <param name="minLevel">
        /// (Optional) The minimum S2 cell level, default=3.
        /// </param>
        /// <param name="maxLevel">
        /// (Optional) The maximum S2 cell level, default=9.
        /// </param>
        /// <param name="maxCells">
        /// (Optional) The maximum S2 cells, default=300.
        /// </param>
        /// <returns>
        /// A RectangleCoverage object containing the chosen level and cell IDs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static RectangleCoverage RectangleCoverage(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, int minLevel = 3, int maxLevel = 9, int maxCells = 300)
        {
            var fromS2 = S2LatLng.FromDegrees(fromLatitude, fromLongitude);
            var toS2 = S2LatLng.FromDegrees(toLatitude, toLongitude);
            var rect = S2LatLngRect.FromPointPair(fromS2, toS2);

            var cells = new List<S2CellId>();
            var selectedLevel = minLevel;
            for (var level = minLevel; level <= maxLevel; level++)
            {
                var levelCells = new List<S2CellId>();
                S2RegionCoverer.GetSimpleCovering(rect, fromS2.ToPoint(), level, levelCells);
                if (levelCells.Count < maxCells)
                {
                    cells.Clear();
                    cells.AddRange(levelCells);
                    selectedLevel = level;
                }
                else
                {
                    break;
                }
            }

            return new RectangleCoverage { Level = selectedLevel, Cells = cells };
        }
    }
}