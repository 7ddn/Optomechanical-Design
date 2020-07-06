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
        public static double Infinity = 1.0E15;

        public static bool IsNumberic(string value)
        {
            return Regex.IsMatch(value, @"^[+-}?/d*[.]?/d*$");
        }

    }
    
}