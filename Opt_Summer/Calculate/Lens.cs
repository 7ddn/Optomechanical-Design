namespace Opt_Summer.Calculate
{
    public class Lens
    {
        public readonly double Radius;
        public readonly double Refractiond;
        public readonly double RefractionC;
        public readonly double RefractionF;
        public readonly double Thickness;

        public Lens(double radius, double refractiond, double thickness, double refractionC, double refractionF)
        {
            Radius = radius;
            Refractiond = refractiond;
            Thickness = thickness;
            RefractionC = refractionC;
            RefractionF = refractionF;
        }
    }
}