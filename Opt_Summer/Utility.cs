using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Opt_Summer
{
    public static class Utility
    {
        public const double Infinity = 1.0E15;

        public static bool IsNumberic(string value)
        {
            return Regex.IsMatch(value, @"[+-]?\d+(\.\d*)?");
        }

        public static double ParseInfinity(object value)
        {
            if (value == null) return 0;
            var v = value.ToString().ToUpper();
            //MessageBox.Show(IsNumberic(v) + " " + v);
            if (v != "INFINITY" && !IsNumberic(v))
            {
                throw new System.ArgumentException("Input Value must be INFINITY or numbers");
            }
            return v == "INFINITY" ? Infinity : double.Parse(value.ToString());
        }
    }
}