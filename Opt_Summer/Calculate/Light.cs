using System;

namespace Opt_Summer.Calculate
{
    public class Light
    {
        public readonly double U;
        public readonly double L;
        public readonly double NowRefraction;

        public Light(double nowRefraction, double l, double u)
        {
            NowRefraction = nowRefraction;
            L = l;
            U = u;
        }

        public Light Paraxial(Lens len)
        {
            double i = (this.L - len.Radius) / len.Radius * this.U;
            double i_ = this.NowRefraction / len.Refraction * i;
            double u_ = this.U + i - i_;
            double l_ = len.Radius + len.Radius * i_ / u_;
            return new Light(len.Refraction, l_ - len.Thickness, u_);
        }

        public Light Actual(Lens len)
        {
            double sinI = (this.L - len.Radius) / len.Radius * Math.Sin(this.U);
            double sinI_ = this.NowRefraction / len.Refraction * sinI;
            double U_ = U + Math.Asin(sinI)-Math.Asin(sinI_);
            double L_ = len.Radius + len.Radius * sinI_ / Math.Sin(U_);
            return new Light(len.Refraction, L_ - len.Thickness, U_);
        }
    }
}