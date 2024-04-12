using System;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.Types;

namespace UABRO.RapidCompare.Extraction
{
    static public class CoordinateTransform
    {
        /// <summary>
        /// Transformation matrix from DICOM coordinate system to IEC.
        /// </summary>
        /// <param name="isocenter"></param>
        /// <returns></returns>
        static public Matrix3D PatientDicomToIEC(Point3D isocenter)
        {
            var M = new Matrix3D();
            M.M22 = 0;
            M.M23 = -1;
            M.M32 = 1;
            M.M33 = 0;

            M.OffsetX = -isocenter.X;
            M.OffsetY = -isocenter.Z;
            M.OffsetZ = isocenter.Y;

            return M;
        }

        /// <summary>
        /// Transformation matrix from IEC fixed coordinate system to IEC gantry coordinate system
        /// </summary>
        /// <param name="gantry"></param>
        /// <returns></returns>
        static public Matrix3D FixedToGantry(double gantry)
        {
            double phi = Math.PI * gantry / 180;

            double c = Math.Cos(phi);
            double s = Math.Sin(phi);
            var Mfg = new Matrix3D();
            Mfg.M11 = c;
            Mfg.M31 = -s;
            Mfg.M13 = s;
            Mfg.M33 = c;

            return Mfg;
        }

        /// <summary>
        /// Transformation matrix from IEC gantry coordinate system to IEC beam limiting device coordinate system
        /// </summary>
        /// <param name="collimator"></param>
        /// <returns></returns>
        static public Matrix3D GantryToBeamLimitingDevice(double collimator)
        {
            double theta = Math.PI * collimator / 180;

            double c = Math.Cos(theta);
            double s = Math.Sin(theta);
            var Mgb = new Matrix3D();
            Mgb.M11 = c;
            Mgb.M21 = s;
            Mgb.M12 = -s;
            Mgb.M22 = c;

            return Mgb;
        }

        static public Matrix3D PatientSupportToFixed(double table)
        {
            double theta = Math.PI * table / 180;

            double c = Math.Cos(theta);
            double s = Math.Sin(theta);
            var Msf = new Matrix3D();
            Msf.M11 = c;
            Msf.M21 = -s;
            Msf.M12 = s;
            Msf.M22 = c;

            return Msf;
        }

        /// <summary>
        /// Provides the transformation from patient DICOM coordinates to IEC coordinates, adjusted for the isocenter position.
        /// Useful for rendering structures.
        /// </summary>
        /// <param name="orient">patient orienation</param>
        /// /// <param name="iso">isocenter position</param>
        /// <returns>4x4 transformation matrix</returns>
        public static Matrix3D DICOM2IEC_IsoAdjusted(PatientOrientation orient, VVector iso)
        {
            var basic = DICOM2IEC(orient);
            var offset = Matrix3D.Identity;
            offset.OffsetX = -iso.x;
            offset.OffsetY = -iso.y;
            offset.OffsetZ = -iso.z;

            return Matrix3D.Multiply(offset, basic);
        }
        /// <summary>
        /// Provides the transformation from patient DICOM coordinates to IEC coordinates. Useful for rendering
        /// structures.
        /// </summary>
        /// <param name="orient"></param>
        /// <returns>4x4 transformation matrix</returns>
        public static Matrix3D DICOM2IEC(PatientOrientation orient)
        {
            switch (orient)
            {
                case PatientOrientation.HeadFirstSupine:
                    return new Matrix3D(
                        1, 0, 0, 0,
                        0, 0, -1, 0,
                        0, 1, 0, 0,
                        0, 0, 0, 1);
                case PatientOrientation.HeadFirstProne:
                    return new Matrix3D(
                       -1, 0, 0, 0,
                       0, 0, 1, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.FeetFirstSupine:
                    return new Matrix3D(
                       -1, 0, 0, 0,
                       0, 0, -1, 0,
                       0, -1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.FeetFirstProne:
                    return new Matrix3D(
                       1, 0, 0, 0,
                       0, 0, 1, 0,
                       0, -1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.HeadFirstDecubitusLeft:
                    return new Matrix3D(
                       0, 0, -1, 0,
                       -1, 0, 0, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.HeadFirstDecubitusRight:
                    return new Matrix3D(
                       0, 0, 1, 0,
                       1, 0, 0, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.FeetFirstDecubitusLeft:
                    return new Matrix3D(
                       0, 0, -1, 0,
                       1, 0, 0, 0,
                       0, -1, 0, 0,
                       0, 0, 0, 1);
                case PatientOrientation.FeetFirstDecubitusRight:
                    return new Matrix3D(
                       0, 0, 1, 0,
                       -1, 0, 0, 0,
                       0, -1, 0, 0,
                       0, 0, 0, 1);
                default: throw new Exception("Don't have transform for this orientation!");
            }
        }
    }
} 

