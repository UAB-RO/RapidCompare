using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UABRO.RapidCompare.Analysis.Tests
{
    [TestClass()]
    public class StatisticsTests
    {
        List<double[]> GetTestArrays()
        {
            // Arrange: generated using MATLAB.
            var x = new List<double[]>();
            x.Add(new double[]{2.042840,0.741870,3.875780,
                1.645110,2.497180,5.027640,4.814600,-2.287540,
                -3.132540,-2.316160,4.368180,0.411190,4.642460,
                -0.554690,3.336630,-2.339750,4.579460,1.726570,
                0.797250,-2.412810});
            x.Add(new double[]{-0.181230,4.502800,4.024900,
                -3.234920,1.104550,0.334270,-0.467270,-1.603480,
                -3.218160,-1.234150,-3.895000,-1.829730,5.358270,
                -2.911190,-3.765520,-1.234050,-2.947370,4.211240,
                1.483960,3.865910});
            x.Add(new double[]{-2.844840,1.265590,2.430700,
                -0.941670,4.132580,3.580670,1.732220,-1.086890,
                0.882050,-3.626870,0.275670,-3.353190,2.589080,
                -3.621890,0.566270,2.563960,-3.400140,4.386060,
                -3.536320,0.953240});
            x.Add(new double[]{5.260940,0.977640,3.305600,
                -2.280290,-1.705680,3.301530,1.778220,2.827880,
                0.446330,2.330830,4.914740,-2.820260,2.337880,
                2.526500,3.048850,-1.540820,4.741280,3.852810,
                -2.400890,-2.484140});
            x.Add(new double[]{2.003280,4.225540,1.424960,
                -1.119350,3.277220,5.098310,-1.000690,4.744560,
                0.058710,1.308720,-1.463750,2.098380,-3.301210,
                -2.418560,-3.906300,4.074810,-3.335650,-0.306740,
                -2.036690,3.258740});
            return x;
        }
        [TestMethod()]
        public void MedianTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{1.685840,-0.850660,0.724160,
                2.3343550,0.6837150};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.Median(x[i]), 1.0e-6);
        }

        [TestMethod()]
        public void WilcoxonSignRankTwoTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.0531692505,0.9272785187,0.8408222198,
                0.0214843750,0.3883762360};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.WilcoxonSignRankPValue(x[i]).pBoth, 1.0e-4);
        }

        [TestMethod()]
        public void WilcoxonSignRankLeftTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.9757795334,0.4636392593,0.5938224792,
                0.9903831482,0.8158617020};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.WilcoxonSignRankPValue(x[i]).pLeft, 1.0e-4);
        }

        [TestMethod()]
        public void WilcoxonSignRankRightTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.0265846252,0.5508413315,0.4204111099,
                0.0107421875,0.1941881180};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.WilcoxonSignRankPValue(x[i]).pRight, 1.0e-4);
        }

        [TestMethod()]
        public void LowerQuartileTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result using R-8 estimation.
            // See https://en.wikipedia.org/wiki/Quantile
            var expected = new double[]{-1.5655191667,-2.9322950000,-3.1413775000,
                -1.6369883333,-1.7979650000};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.LowerQuartile(x[i]), 1.0e-6);
        }

        [TestMethod()]
        public void UpperQuartileTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result using R-8 estimation.
            // See https://en.wikipedia.org/wiki/Quantile
            var expected = new double[]{4.1630133333,2.8734308333,2.5084350000,
                3.3039041667,3.2695200000};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.UpperQuartile(x[i]), 1.0e-6);
        }

        [TestMethod()]
        public void SignTestTwoTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.1153182983,0.5034446716,0.5034446716,
                0.1153182983,0.8238029480};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.SignTestPValue(x[i]).pBoth, 1.0e-4);
        }

        [TestMethod()]
        public void LeftTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.9793052673,0.2517223358,0.8684120178,
                0.9793052673,0.7482776642};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.SignTestPValue(x[i]).pLeft, 1.0e-4);
        }

        [TestMethod()]
        public void RightTailPValueTest()
        {
            // Arrange: generated using MATLAB.
            var x = GetTestArrays();

            // Expected result.
            var expected = new double[]{0.0576591492,0.8684120178,0.2517223358,
                0.0576591492,0.4119014740};

            // Act and Assert
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], Statistics.SignTestPValue(x[i]).pRight, 1.0e-4);
        }
    }
}