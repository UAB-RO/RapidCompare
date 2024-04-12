using System;
using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UABRO.RapidCompare.Extraction.Tests
{
    [TestClass()]
    public class CoordinateTransformTests
    {
        [TestMethod()]
        public void PatientDicomToIECTest()
        {
            Point3D iso = new Point3D(1, 2, 3);
            var M = CoordinateTransform.PatientDicomToIEC(iso);
            Point3D point3D = new Point3D(7, 8, 9);
            
            var expected = new Point3D(point3D.X-iso.X, point3D.Z-iso.Z, iso.Y-point3D.Y);
            var actual = M.Transform(point3D);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void FixedToGantryTest()
        {
            Point3D expected = new Point3D(0, 1, 1);
            for(double gantry=0; gantry < 360; gantry+=45)
            {
                var M = CoordinateTransform.FixedToGantry(gantry);
                var phi = gantry * Math.PI / 180;
                Point3D f = new Point3D(Math.Sin(phi), 1, Math.Cos(phi));
                var actual = M.Transform(f);
                Assert.AreEqual(expected, actual,$" gantry angle {gantry:0.0} degrees.");
            }
        }

        [TestMethod()]
        public void GantryToBeamLimitingDeviceTest()
        {
            Point3D expected = new Point3D(1, 0, 1);
            for (double collimator = 0; collimator < 360; collimator += 45)
            {
                var M = CoordinateTransform.GantryToBeamLimitingDevice(collimator);
                var theta = collimator * Math.PI / 180;
                Point3D f = new Point3D(Math.Cos(theta), Math.Sin(theta), 1);
                var actual = M.Transform(f);
                Assert.AreEqual(expected, actual, $" collimator angle {collimator:0.0} degrees.");
            }
        }

        [TestMethod()]
        public void PatientSupportToFixedTest()
        {
            Point3D expected = new Point3D(1, 0, 1);
            for (double table = 0; table < 360; table += 45)
            {
                var M = CoordinateTransform.PatientSupportToFixed(table);
                var theta = table * Math.PI / 180;
                Point3D f = new Point3D(Math.Cos(theta), -Math.Sin(theta), 1);
                var actual = M.Transform(f);
                Assert.AreEqual(expected, actual, $" table angle {table:0.0} degrees.");
            }
        }
    }
}