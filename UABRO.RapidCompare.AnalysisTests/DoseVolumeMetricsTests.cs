using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using UABRO.RapidCompare.Model;

namespace UABRO.RapidCompare.Analysis.Tests
{
    [TestClass()]
    public class DoseVolumeMetricsTests
    {
        private RapidPlanModelDvhSet rapidPlanModelDvhSet;
        [TestInitialize]
        public void TestInitialize()
        {
            rapidPlanModelDvhSet = new RapidPlanModelDvhSet();
            rapidPlanModelDvhSet.RapidPlanDvhs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RapidPlanDoseVolumeHistograms>>(Properties.Resources.RapidPlanDvhsJson);
        }
        [TestMethod()]
        public void GetRapidPlanReferencePlanDvhDifferenceStatisticsTest()
        {
            double expected = 75.9;
            double actual = DoseVolumeMetrics.GetMaximumDose(rapidPlanModelDvhSet.RapidPlanDvhs);
            Assert.AreEqual(expected, actual, 1e-9);
        }

        [TestMethod()]
        public void GetDvhDifferenceStatisticsMedianTest()
        {
            // Arrange: generated using MATLAB.
            var dose = new double[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 75.0, 75.9 };
            // Expected result.
            var expected = new Dictionary<string, double[]>();
            expected.Add("Bladder", new double[] { 1.421085472e-14, 1.421085472e-14, -5.531686941, -4.597225783, -2.496053865, -0.229348076, -0.5856769832, -0.2920233932, 0, 0 });
            expected.Add("Bowel_Small", new double[] { 0, -0.4012384534, -4.907719359, -2.857667233, -1.463120746, -0.1250330284, 0, 0, 0, 0 });
            expected.Add("PTV_High^p+sv", new double[] { 0, 0, 0, 0, 0, 0, 0, 0.0005296015728, -0.002597965183, 0 });
            expected.Add("PTVn_Low", new double[] { 0, 0, 0, 0, 1.867358141e-05, 0.1721892985, -0.01884494461, 0.000606877777, 0, 0 });
            expected.Add("Rectum", new double[] { 0, 0.1995776128, -2.867757918, -7.463687235, -5.900003646, 0.5957435239, -0.6682688468, 0.2962316026, 0, 0 });

            foreach (var group in rapidPlanModelDvhSet.RapidPlanDvhsByStructure)
            {
                var result = DoseVolumeMetrics.GetVolumeDifferenceBoxWhiskerStatistics(group.ToList(), dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
                for (int i = 0; i < dose.Length; i++)
                    Assert.AreEqual(expected[group.Key][i], result[i].Median, 1e-6, $"{group.Key} at {dose[i]} Gy");
            }
        }

        [TestMethod()]
        public void GetDvhDifferenceStatisticsLowerQuartileTest()
        {
            // Arrange: generated using MATLAB.
            var dose = new double[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 75.0, 75.9 };
            // Expected result.
            var expected = new Dictionary<string, double[]>();
            expected.Add("Bladder", new double[] { 4.736951572e-15, -0.08568358681, -11.35920778, -8.112186683, -6.480582197, -5.647666665, -0.920086032, -0.367502198, -0.001389783435, 0 });
            expected.Add("Bowel_Small", new double[] { -4.736951572e-15, -1.274210067, -5.804769587, -4.510251649, -2.832254642, -1.388665717, 0, 0, 0, 0 });
            expected.Add("PTV_High^p+sv", new double[] { -4.736951572e-15, -4.736951572e-15, -4.736951572e-15, -4.736951572e-15, -4.736951572e-15, -4.736951572e-15, -4.736951572e-15, 2.101892023e-05, -0.01074204366, 0 });
            expected.Add("PTVn_Low", new double[] { -9.473903143e-15, -9.473903143e-15, -9.473903143e-15, 0, 9.473903143e-15, -1.878080929, -0.2311354897, -0.02986723431, 0, 0 });
            expected.Add("Rectum", new double[] { -4.736951572e-15, -0.1119687948, -6.274392593, -11.75012833, -6.696940207, -6.894716832, -1.759126441, -0.4403641042, 0, 0 });

            foreach (var group in rapidPlanModelDvhSet.RapidPlanDvhsByStructure)
            {
                var result = DoseVolumeMetrics.GetVolumeDifferenceBoxWhiskerStatistics(group.ToList(), dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
                for (int i = 0; i < dose.Length; i++)
                    Assert.AreEqual(expected[group.Key][i], result[i].LowerQuartile, 1e-6, $"{group.Key} at {dose[i]} Gy");
            }
        }

        [TestMethod()]
        public void GetDvhDifferenceStatisticsUpperQuartileTest()
        {
            // Arrange: generated using MATLAB.
            var dose = new double[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 75.0, 75.9 };
            // Expected result.
            var expected = new Dictionary<string, double[]>();
            expected.Add("Bladder", new double[] { 1.421085472e-14, 1.421085472e-14, 4.134951904, 8.780372506, 3.984841335, 0.3736822892, -0.4323364241, -0.06795504418, 0.02931925886, 0 });
            expected.Add("Bowel_Small", new double[] { 1.894780629e-14, 0.6999042134, -1.639159894, -1.229849605, 0.3884724798, 0.8000175246, 0, 0, 0, 0 });
            expected.Add("PTV_High^p+sv", new double[] { 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 0.003686958559, 0.1546029101, 0 });
            expected.Add("PTVn_Low", new double[] { 4.736951572e-15, 4.736951572e-15, 4.736951572e-15, 0.009001959305, 0.07236003464, 2.335554844, 0.04195257716, 0.003665169443, 0, 0 });
            expected.Add("Rectum", new double[] { 4.736951572e-15, 0.8863092883, 1.418059802, -0.4831750559, 0.1119301874, 3.540858838, 0.309243308, 0.9673592557, 0, 0 });

            foreach (var group in rapidPlanModelDvhSet.RapidPlanDvhsByStructure)
            {
                var result = DoseVolumeMetrics.GetVolumeDifferenceBoxWhiskerStatistics(group.ToList(), dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
                for (int i = 0; i < dose.Length; i++)
                    Assert.AreEqual(expected[group.Key][i], result[i].UpperQuartile, 1e-6, $"{group.Key} at {dose[i]} Gy");
            }
        }

        [TestMethod()]
        public void GetDvhDifferenceStatisticsOneTailSignTest()
        {
            // Arrange: generated using MATLAB.
            var dose = new double[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 75.0, 75.9 };
            // Expected result.
            var expected = new Dictionary<string, double[]>();
            expected.Add("Bladder", new double[] { 0.1875, 0.5, 0.8125, 0.8125, 0.8125, 0.96875, 1, 0.96875, 0.75, 1 });
            expected.Add("Bowel_Small", new double[] { 0.5, 0.8125, 0.96875, 0.96875, 0.96875, 0.8125, 1, 1, 1, 1 });
            expected.Add("PTV_High^p+sv", new double[] { 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.1875, 0.8125, 1 });
            expected.Add("PTVn_Low", new double[] { 0.75, 0.75, 0.75, 0.25, 0.0625, 0.5, 0.8125, 0.5, 1, 1 });
            expected.Add("Rectum", new double[] { 0.75, 0.1875, 0.8125, 0.96875, 0.8125, 0.5, 0.8125, 0.5, 1, 1 });

            foreach (var group in rapidPlanModelDvhSet.RapidPlanDvhsByStructure)
            {
                var result = DoseVolumeMetrics.GetVolumeDifferenceOneSidedSignTestPValue(group.ToList(), dose, DoseUnit.Gy, DvhType.RapidPlan, DvhType.ReferencePlan);
                for (int i = 0; i < dose.Length; i++)
                    Assert.AreEqual(expected[group.Key][i], result[i], 1e-6, $"{group.Key} at {dose[i]} Gy");
            }
        }
    }
}