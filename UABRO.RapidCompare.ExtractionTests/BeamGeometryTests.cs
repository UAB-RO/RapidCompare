﻿using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction.Tests
{
    [TestClass()]
    public class BeamGeometryTests
    {
        [TestMethod()]
        public void PointCloudToBEVTest()
        {
            // Arrange: generated using MATLAB
            double gantry = 191.66;
            double collimator = 133.79;

            // The expected result in the beams-eye-view was created by randomly generating 25 points
            // in the beam limiting device frame and projecting to the isocenter (BEV) plane.
            Point3D[] expected = new Point3D[]{ new Point3D(-25.5874790799,54.3546558326,0),
                 new Point3D(1.3209099091,-31.8359286307,0), new Point3D(-11.6375525262,37.8805595809,0),
                 new Point3D(32.3764705328,-16.2297634753,0), new Point3D(-40.2707993516,33.6080031345,0),
                 new Point3D(14.8047342592,-24.0410071047,0), new Point3D(2.7440312595,-3.3859327045,0),
                 new Point3D(25.7811341629,12.1547305437,0), new Point3D(31.8318130984,49.0985891428,0),
                 new Point3D(14.5079587429,41.8543450563,0), new Point3D(21.6847334498,54.7313963282,0),
                 new Point3D(-3.3810891012,6.8616486719,0), new Point3D(-29.1383282038,6.4382993206,0),
                 new Point3D(-29.2422566988,46.9471496596,0), new Point3D(-34.9974936076,38.3242402688,0),
                 new Point3D(-32.1186536467,-4.2942603501,0), new Point3D(12.6258438179,18.1139410234,0),
                 new Point3D(-39.8996234176,27.5922782442,0), new Point3D(8.5037797862,-7.2132689709,0),
                 new Point3D(44.0291929796,26.7137542914,0), new Point3D(38.7163020670,17.8773153280,0),
                 new Point3D(-30.5167234070,-18.6837442775,0), new Point3D(5.2853107680,48.7006054349,0),
                 new Point3D(-50.1741683147,9.5112346427,0), new Point3D(8.9306580555,14.7605899437,0)};

            // The input point cloud was generated by transforming the points from the beam limiting device frame to the fixed frame
            Point3D[] pointCloud = new Point3D[]{ new Point3D(19.1385961232,-55.5960285899,-12.8600963608),
                 new Point3D(-20.1523883786,23.1704580492,12.4175201576), new Point3D(26.4659458273,-35.8009608878,29.5240518887),
                 new Point3D(12.4218532657,34.9212025063,6.8191743960), new Point3D(2.5812798586,-53.9392090147,30.9058333911),
                 new Point3D(-6.0233467379,27.4556057026,6.1592036990), new Point3D(5.4772551637,4.4529402313,29.3206724278),
                 new Point3D(20.9905231935,9.9727438818,-27.0447908976), new Point3D(57.1741284234,-11.0361092552,-8.2871088092),
                 new Point3D(36.4630847901,-18.2643038197,-20.0376223744), new Point3D(47.8728690818,-21.7413566956,-31.9312211664),
                 new Point3D(2.5819886143,-7.1899096859,-0.4210766082), new Point3D(-9.3108569488,-26.2924634526,34.0736874222),
                 new Point3D(18.8231663806,-54.9535057996,21.9452733681), new Point3D(-2.1584320256,-50.3903801595,-27.0541377988),
                 new Point3D(-23.3165802303,-20.3837234379,13.3775486642), new Point3D(33.0989454341,-3.6004497346,46.7925699116),
                 new Point3D(-4.5254823376,-48.6377506555,16.7228291973), new Point3D(7.6502740575,11.5139369920,33.6036300355),
                 new Point3D(44.8314381198,13.0909739152,-25.1070432993), new Point3D(45.4829396661,16.0040704714,18.5980964889),
                 new Point3D(-31.1479616814,-9.2483873348,23.0809357501), new Point3D(48.2331078805,-31.1583935730,33.5081456561),
                 new Point3D(-25.0270767929,-43.3534419353,18.3264827608), new Point3D(10.6779820906,-3.6675639062,-29.3435484379)};

            // Act and Assert
            var actual = BeamGeometry.PointCloudToBEV(pointCloud, gantry, collimator);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(0, (expected[i] - actual[i]).Length, 1e-6, $"{i} ({pointCloud[i].X},{pointCloud[i].Y},{pointCloud[i].Z})");
        }

        [TestMethod()]
        public void PointCloudToFixedTest()
        {
            // Arrange: generated using MATLAB
            double table = 57.0;

            // The expected result is 25 points randomly generated in the fixed frame
            Point3D[] expected = new Point3D[]{ new Point3D(-8.3709829087,9.1001267947,32.3353637101),
                 new Point3D(60.2680487246,-29.8267169581,33.6439583427), new Point3D(59.4958900289,2.9495397465,16.6127270555),
                 new Point3D(-22.0321709326,-3.4118968632,28.1582326856), new Point3D(42.9999281606,50.3612175135,2.1587500823),
                 new Point3D(-32.6496369379,39.3249054611,29.9840049424), new Point3D(31.6527106904,30.1859879320,10.8979269791),
                 new Point3D(3.0018971581,19.9597638919,-46.2966510522), new Point3D(34.3838040066,-42.4047404881,-35.7230213373),
                 new Point3D(-31.6036657323,-35.8748470022,18.9304629994), new Point3D(33.2620575022,-13.8780771197,31.6068850505),
                 new Point3D(-32.7761967451,-1.7135891602,-25.2594741241), new Point3D(40.3308740195,33.9319649879,-44.7280593779),
                 new Point3D(12.7556347835,-1.0294050547,1.2159811778), new Point3D(34.7156782904,29.8806430724,-35.8128121335),
                 new Point3D(31.7494628900,19.9217752716,-47.1541463139), new Point3D(-24.3210366395,4.2483800724,32.5590760183),
                 new Point3D(-2.1822321288,12.9387499722,-41.0341258842), new Point3D(38.9059011352,-20.0785135799,-12.8196146668),
                 new Point3D(33.6868674703,43.5023001278,32.5138226872), new Point3D(18.5007482010,-31.7255808430,-48.4859192774),
                 new Point3D(-10.4699793830,38.4837004725,-37.9871019362), new Point3D(45.2076778115,-21.2355282533,29.5110424854),
                 new Point3D(-1.2224281969,-25.9285000827,-38.3069340357), new Point3D(25.3836628193,1.7408597645,-28.2493691271)};

            // The input point cloud was generated by transforming the expected result to the patient support frame
            var iso = new Point3D(15.7361843197, 20.2895968538, -18.6506591853);
            Point3D[] pointCloud = new Point3D[]{ new Point3D(18.8090287734,-12.0457668563,-6.6738779191),
                 new Point3D(23.5457265681,-13.3543614889,-85.4404921798), new Point3D(50.6136606267,3.6768697983,-66.9416765851),
                 new Point3D(0.8751465236,-7.8686358318,-2.0311780913), new Point3D(81.3920945932,18.1308467715,-27.2847484487),
                 new Point3D(30.9345583616,-9.6944080886,30.1495089310), new Point3D(58.2915857686,9.3916698746,-28.7563886988),
                 new Point3D(34.1108012103,66.5862479060,-10.2973954345), new Point3D(-1.1006616348,56.0126181910,-70.5826205591),
                 new Point3D(-31.5635839980,1.3591338544,-11.6844369491), new Point3D(22.2128644003,-11.3172881967,-54.1051103721),
                 new Point3D(-3.5521486412,45.5490709778,7.9044848073), new Point3D(66.1596929749,65.0176562317,-33.9943035379),
                 new Point3D(21.8200692172,19.0736156760,-29.9090888293), new Point3D(59.7037137398,56.1024089873,-31.4915122052),
                 new Point3D(49.7359877318,67.4437431677,-34.4278227996), new Point3D(6.0529897219,-12.2694791645,4.0605120492),
                 new Point3D(25.3990043065,61.3237227380,-9.7735370274), new Point3D(20.0865983828,33.1092115206,-62.2154356475),
                 new Point3D(70.5674660664,-12.2242258334,-23.2097926964), new Point3D(-0.7948969370,68.7755161312,-51.4456819221),
                 new Point3D(42.3089717838,58.2766987900,11.0899298594), new Point3D(22.5484377974,-9.2214456316,-68.1307056269),
                 new Point3D(-6.6750676842,58.5965308895,-31.7471178996), new Point3D(31.0211257901,48.5389659808,-38.9910499163)};

            // Act and Assert
            var actual = BeamGeometry.PointCloudToFixed(pointCloud, iso, table);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(0, (expected[i] - actual[i]).Length, 1e-6,$"{i} ({pointCloud[i].X},{pointCloud[i].Y},{pointCloud[i].Z})");
            
        }

        [TestMethod()]
        public void PointCloudBoundingRectangleTest()
        {
            // Arrange: generated using MATLAB
            double gantry = 229.20;
            double collimator = 202.52;

            // The expected result in the beams-eye-view was created by randomly generating 25 points
            // in the beam limiting device frame, projecting to the isocenter (BEV) plane, and finding the bounding box.
            var expected = new VRect<double>(-48.2480708567, -45.0426647290, 52.5609960366, 47.0385085309);

            // The input point cloud was generated by transforming the points from the beam limiting device frame to the fixed frame
            Point3D[] pointCloud = new Point3D[]{ new Point3D(-10.0605004441,-7.0939017746,25.0474904839),
                 new Point3D(-0.1801820007,-53.5887765457,-9.7046127158), new Point3D(-14.0703957012,-45.7083136819,-45.8810316959),
                 new Point3D(-21.8082167996,47.8614041359,-14.7416183650), new Point3D(-30.9850893664,-39.0643048393,-3.6891236835),
                 new Point3D(46.2495547084,17.7430583617,-36.4575245200), new Point3D(7.5022659965,20.5238755948,-76.5958960990),
                 new Point3D(-0.9787039497,-23.6583780492,27.2866286525), new Point3D(38.5166822181,14.7260558930,-12.4968658216),
                 new Point3D(28.6957321617,24.9507591652,-28.0235873283), new Point3D(2.8434705169,33.5467527277,41.1436989507),
                 new Point3D(-52.2160490046,30.3828281370,4.7725232516), new Point3D(-28.9979406745,-23.1104412238,51.7745993492),
                 new Point3D(-19.1301758386,32.7675424602,-42.1626428230), new Point3D(9.8304692841,8.7801005548,-9.3440397950),
                 new Point3D(-38.6826997822,20.7846660381,-23.7909874993), new Point3D(22.0327630391,12.5952127270,-0.2731761914),
                 new Point3D(35.7795541095,-23.0413750909,-5.3586561668), new Point3D(33.5156583149,8.7459293610,6.7912304643),
                 new Point3D(33.3139366252,-43.9850542262,1.8802194985), new Point3D(-24.4188144145,-7.0112328295,39.8395555259),
                 new Point3D(-1.8265872986,-16.7467690011,-42.6040043115), new Point3D(-10.5455506272,43.0560379503,17.1247981140),
                 new Point3D(18.3017729063,17.9267243280,19.3674966416), new Point3D(14.1647145115,14.1473034989,-39.8463302617)};
            
            // Act and Assert
            var actual = BeamGeometry.PointCloudBoundingRectangle(pointCloud, gantry, collimator);
            Assert.AreEqual(expected.X1, actual.X1, 1e-6, "X1");
            Assert.AreEqual(expected.X2, actual.X2, 1e-6, "X2");
            Assert.AreEqual(expected.Y1, actual.Y1, 1e-6, "Y1");
            Assert.AreEqual(expected.Y2, actual.Y2, 1e-6, "Y2");
        }
    }
}