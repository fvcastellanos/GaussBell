using System.Collections.Generic;
using System.Linq;
using System.Text;
using GaussBell.Services.Domain;
using MathNet.Numerics.Distributions;

namespace GaussBell.Services
{
    public static class ChartPointService
    {
        private static readonly Normal NormalDist = Normal.WithMeanStdDev(0, 1);

        public static IEnumerable<ChartPoint> BuildChartPoints(int initValue, int count)
        {
            return Enumerable.Range(initValue, count)
                .Select(value => new ChartPoint(value, NormalDist.Density(value)))
                .ToList();
        }

        public static string PointsToString(IEnumerable<ChartPoint> points)
        {
            var sb = new StringBuilder();

            points.ToList()
                .ForEach(point => sb.AppendFormat("[{0}, {1}], ", point.X, point.Y));

            return sb.ToString();
        }
    }
}