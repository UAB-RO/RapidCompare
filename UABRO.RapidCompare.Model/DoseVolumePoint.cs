
namespace UABRO.RapidCompare.Model
{
    public struct DoseVolumePoint
    {
        public double DoseGy;
        public double VolumePercent;

        public DoseVolumePoint(double doseGy, double volumePercent)
        {
            DoseGy = doseGy;
            VolumePercent = volumePercent;
        }
    }
}
