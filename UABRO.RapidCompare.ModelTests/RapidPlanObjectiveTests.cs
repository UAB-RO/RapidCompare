using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UABRO.RapidCompare.Model.Tests
{
    [TestClass()]
    public class RapidPlanObjectiveTests
    {
        private RapidPlanObjective GetRapidPlanObjective(string xmlstr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlstr);
            XmlNodeList elemList = doc.GetElementsByTagName("Objective");
            return new RapidPlanObjective(elemList.Item(0));
        }
        [TestMethod()]
        public void RapidPlanObjectivePriorityTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDoseGyPriority);
            Assert.AreEqual(90, obj.Priority);
        }
        [TestMethod()]
        public void RapidPlanObjectiveGeneratedPriorityTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.UpperVolumeDoseGy);
            Assert.IsNull(obj.Priority);
        }
        [TestMethod()]
        public void RapidPlanObjectiveVolumeTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDoseGyPriority);
            Assert.AreEqual(98, obj.Volume);
        }
        [TestMethod()]
        public void RapidPlanObjectiveGeneratedVolumeTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.UpperDoseGyPriority);
            Assert.IsNull(obj.Volume);
        }
        [TestMethod()]
        public void RapidPlanObjectiveDoseTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDoseGyPriority);
            Assert.AreEqual(50, obj.Dose);
        }
        [TestMethod()]
        public void RapidPlanObjectiveAbsoluteDoseUnitTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDoseGyPriority);
            Assert.AreEqual(DoseUnit.Gy, obj.DoseUnit);
        }
        [TestMethod()]
        public void RapidPlanObjectiveRelativeDoseUnitTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDosePercentPriority);
            Assert.AreEqual(DoseUnit.Percent, obj.DoseUnit);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeLowerTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowerVolumeDoseGyPriority);
            Assert.AreEqual(ObjectiveType.Point, obj.Type);
            Assert.AreEqual(ObjectiveOperator.GreaterThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeUpperTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.UpperVolumeDoseGy);
            Assert.AreEqual(ObjectiveType.Point, obj.Type);
            Assert.AreEqual(ObjectiveOperator.LessThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeLinePreferringOarTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LinePreferringOar);
            Assert.AreEqual(ObjectiveType.LinePreferringOar, obj.Type);
            Assert.AreEqual(ObjectiveOperator.LessThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeLinePreferringTargetTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LinePreferringOar);
            Assert.AreEqual(ObjectiveType.LinePreferringOar, obj.Type);
            Assert.AreEqual(ObjectiveOperator.LessThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeLowerGeudTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.LowergEUDDosePercentgEUDa);
            Assert.AreEqual(ObjectiveType.gEUD, obj.Type);
            Assert.AreEqual(ObjectiveOperator.GreaterThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeTargetGeudTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.TargetgEUDDosePercentgEUDa);
            Assert.AreEqual(ObjectiveType.gEUD, obj.Type);
            Assert.AreEqual(ObjectiveOperator.Equal, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeUpperGeudTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.UppergEUDDoseGygEUDa);
            Assert.AreEqual(ObjectiveType.gEUD, obj.Type);
            Assert.AreEqual(ObjectiveOperator.LessThan, obj.Operator);
        }
        [TestMethod()]
        public void RapidPlanObjectiveParameterTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.TargetgEUDDosePercentgEUDa);
            Assert.AreEqual(-2, obj.ParameterA);
        }
        [TestMethod()]
        public void RapidPlanObjectiveTypeMeanTest()
        {
            var obj = GetRapidPlanObjective(Properties.Resources.MeanDose);
            Assert.AreEqual(ObjectiveType.Mean, obj.Type);
            Assert.AreEqual(ObjectiveOperator.LessThan, obj.Operator);
        }
    }
}