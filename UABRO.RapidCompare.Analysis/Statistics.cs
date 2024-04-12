using MathNetStats = MathNet.Numerics.Statistics;

namespace UABRO.RapidCompare.Analysis
{
    public static class Statistics
    {
        public static double Median(double[] values)
        {
            return MathNetStats.Statistics.Median(values);
        }

        public static double LowerQuartile(double[] values)
        {
            return MathNetStats.Statistics.LowerQuartile(values);
        }

        public static double UpperQuartile(double[] values)
        {
            return MathNetStats.Statistics.UpperQuartile(values);
        }

        public static (double pBoth, double pLeft, double pRight) SignTestPValue(double[] values)
        {
            double pboth;
            double pleft;
            double pright;
            alglib.onesamplesigntest(values, values.Length, 0.0, out pboth, out pleft, out pright);
            
            return (pBoth: pboth, pLeft: pleft, pRight: pright);
        }

        public static (double pBoth, double pLeft, double pRight) WilcoxonSignRankPValue(double[] values)
        {
            double pboth;
            double pleft;
            double pright;
            alglib.wilcoxonsignedranktest(values, values.Length, 0.0, out pboth, out pleft, out pright);

            return (pBoth: pboth, pLeft: pleft, pRight: pright);
        }

    }
}
