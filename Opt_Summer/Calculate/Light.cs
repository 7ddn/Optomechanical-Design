using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Opt_Summer.Calculate
{
    public class Light
    {
        public double U;
        public double L;
        public double NowRefraction;

        public Light(double nowRefraction, double l, double u)
        {
            NowRefraction = nowRefraction;
            L = l;
            U = u;
        }

        public Light Paraxial(Lens len, double a, char ty)
        {
            double nextRefraction = 1;
            switch (ty)
            {
                case 'd':
                {
                    nextRefraction = len.Refractiond;
                    break;
                }
                case 'C':
                {
                    nextRefraction = len.RefractionC;
                    break;
                }
                case 'F':
                {
                    nextRefraction = len.RefractionF;
                    break;
                }
            }
            double i = (this.L - len.Radius) / len.Radius * this.U;
            // MessageBox.Show(L.ToString() +" "+len.Radius+" "+U+" "+i);
            if (this.L >= Utility.Infinity) i = a / len.Radius;
            // ReSharper disable once InconsistentNaming
            double i_ = this.NowRefraction / nextRefraction * i;
            // ReSharper disable once InconsistentNaming
            double u_ = this.U + i - i_;
            // ReSharper disable once InconsistentNaming
            double l_ = len.Radius + len.Radius * i_ / u_;
            return new Light(nextRefraction, l_ - len.Thickness, u_);
        }

        public Light Actual(Lens len, double h1, char ty)
        {
            double nextRefraction = 1;
            switch (ty)
            {
                case 'd':
                {
                    nextRefraction = len.Refractiond;
                    break;
                }
                case 'C':
                {
                    nextRefraction = len.RefractionC;
                    break;
                }
                case 'F':
                {
                    nextRefraction = len.RefractionF;
                    break;
                }
            }
            double sinI = (this.L - len.Radius) / len.Radius * Math.Sin(this.U);
            if (this.L >= Utility.Infinity) sinI = h1 / len.Radius;
            double sinI_ = this.NowRefraction / nextRefraction * sinI;
            double U_ = U + Math.Asin(sinI)-Math.Asin(sinI_);
            double L_ = len.Radius + len.Radius * sinI_ / Math.Sin(U_);
            return new Light(nextRefraction, L_ - len.Thickness, U_);
        }

        public List<double> GetActualArgs(Lens len, double h1)
        {
            double nextRefraction = len.Refractiond;
            double sinI = (this.L - len.Radius) / len.Radius * Math.Sin(this.U);
            if (this.L >= Utility.Infinity) sinI = h1 / len.Radius;
            double sinI_ = this.NowRefraction / nextRefraction * sinI;
            double U_ = U + Math.Asin(sinI)-Math.Asin(sinI_);
            double L_ = len.Radius + len.Radius * sinI_ / Math.Sin(U_);
            return new List<double>{Math.Asin(sinI),Math.Asin(sinI_),U_,L_};
        }
        
      
    }
}