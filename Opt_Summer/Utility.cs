using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Opt_Summer.Calculate;
using Opt_Summer.Properties;

namespace Opt_Summer
{
    public static class Utility
    {
        public const double Infinity = 1.0E15;

        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"[+-]?\d+(\.\d*)?");
        }

        public static double ParseInfinity(object value)
        {
            if (value == null) return 0;
            var v = value.ToString().ToUpper();
            //MessageBox.Show(IsNumeric(v) + " " + v);
            if (v != "INFINITY" && !IsNumeric(v))
            {
                throw new ArgumentException("Input Value must be INFINITY or numbers");
            }
            return v == "INFINITY" ? Infinity : double.Parse(value.ToString());
        }
        
        public static Light RepeatedLightParaxial(Light startLight, char ty, List<Lens> lenses, double a)
        {
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                startLight = startLight.Paraxial(lenses[i], a, ty);
            }

            startLight.L = Math.Round(startLight.L, 10);

            return startLight;
        }

        public static Light RepeatedLightActual(Light startLight, char ty, List<Lens> lenses, double a)
        {
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                startLight = startLight.Actual(lenses[i], a, ty);
            }

            startLight.L = Math.Round(startLight.L, 10);

            return startLight;
        }

        public static List<double> Astigmatism(Light startLight, List<Lens> lenses, double diaphragm, double lightAngle1)
        {
            var i1 = startLight.GetActualArgs(lenses[1], diaphragm / 2)[0]; //i1
            // u1 = light.U;
            // L = lenses[0].Thickness;
            // var h1 = lenses[1].Radius * Math.Sin(u1 + i1);
            var pa = startLight.L * Math.Sin(lightAngle1) / Math.Cos((i1 - lightAngle1) / 2);
            var x = pa * pa / 2 * lenses[1].Radius;
            double t;
            var s = t = (lenses[0].Thickness - x) / Math.Cos(lightAngle1);
            double tNext = 0, sNext = 0;
            var args = startLight.GetActualArgs(lenses[1], diaphragm / 2);
            var xNext = x;
            for (int i = 1; i < lenses.Count - 1; i++)
            {
                x = xNext;
                tNext = lenses[i].Refractiond * Math.Pow(Math.Cos(args[1]), 2) /
                        ((lenses[i].Refractiond * Math.Cos(args[1]) - startLight.NowRefraction * Math.Cos(args[0])) /
                            lenses[i].Radius + startLight.NowRefraction * Math.Pow(Math.Cos(args[0]), 2) / t);
                //MessageBox.Show("r = " + lenses[i].Radius + " t‘’ = "+ t_);
                sNext = lenses[i].Refractiond /
                        ((lenses[i].Refractiond * Math.Cos(args[1]) - startLight.NowRefraction * Math.Cos(args[0])) /
                            lenses[i].Radius + startLight.NowRefraction / s);
                startLight = startLight.Actual(lenses[i], diaphragm / 2, 'd');
                args = startLight.GetActualArgs(lenses[i + 1], diaphragm / 2);
                pa = startLight.L * Math.Sin(startLight.U) / Math.Cos((args[0] - startLight.U) / 2);
                xNext = pa * pa / (2 * lenses[i + 1].Radius);
                var dv = (lenses[i].Thickness - x + xNext) / Math.Cos(startLight.U);

                t = tNext - dv;
                s = sNext - dv;
            }

            return new List<double>{tNext,sNext,x};
        }

        public static bool SaveData(DataGridView lensList, Control textBox1, Control textBox2, Control textBox3, Control textBox4, Control textBox5)
        {
            var jArray = new JArray();
            var lensData = new JArray();
            foreach (DataGridViewRow row in lensList.Rows)
            {
                var jObject = new JObject
                {
                    {"Type", (row.Cells[0].Value ?? "").ToString()},
                    {"Radius", (row.Cells[1].Value ?? "INFINITY").ToString()},
                    {"Thickness", (row.Cells[2].Value ?? "0").ToString()},
                    {"RefractionD", (row.Cells[3].Value ?? "1").ToString()},
                    {"RefractionC", (row.Cells[4].Value ?? "1").ToString()},
                    {"RefractionF", (row.Cells[5].Value ?? "1").ToString()}
                };
                lensData.Add(jObject);
            }

            jArray.Add(lensData);
            var otherArgs = new JObject
            {
                {"EntranceHeight", textBox1.Text},
                {"EntranceLocation", textBox3.Text},
                {"AOV", textBox2.Text},
                {"Aperture", textBox4.Text},
                {"Height", textBox5.Text}
            };
            jArray.Add(otherArgs);

            var saveFileDialog = new SaveFileDialog
                {Filter = Resources.textFormat, Title = Resources.saveDataTitle, InitialDirectory = Application.StartupPath};
            saveFileDialog.ShowDialog();

            var fileName = saveFileDialog.FileName;

            var sw = new StreamWriter(fileName, true, Encoding.UTF8);
            sw.Write(jArray.ToString());
            sw.Close();
            MessageBox.Show(Resources.saveSucceeded);
            return true;
        }
    }
}