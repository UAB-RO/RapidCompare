using Microsoft.VisualStudio.TestTools.UnitTesting;
using UABRO.RapidCompare.Extraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UABRO.RapidCompare.Extraction.Tests
{
    [TestClass()]
    public class WorklistBuilderTests
    {
        [TestMethod()]
        public void ParseQuotedStringTest()
        {
            var input = "\"{\"\"$type\"\":\"\"UAB.AutoPlanning.Helpers.BeamCreation.CoplanarVmatPlacer, AutoPlanning.Helpers\"\",\"\"NumberOfArcs\"\":3,\"\"ThirdArcCollimatorAngle\"\":0}\"";
            var expected = "{\"$type\":\"UAB.AutoPlanning.Helpers.BeamCreation.CoplanarVmatPlacer, AutoPlanning.Helpers\",\"NumberOfArcs\":3,\"ThirdArcCollimatorAngle\":0}";
            var actual = WorklistBuilder.ParseQuotedString(input);

            Assert.AreEqual(expected, actual);
        }
    }
}