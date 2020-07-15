using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

        public static double ParseInfinity(object value, double defaultValue)
        {
            if (value == null) return defaultValue;
            var v = value.ToString().ToUpper();
            //MessageBox.Show(IsNumeric(v) + " " + v);
            if (v != "INFINITY" && !IsNumeric(v))
            {
                throw new ArgumentException("Input Value must be INFINITY or numbers");
            }
            return v == "INFINITY" ? Infinity : double.Parse(value.ToString());
        }

        public static double ParseInfinity(object value)
        {
            return ParseInfinity(value, 0);
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
            var pa = startLight.L * Math.Sin(lightAngle1) / Math.Cos((i1 - lightAngle1) / 2);
            var x = pa * pa / 2 * lenses[1].Radius;
            double t;
            var s = t = (lenses[0].Thickness - x) / Math.Cos(lightAngle1);
            double tNext = 0, sNext = 0;
            var args = startLight.GetActualArgs(lenses[1], diaphragm / 2);
            var xNext = x;
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                x = xNext;
                tNext = lenses[i].Refractiond * Math.Pow(Math.Cos(args[1]), 2) /
                        ((lenses[i].Refractiond * Math.Cos(args[1]) - startLight.NowRefraction * Math.Cos(args[0])) /
                            lenses[i].Radius + startLight.NowRefraction * Math.Pow(Math.Cos(args[0]), 2) / t);
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

        public static bool SaveData(DataGridView lensList, List<KeyValuePair<string,string>> otherArgs)
        {
            var saveFileDialog = new SaveFileDialog
                {Filter = Resources.saveFormatFilter, Title = Resources.saveDataTitle, InitialDirectory = Application.StartupPath};
            saveFileDialog.ShowDialog();

            var fileName = saveFileDialog.FileName;
            if (fileName == "")
            {
                MessageBox.Show(Resources.BlankFileNameError);
                return false;
            }

            if (fileName.EndsWith(".txt"))
            {
                //save as json
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
                var otherArgsJson = new JObject();
                foreach (var item in otherArgs)
                {
                    otherArgsJson.Add(item.Key,item.Value);
                }
                jArray.Add(otherArgsJson);
            

                var sw = new StreamWriter(fileName, true, Encoding.UTF8);
                sw.Write(jArray.ToString());
                sw.Close();
                MessageBox.Show(Resources.saveSucceeded);
                return true;
            }
            else
            {
                // save as excel
                IWorkbook workbook = new XSSFWorkbook();
                try
                {
                    var sheet = workbook.CreateSheet("Sheet0");
                    var rowCount = lensList.RowCount + 3;
                    var columnCount = lensList.ColumnCount;
                    var excelRow = sheet.CreateRow(0);
                    ICell excelCell;
                    for (var i = 0; i < otherArgs.Count; i++)
                    {
                        excelCell = excelRow.CreateCell(i);
                        excelCell.SetCellValue(otherArgs[i].Key);
                    }

                    excelRow = sheet.CreateRow(1);
                    for (var i = 0; i < otherArgs.Count; i++)
                    {
                        excelCell = excelRow.CreateCell(i);
                        excelCell.SetCellValue(otherArgs[i].Value);
                    }

                    excelRow = sheet.CreateRow(2);
                    for (var i = 0; i < columnCount; i++)
                    {
                        excelCell = excelRow.CreateCell(i);
                        excelCell.SetCellValue(lensList.Columns[i].Name);
                    }

                    for (var i = 3; i < rowCount; i++)
                    {
                        var row = lensList.Rows[i - 3];
                        excelRow = sheet.CreateRow(i);
                        excelRow.CreateCell(0).SetCellValue((row.Cells[0].Value ?? "").ToString());
                        excelRow.CreateCell(1).SetCellValue((row.Cells[1].Value ?? "INFINITY").ToString());
                        excelRow.CreateCell(2).SetCellValue((row.Cells[2].Value ?? "0").ToString());
                        excelRow.CreateCell(3).SetCellValue((row.Cells[3].Value ?? "1").ToString());
                        excelRow.CreateCell(4).SetCellValue((row.Cells[4].Value ?? "1").ToString());
                        excelRow.CreateCell(5).SetCellValue((row.Cells[5].Value ?? "1").ToString());
                    }

                    using (var fs = File.OpenWrite(fileName))
                    {
                        workbook.Write(fs);
                        MessageBox.Show(Resources.saveSucceeded);
                        return true;
                    }
                }
                catch
                {
                    MessageBox.Show(Resources.saveError);
                    return false;
                }
            }
            
            
        }
    }
}