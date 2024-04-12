using Microsoft.VisualStudio.TestTools.UnitTesting;
using UABRO.RapidCompare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction.Tests
{
    [TestClass()]
    public class WorklistItemTests
    {
        [TestMethod()]
        public void ConvertDoseUnitsCGyToGyTest()
        {
            DoseValue toConvert = new DoseValue(1234, DoseValue.DoseUnit.cGy);
            DoseValue expected = new DoseValue(toConvert.Dose / 100, DoseValue.DoseUnit.Gy);
            DoseValue actual = WorklistItem.ConvertDoseUnits(toConvert, expected.Unit);

            Assert.AreEqual(expected.Dose, actual.Dose);
            Assert.AreEqual(expected.Unit, actual.Unit);
        }

        [TestMethod()]
        public void ConvertDoseUnitsGyToCGyTest()
        {
            DoseValue toConvert = new DoseValue(12.34, DoseValue.DoseUnit.Gy);
            DoseValue expected = new DoseValue(toConvert.Dose * 100, DoseValue.DoseUnit.cGy);
            DoseValue actual = WorklistItem.ConvertDoseUnits(toConvert, expected.Unit);

            Assert.AreEqual(expected.Dose, actual.Dose);
            Assert.AreEqual(expected.Unit, actual.Unit);
        }

        [TestMethod()]
        public void ConvertDoseUnitsGyToGyTest()
        {
            DoseValue toConvert = new DoseValue(12.34, DoseValue.DoseUnit.Gy);
            DoseValue expected = new DoseValue(toConvert.Dose, toConvert.Unit);
            DoseValue actual = WorklistItem.ConvertDoseUnits(toConvert, expected.Unit);

            Assert.AreEqual(expected.Dose, actual.Dose);
            Assert.AreEqual(expected.Unit, actual.Unit);
        }

        [TestMethod()]
        public void ConvertDoseUnitsCGyToCGyTest()
        {
            DoseValue toConvert = new DoseValue(1234, DoseValue.DoseUnit.cGy);
            DoseValue expected = new DoseValue(toConvert.Dose, toConvert.Unit);
            DoseValue actual = WorklistItem.ConvertDoseUnits(toConvert, expected.Unit);

            Assert.AreEqual(expected.Dose, actual.Dose);
            Assert.AreEqual(expected.Unit, actual.Unit);
        }

        [TestMethod()]
        public void ConvertDoseUnitsInvalidTargetUnitThrowsExceptionTest()
        {
            DoseValue toConvert = new DoseValue(12.34, DoseValue.DoseUnit.Gy);

            var ex = Assert.ThrowsException<ArgumentException>(() => WorklistItem.ConvertDoseUnits(toConvert, DoseValue.DoseUnit.Percent));
            Assert.AreEqual("Target dose unit Percent is invalid.", ex.Message);
        }
        [TestMethod()]
        public void ConvertDoseUnitsInvalidDoseUnitThrowsExceptionTest()
        {
            DoseValue toConvert = new DoseValue(12.34, DoseValue.DoseUnit.Percent);

            var ex = Assert.ThrowsException<ArgumentException>(() => WorklistItem.ConvertDoseUnits(toConvert, DoseValue.DoseUnit.Gy));
            Assert.AreEqual("Dose value unit Percent is invalid.", ex.Message);
        }
    }
}