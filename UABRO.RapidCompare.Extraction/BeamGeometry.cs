using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    static public class BeamGeometry
    {
        static public double SourceAxisDistance = 1000.0;
        /// <summary>
        /// Calculate the beams-eye-view projection of a point cloud in the IEC fixed coordinate system.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="gantryAngle"></param>
        /// <param name="collimatorAngle"></param>
        /// <returns></returns>
        static public Point3D[] PointCloudToBEV(Point3D[] points, double gantryAngle, double collimatorAngle)
        {
            var M = CoordinateTransform.FixedToGantry(gantryAngle);
            M.Append(CoordinateTransform.GantryToBeamLimitingDevice(collimatorAngle));
            var result = new Point3D[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = M.Transform(points[i]);
                double mag = SourceAxisDistance / (SourceAxisDistance - result[i].Z);
                result[i].X *= mag;
                result[i].Y *= mag;
                result[i].Z = 0;
            }
            return result;
        }

        /// <summary>
        /// Transform a point cloud in the DICOM patient support coordinate system to the IEC fixed coordinate system.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="isocenter"></param>
        /// <param name="patientSupportAngle"></param>
        /// <returns></returns>
        static public Point3D[] PointCloudToFixed(Point3D[] points, Point3D isocenter, double patientSupportAngle)
        {
            var M = CoordinateTransform.PatientDicomToIEC(isocenter);
            M.Append(CoordinateTransform.PatientSupportToFixed(patientSupportAngle));
            var result = new Point3D[points.Length];
            for (int i = 0; i < points.Length; i++)
                result[i] = M.Transform(points[i]);

            return result;
        }

        /// <summary>
        /// Get the beams-eye-view rectangle bounding a point cloud in the IEC fixed coordinate system.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="gantryAngle"></param>
        /// <param name="collimatorAngle"></param>
        /// <returns></returns>
        static public VRect<double> PointCloudBoundingRectangle(Point3D[] points, double gantryAngle, double collimatorAngle)
        {
            var bev = PointCloudToBEV(points, gantryAngle, collimatorAngle);

            var x1 = bev.Min(p => p.X);
            var x2 = bev.Max(p => p.X);
            var y1 = bev.Min(p => p.Y);
            var y2 = bev.Max(p => p.Y);

            return new VRect<double>(x1, y1, x2, y2);
        }

        /// <summary>
        /// Get the beams-eye-view rectangles for an array of gantry angles that bounds a point cloud in the IEC fixed coordinate system.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="gantryAngles"></param>
        /// <param name="collimatorAngle"></param>
        /// <returns></returns>
        static public VRect<double>[] PointCloudBoundingRectangle(Point3D[] points, double[] gantryAngles, double collimatorAngle)
        {
            var result = new VRect<double>[gantryAngles.Length];
            for (int i = 0; i < gantryAngles.Length; i++)
                result[i] = PointCloudBoundingRectangle(points, gantryAngles[i], collimatorAngle);

            return result;
        }

        /// <summary>
        /// Get the beams-eye-view rectangles for an arc defined by an array of gantry angles for an array of collimator angles that bounds
        /// a point cloud in the IEC fixed coordinate system.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="s"></param>
        /// <param name="collimatorAngles"></param>
        /// <returns></returns>
        static public VRect<double>[] PointCloudBoundingRectangle(Point3D[] points, double[] gantryAngles, double[] collimatorAngles)
        {
            var result = new VRect<double>[collimatorAngles.Length];
            for (int i = 0; i < collimatorAngles.Length; i++)
            {
                var bev = PointCloudBoundingRectangle(points, gantryAngles, collimatorAngles[i]);
                var x1 = bev.Min(p => p.X1);
                var x2 = bev.Max(p => p.X2);
                var y1 = bev.Min(p => p.Y1);
                var y2 = bev.Max(p => p.Y2);
                result[i] = new VRect<double>(x1, y1, x2, y2);
            }
            return result;
        }

        /// <summary>
        /// Returns the jaw positions that encompass a set of target structures
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="isocenter"></param>
        /// <param name="gantryAngle"></param>
        /// <param name="collimatorAngle"></param>
        /// <param name="patientSupportAngle"></param>
        /// <returns></returns>
        static public VRect<double> GetJawPositions(List<Structure> targets, VVector isocenter,
            double gantryAngle, double collimatorAngle, double patientSupportAngle)
        {
            var iso = new Point3D(isocenter.x, isocenter.y, isocenter.z);
            var points = MeshPointsToArray(targets.ToArray());
            points = PointCloudToFixed(points, iso, patientSupportAngle);

            return PointCloudBoundingRectangle(points, gantryAngle, collimatorAngle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point3Ds"></param>
        /// <returns></returns>
        static public Point3D[] CopyPoint3DCollectionToArray(Point3DCollection point3Ds)
        {
            var result = new List<Point3D>(point3Ds.Count);
            foreach (var p in point3Ds)
                result.Add(new Point3D(p.X, p.Y, p.Z));
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structures"></param>
        /// <returns></returns>
        static public Point3D[] MeshPointsToArray(params Structure[] structures)
        {
            var result = new List<Point3D>();
            foreach (var s in structures.Where(s => s != null))
                result.AddRange(CopyPoint3DCollectionToArray(s.MeshGeometry.Positions));
            return result.ToArray();
        }
    }
}
