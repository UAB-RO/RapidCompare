using System.Linq;

namespace UABRO.RapidCompare.Analysis
{
    public struct BoxWhiskerComponents
    {
        public double Median;
        public double UpperQuartile;
        public double LowerQuartile;
        public double UpperAdjacentValue;
        public double LowerAdjacentValue;
        public double Mean;
        public double Minimum;
        public double Maximum;
        public double[] MildOutliers;
        public double[] ExtremeOutliers;
        public double InterQuartileRange { get => UpperQuartile - LowerQuartile; }
        public double UpperInnerFence { get => UpperQuartile + 1.5 * InterQuartileRange; }
        public double LowerInnerFence { get => LowerQuartile - 1.5 * InterQuartileRange; }
        public BoxWhiskerComponents(double[] values)
        {
            Median = Statistics.Median(values);
            UpperQuartile = Statistics.UpperQuartile(values);
            LowerQuartile = Statistics.LowerQuartile(values);

            double iqr = UpperQuartile - LowerQuartile;
            var upperInnerFence = UpperQuartile + 1.5 * iqr;
            var upperOuterFence = UpperQuartile + 3 * iqr;
            UpperAdjacentValue = values.Where(v => v <= upperInnerFence).Max();

            var lowerInnerFence = LowerQuartile - 1.5 * iqr;
            var lowerOuterFence = LowerQuartile - 3 * iqr;
            LowerAdjacentValue = values.Where(v => v >= lowerInnerFence).Min();

            MildOutliers = values.Where(v => (v < lowerInnerFence || v > upperInnerFence) && v >= lowerOuterFence && v <= upperOuterFence).ToArray();
            ExtremeOutliers = values.Where(v => v < lowerOuterFence || v > upperOuterFence).ToArray();

            Minimum = values.Min();
            Maximum = values.Max();
            Mean = values.Average();
        }
    }
}
