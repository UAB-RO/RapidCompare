using System;
using System.Linq;

namespace UABRO.RapidCompare.Model.Helpers
{
    public static class DvhInterpolation
    {
        private static double sMaximumVolumeDeviationFromMonotonicDecrease = 1.0e-6;

        /// <summary>
        /// Interpolates dose at a specified volume.
        /// </summary>
        /// <param name="dose"></param>
        /// <param name="volume"></param>
        /// <param name="interpolateatdose"></param>
        /// <returns></returns>
        static internal double[] InterpolateVolume(double[] dose, double[] volume, double[] interpolateatdose)
        {
            if (dose.Length != volume.Length)
                throw new ArgumentException("Dose and volume arrays are not the same length");

            if (!dose.Skip(1).Zip(dose.Take(dose.Length - 1), (first, second) => first - second).All(s => s > 0))
                throw new ArgumentException("Dose array is not monotonically increasing");

            if (!volume.Skip(1).Zip(volume.Take(volume.Length - 1), (first, second) => first - second).All(s => s < sMaximumVolumeDeviationFromMonotonicDecrease))
                throw new ArgumentException("Volume array is not monotonically decreasing");

            if (dose[0] != 0)
                throw new ArgumentException("Dose array must start at zero");

            if (interpolateatdose.Any(s => s < 0))
                throw new ArgumentException("Cannot interpolate for dose values less than zero");

            double[] interpolatedvolume = new double[interpolateatdose.Length];
            for (int i = 0; i < interpolateatdose.Length; i++)
            {
                int j = Array.IndexOf(dose, dose.Where(d => d <= interpolateatdose[i]).Last());
                j = j > dose.Length - 2 ? dose.Length - 2 : j;

                interpolatedvolume[i] = volume[j] + (interpolateatdose[i] - dose[j]) * (volume[j + 1] - volume[j]) / (dose[j + 1] - dose[j]);
                interpolatedvolume[i] = interpolatedvolume[i] > 0 ? interpolatedvolume[i] : 0;
            }
            return interpolatedvolume;
        }

        /// <summary>
        /// Interpolates dose for a specified volume. For volumes where the dose is not unique, returns
        /// the maximum dose at the specified volume.
        /// </summary>
        /// <param name="dose"></param>
        /// <param name="volume"></param>
        /// <param name="interpolateatvolume"></param>
        /// <returns></returns>
        static internal double[] InterpolateDose(double[] dose, double[] volume, double[] interpolateatvolume)
        {
            if (dose.Length != volume.Length)
                throw new ArgumentException("Dose and volume arrays are not the same length");

            if (!dose.Skip(1).Zip(dose.Take(dose.Length - 1), (first, second) => first - second).All(s => s > 0))
                throw new ArgumentException("Dose array is not monotonically increasing");

            if (!volume.Skip(1).Zip(volume.Take(volume.Length - 1), (first, second) => first - second).All(s => s < sMaximumVolumeDeviationFromMonotonicDecrease))
                throw new ArgumentException("Volume array is not monotonically decreasing");

            if (dose[0] != 0)
                throw new ArgumentException("Dose array must start at zero");

            if (interpolateatvolume.Any(s => s < 0))
                throw new ArgumentException("Cannot interpolate for volume values less than zero");

            if (interpolateatvolume.Any(s => s > volume[0]))
                throw new ArgumentException("Cannot interpolate for volume values greater than the maximum volume");

            double[] interpolateddose = new double[interpolateatvolume.Length];
            for (int i = 0; i < interpolateatvolume.Length; i++)
            {
                int j;
                if (interpolateatvolume[i] == volume[0])
                {
                    j = Array.LastIndexOf(volume, volume[0]);
                    interpolateddose[i] = dose[j];
                }
                else
                {
                    j = Array.LastIndexOf(volume, volume.Where(v => v >= interpolateatvolume[i]).Last());
                    j = j > volume.Length - 2 ? volume.Length - 2 : j;

                    interpolateddose[i] = dose[j] + (interpolateatvolume[i] - volume[j]) * (dose[j + 1] - dose[j]) / (volume[j + 1] - volume[j]);
                    interpolateddose[i] = interpolateddose[i] > 0 ? interpolateddose[i] : 0;
                }
            }
            return interpolateddose;
        }
    }
}
