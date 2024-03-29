﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using NPOI.XSSF.UserModel;
using Opt_Summer.Calculate;
using Opt_Summer.Properties;

namespace Opt_Summer
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            panel1.BringToFront();
            panel2.SendToBack();

            lensList.ColumnCount = 6;
            lensList.ColumnHeadersVisible = true;

            lensList.Columns[0].Name = "表面类型";
            lensList.Columns[1].Name = "曲率半径";
            lensList.Columns[2].Name = "厚度";
            lensList.Columns[3].Name = "d光折射率";
            lensList.Columns[4].Name = "C光折射率";
            lensList.Columns[5].Name = "F光折射率";

            for (int i = 0; i < 3; i++)
            {
                int index = lensList.Rows.Add();
                switch (i)
                {
                    case 0:
                        lensList.Rows[index].Cells[0].Value = "物面";
                        break;
                    case 1:
                        lensList.Rows[index].Cells[0].Value = "光阑";
                        break;
                    case 2:
                        lensList.Rows[index].Cells[0].Value = "像面";
                        break;
                }

                lensList.Rows[index].Cells[1].Value = "INFINITY";
                lensList.Rows[index].Cells[2].Value = "0";
                lensList.Rows[index].Cells[3].Value = "1";
                lensList.Rows[index].Cells[4].Value = "1";
                lensList.Rows[index].Cells[5].Value = "1";
            }

            lensList.Rows[0].Cells[2].Value = "INFINITY";

            lensList.AllowUserToAddRows = false;
            lensList.AllowUserToDeleteRows = false;
            lensList.MultiSelect = false;
            lensList.Columns[0].ReadOnly = true;

            KeyPreview = true;
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        private void AddRow()
        {
            lensList.Rows.Insert(lensList.RowCount - 1, 1);
            var index = lensList.RowCount - 2;
            lensList.Rows[index].Cells[1].Value = "INFINITY";
            lensList.Rows[index].Cells[2].Value = "0";
            lensList.Rows[index].Cells[3].Value = "1";
            lensList.Rows[index].Cells[4].Value = "1";
            lensList.Rows[index].Cells[5].Value = "1";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var row = lensList.CurrentRow;
            if (row == null) return;
            DeleteRow(row);
        }

        private void DeleteRow(DataGridViewRow row)
        {
            if (row.Index == 1)
            {
                if (lensList.RowCount == 3)
                {
                    const string message = "不能删除最后一个透镜";
                    MessageBox.Show(message);
                    return;
                }

                lensList.Rows.Remove(row);
                lensList.Rows[1].Cells[0].Value = "光阑";
                return;
            }

            if (row.Index != 0 && row.Index != lensList.RowCount - 1)
            {
                lensList.Rows.Remove(row);
            }
            else
            {
                var message = "不能删除" + (row.Index == 0 ? "物面" : "像面");
                MessageBox.Show(message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Calc();
        }

        private void Calc()
        {
            var lenses = new List<Lens>();

            //获取透镜值
            try
            {
                lenses.AddRange(from DataGridViewRow row in lensList.Rows
                    let radius = Utility.ParseInfinity(row.Cells[1].Value, Utility.Infinity)
                    let refractionD = Utility.ParseInfinity(row.Cells[3].Value, 1)
                    let refractionC = Utility.ParseInfinity(row.Cells[4].Value, 1)
                    let refractionF = Utility.ParseInfinity(row.Cells[5].Value, 1)
                    let thickness = Utility.ParseInfinity(row.Cells[2].Value, 0)
                    select new Lens(radius, refractionD, thickness, refractionC, refractionF));
            }
            catch
            {
                MessageBox.Show(Resources.InputValueError);
                return;
            }


            // 获取外部参数
            var angelOfViewObject = Utility.ParseInfinity(textBox2.Text);
            var diaphragm = Utility.ParseInfinity(textBox1.Text);
            var lengthExitPupil = Utility.ParseInfinity(textBox3.Text);
            var angularAperture = Utility.ParseInfinity(textBox4.Text) * Math.PI / 180;
            //MessageBox.Show(u.ToString());
            var objectHeight = Utility.ParseInfinity(textBox5.Text);

            //生成新窗口
            var dataAnalysis = new DataAnalysis();

            //计算焦距
            var length1 = Utility.Infinity;
            double lightAngle1 = 0;
            var startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            double focalLengthImage = 1;
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                startLight = startLight.Paraxial(lenses[i], diaphragm / 2, 'd');
                focalLengthImage = focalLengthImage * (lenses[i].Thickness + startLight.L) / startLight.L;
            }

            focalLengthImage *= startLight.L; //乘上多除的最后一面物距

            // 更新焦距
            dataAnalysis.Data1 = Math.Round(focalLengthImage, 10).ToString(); //焦距
            dataAnalysis.Data2 = Math.Round(startLight.L - focalLengthImage, 10).ToString(); //像方主面位置
            
            //计算第二近轴光线
            length1 = lengthExitPupil;
            lightAngle1 = Math.Abs(lenses[0].Thickness - Utility.Infinity) < 10E-5?Math.Sin(angelOfViewObject):Math.Asin(objectHeight/(lengthExitPupil-lenses[0].Thickness));
            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            // 更新出瞳距
            dataAnalysis.Data3 = Utility.RepeatedLightParaxial(startLight, 'd', lenses, diaphragm / 2).L.ToString(); // 出瞳距

            // 计算第一近轴光线

            length1 = lenses[0].Thickness;
            double heightIdealImageFullAperture = 0, heightIdealImage07Aperture = 0;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = 0;
                heightIdealImageFullAperture =
                    Math.Round(focalLengthImage * Math.Tan(angelOfViewObject * Math.PI / 180), 10);
                dataAnalysis.Data4 = heightIdealImageFullAperture.ToString(); // 全视场理想像高
                heightIdealImage07Aperture =
                    Math.Round(focalLengthImage * Math.Tan(angelOfViewObject * 0.7 * Math.PI / 180), 10);
                dataAnalysis.Data5 = heightIdealImage07Aperture.ToString(); // 0.7视场理想像高
            }
            else
            {
                lightAngle1 = Math.Sin(angularAperture);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            var lightD = Utility.RepeatedLightParaxial(startLight, 'd', lenses, diaphragm / 2);
            var lengthLightD = lightD.L;
            dataAnalysis.DataD1 = Math.Round(lengthLightD, 10).ToString(); //d光理想像距
            
            startLight = new Light(lenses[0].RefractionF, length1, lightAngle1);
            var lengthLightF = Utility.RepeatedLightParaxial(startLight, 'F', lenses, diaphragm / 2).L;
            dataAnalysis.DataF1 = lengthLightF.ToString(); //F光理想像距
            
            startLight = new Light(lenses[0].RefractionC, length1, lightAngle1);
            var lengthLightC = Utility.RepeatedLightParaxial(startLight, 'C', lenses, diaphragm / 2).L;
            dataAnalysis.DataC1 = lengthLightC.ToString(); //C光理想像距

            if (Math.Abs(length1 - Utility.Infinity) > 1.0E-05)
            {
                startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
                double beta = 1;
                for (var i = 1; i < lenses.Count - 1; i++)
                {
                    beta /= startLight.L;
                    startLight = startLight.Paraxial(lenses[i], diaphragm / 2, 'd');
                    beta *= (startLight.L + lenses[i].Thickness);
                }

                heightIdealImageFullAperture = Math.Abs(objectHeight * beta);
                dataAnalysis.Data4 = heightIdealImageFullAperture.ToString(); // 全视场理想像高
                heightIdealImage07Aperture = Math.Abs(objectHeight * 0.7 * beta);
                dataAnalysis.Data5 = heightIdealImage07Aperture.ToString(); // 0.7视场理想像高
            }

            dataAnalysis.Data10 = Math.Round(lengthLightF - lengthLightC, 10)
                .ToString(); //零孔径位置色差


            // 计算轴上点实际光路
            double kEta = 1; //取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                length1 = Utility.Infinity;
                lightAngle1 = 0;
            }
            else
            {
                length1 = lenses[0].Thickness;
                lightAngle1 = Math.Asin(kEta * Math.Sin(angularAperture));
            }
            
            // 更新全孔径实际光线相关信息
            
            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            var lengthLightDFullAperture = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataD2 = lengthLightDFullAperture.ToString(); // d光全孔径实际像距
            
            startLight = new Light(lenses[0].RefractionF, length1, lightAngle1);
            var lengthLightFFullAperture = Utility.RepeatedLightActual(startLight, 'F', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataF2 = lengthLightFFullAperture.ToString(); // F光全孔径实际像距
            
            startLight = new Light(lenses[0].RefractionC, length1, lightAngle1);
            var lengthLightCFullAperture = Utility.RepeatedLightActual(startLight, 'C', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataC2 = lengthLightCFullAperture.ToString(); // C光全孔径实际像距
            dataAnalysis.Data6 = Math.Round(lengthLightDFullAperture - lengthLightD, 10).ToString(); //全孔径球差
            dataAnalysis.Data8 =
                Math.Round(lengthLightFFullAperture - lengthLightCFullAperture, 10).ToString(); // 全孔径位置色差


            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                length1 = Utility.Infinity;
                lightAngle1 = 0;
            }
            else
            {
                length1 = lenses[0].Thickness;
                lightAngle1 = Math.Asin(kEta * Math.Sin(angularAperture));
            }
            
            // 更新0.7孔径实际光线相关信息
            
            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            var lengthLightD07Aperture = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataD3 = lengthLightD07Aperture.ToString(); // d光0.7孔径实际像距
            
            startLight = new Light(lenses[0].RefractionF, length1, lightAngle1);
            var lengthLightF07Aperture = Utility.RepeatedLightActual(startLight, 'F', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataF3 = lengthLightF07Aperture.ToString(); // F光0.7孔径实际像距
            
            startLight = new Light(lenses[0].RefractionC, length1, lightAngle1);
            var lengthLightC07Aperture = Utility.RepeatedLightActual(startLight, 'C', lenses, kEta * diaphragm / 2).L;
            dataAnalysis.DataC3 = lengthLightC07Aperture.ToString(); // C光0.7孔径实际像距
            dataAnalysis.Data7 = Math.Round(lengthLightD07Aperture - lengthLightD, 10).ToString(); //0.7孔径球差
            dataAnalysis.Data9 = Math.Round(lengthLightF07Aperture - lengthLightC07Aperture, 10)
                .ToString(); // 0.7孔径位置色差

            // 计算轴外光全视场实际光路
            // ReSharper disable once InconsistentNaming
            double Kw = 1; //视场取点系数
            kEta = 0; //孔径取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            var dLight = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            var heightDActualImageFullAperture =
                Math.Abs(Math.Round((dLight.L - lengthLightD) * Math.Tan(dLight.U), 10));
            dataAnalysis.DataD4 = heightDActualImageFullAperture.ToString(); // d光全视场实际像高
            var distortionDFullAperture = Math.Round(heightDActualImageFullAperture - heightIdealImageFullAperture, 10);
            dataAnalysis.Data16 = distortionDFullAperture.ToString(); // d光全视场绝对畸变
            dataAnalysis.Data14 = Math.Round(distortionDFullAperture / heightIdealImageFullAperture, 10)
                .ToString(); //d光全视场相对畸变
            
            startLight = new Light(lenses[0].RefractionC, length1, lightAngle1);
            var cLight = Utility.RepeatedLightActual(startLight, 'C', lenses, kEta * diaphragm / 2);
            var heightCActualImageFullAperture = (cLight.L - lengthLightD) * Math.Tan(cLight.U);
            dataAnalysis.DataC4 =
                Math.Abs(Math.Round(heightCActualImageFullAperture, 10)).ToString(); // C光全视场实际像高
            
            startLight = new Light(lenses[0].RefractionF, length1, lightAngle1);
            var fLight = Utility.RepeatedLightActual(startLight, 'F', lenses, kEta * diaphragm / 2);
            var heightFActualImageFullAperture = (fLight.L - lengthLightD) * Math.Tan(fLight.U);
            dataAnalysis.DataF4 =
                Math.Abs(Math.Round(heightFActualImageFullAperture, 10)).ToString(); // F光全视场实际像高

            dataAnalysis.Data22 = Math.Round(heightFActualImageFullAperture - heightCActualImageFullAperture, 10)
                .ToString(); // 全视场倍率色差

            // 计算全视场彗差
            // 0.7孔径
            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            var light1U = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            var lu = Math.Abs(Math.Round((light1U.L - lengthLightD) * Math.Tan(light1U.U), 10));

            kEta = -0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            var light1d = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            var ldd = Math.Abs(Math.Round((light1d.L - lengthLightD) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data19 = Math.Round((lu + ldd) / 2 - heightDActualImageFullAperture, 10).ToString();

            // 全孔径
            kEta = 1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1U = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            lu = Math.Abs(Math.Round((light1U.L - lengthLightD) * Math.Tan(light1U.U), 10));

            kEta = -1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1d = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            ldd = Math.Abs(Math.Round((light1d.L - lengthLightD) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data18 = Math.Round((lu + ldd) / 2 - heightDActualImageFullAperture, 10).ToString();


            // 计算轴外光0.7视场实际光路
            Kw = 0.7; //视场取点系数
            kEta = 0; //孔径取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            dLight = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            var heightDImageActual07Aperture = Math.Abs(Math.Round((dLight.L - lengthLightD) * Math.Tan(dLight.U), 10));
            dataAnalysis.DataD5 = heightDImageActual07Aperture.ToString(); // d光0.7视场实际像高
            var distortionD07Aperture = Math.Round(heightDImageActual07Aperture - heightIdealImage07Aperture, 10);
            dataAnalysis.Data17 = distortionD07Aperture.ToString(); // d光0.7视场绝对畸变
            dataAnalysis.Data15 = Math.Round(distortionD07Aperture / heightIdealImage07Aperture, 10)
                .ToString(); //d光0.7视场相对畸变
            cLight = Utility.RepeatedLightActual(startLight, 'C', lenses, kEta * diaphragm / 2);
            var heightCActualImage07Aperture = Math.Abs(Math.Round((cLight.L - lengthLightD) * Math.Tan(cLight.U), 10));
            dataAnalysis.DataC5 = heightCActualImage07Aperture.ToString(); // C光0.7视场实际像高

            fLight = Utility.RepeatedLightActual(startLight, 'F', lenses, kEta * diaphragm / 2);
            var heightFActualImage07Aperture = Math.Abs(Math.Round((fLight.L - lengthLightD) * Math.Tan(fLight.U), 10));
            dataAnalysis.DataF5 = heightFActualImage07Aperture.ToString(); // F光0.7视场实际像高
            dataAnalysis.Data23 =
                Math.Round(heightFActualImage07Aperture - heightCActualImage07Aperture, 10)
                    .ToString(); // 0.7视场倍率色差

            // 计算0.7视场彗差
            // 0.7孔径
            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1U = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            lu = Math.Abs(Math.Round((light1U.L - lengthLightD) * Math.Tan(light1U.U), 10));

            kEta = -0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1d = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            ldd = Math.Abs(Math.Round((light1d.L - lengthLightD) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data21 = Math.Round((lu + ldd) / 2 - heightDImageActual07Aperture, 10).ToString();

            // 全孔径
            kEta = 1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1U = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            lu = Math.Abs(Math.Round((light1U.L - lengthLightD) * Math.Tan(light1U.U), 10));

            kEta = -1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);

            light1d = Utility.RepeatedLightActual(startLight, 'd', lenses, kEta * diaphragm / 2);
            ldd = Math.Abs(Math.Round((light1d.L - lengthLightD) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data20 = Math.Round((lu + ldd) / 2 - heightDImageActual07Aperture, 10).ToString();


            // 轴外点沿主光线的细光束成像位置的计算
            Kw = 1;
            kEta = 0;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                lightAngle1 = Kw * angelOfViewObject * Math.PI / 180;
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }
            else
            {
                lightAngle1 = Math.Atan((Kw * objectHeight - kEta * diaphragm / 2) /
                                        (lengthExitPupil - lenses[0].Thickness));
                length1 = lengthExitPupil + kEta * diaphragm / 2 / Math.Tan(lightAngle1);
            }

            startLight = new Light(lenses[0].Refractiond, length1, lightAngle1);
            
            var tsx = Utility.Astigmatism(startLight, lenses, diaphragm, lightAngle1);

            var xt = tsx[0] * Math.Cos(startLight.U) + tsx[2] - lengthLightD;
            var xs = tsx[1] * Math.Cos(startLight.U) + tsx[2] - lengthLightD;
            var dx = xt - xs;
            dataAnalysis.Data11 = Math.Round(xt, 10).ToString();
            dataAnalysis.Data12 = Math.Round(xs, 10).ToString();
            dataAnalysis.Data13 = Math.Round(dx, 10).ToString();


            // 弹出分析窗口
            dataAnalysis.Show();
        }

        private void LoadData()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    FileName = "选择要打开的文件",
                    Filter = Resources.loadFormatFilter,
                    Title = Resources.loadDataTitle
                };
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var fileName = openFileDialog.FileName;
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        MessageBox.Show(Resources.BlankFileNameError);
                        return;
                    }
                    if (fileName.EndsWith(".txt"))
                    {
                        // Read as JSON
                        var sr = new StreamReader(fileName);
                        var jsonContent = sr.ReadToEnd();
                        var jArray = JArray.Parse(jsonContent);
                        lensList.Rows.Clear();
                        foreach (var rowContent in jArray[0])
                        {
                            var index = lensList.Rows.Add();
                            lensList.Rows[index].Cells[0].Value = rowContent["Type"]?.ToString();
                            lensList.Rows[index].Cells[1].Value = rowContent["Radius"]?.ToString();
                            lensList.Rows[index].Cells[2].Value = rowContent["Thickness"]?.ToString();
                            lensList.Rows[index].Cells[3].Value = rowContent["RefractionD"]?.ToString();
                            lensList.Rows[index].Cells[4].Value = rowContent["RefractionC"]?.ToString();
                            lensList.Rows[index].Cells[5].Value = rowContent["RefractionF"]?.ToString();
                        }

                        var otherArg = jArray[1];
                        textBox1.Text = otherArg["EntranceHeight"]?.ToString();
                        textBox2.Text = otherArg["AOV"]?.ToString();
                        textBox3.Text = otherArg["EntranceLocation"]?.ToString();
                        textBox4.Text = otherArg["Aperture"]?.ToString();
                        textBox5.Text = otherArg["Height"]?.ToString();
                    }
                    else
                    {
                        //Read as Excel
                        using (var fs = File.OpenRead(fileName))
                        {
                            var workbook = new XSSFWorkbook(fs);
                            var sheet = workbook.GetSheetAt(0);
                            if (sheet != null)
                            {
                                lensList.Rows.Clear();
                                var rowCount = sheet.LastRowNum;
                                if (rowCount > 0)
                                {
                                    var row = sheet.GetRow(1);
                                    textBox1.Text = row.GetCell(0).StringCellValue;
                                    textBox3.Text = row.GetCell(1).StringCellValue;
                                    textBox2.Text = row.GetCell(2).StringCellValue;
                                    textBox4.Text = row.GetCell(3).StringCellValue;
                                    textBox5.Text = row.GetCell(4).StringCellValue;
                                }

                                for (var i = 3; i <= rowCount; i++)
                                {
                                    var row = sheet.GetRow(i);
                                    var index = lensList.Rows.Add();
                                    for (var j = 0; j < row.Cells.Count; j++)
                                    {
                                        lensList.Rows[index].Cells[j].Value = row.GetCell(j).StringCellValue;
                                    }
                                }
                            }
                        }
                        
                    }

                    ChangeVisibility();
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show(string.Format(Resources.authorityError1, ex.Message) + string.Format(Resources.authorityError2, ex.StackTrace));
                }
            }
            catch
            {
                MessageBox.Show(Resources.loadError);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var otherArgs = new List<KeyValuePair<string, string>>();
            otherArgs.Add(new KeyValuePair<string, string>("EntranceHeight",textBox1.Text));
            otherArgs.Add(new KeyValuePair<string, string>("EntranceLocation",textBox3.Text));
            otherArgs.Add(new KeyValuePair<string, string>("AOV",textBox2.Text));
            otherArgs.Add(new KeyValuePair<string, string>("Aperture",textBox4.Text));
            otherArgs.Add(new KeyValuePair<string, string>("Height",textBox5.Text));
            Utility.SaveData(lensList, otherArgs);
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        

        private void CellEndEdit(object sender, EventArgs e)
        {
            ChangeVisibility();
        }

        private void ChangeVisibility()
        {
            try
            {
                if (lensList.Rows[0].Cells[2].Value.ToString() == "INFINITY")
                {
                    // 无穷远物,不显示物距和半孔径角
                    panel1.BringToFront();
                    panel1.Visible = true;
                    panel2.SendToBack();
                    panel2.Visible = false;
                    Refresh();
                }
                else
                {
                    // 有限物，不显示物方视场角
                    panel1.SendToBack();
                    panel1.Visible = false;
                    panel2.BringToFront();
                    panel2.Visible = true;
                    Refresh();
                }
                CheckLegality();
            }
            catch
            {
                Console.Write(Resources.Error);
            }
        }

        private void CheckLegality()
        {
            lensList.Rows[0].Cells[0].Value = "物面";
            lensList.Rows[1].Cells[0].Value = "光阑";
            lensList.Rows[lensList.RowCount - 1].Cells[0].Value = "像面";
            lensList.Rows[0].Cells[1].Value = "INFINITY";
            lensList.Rows[lensList.RowCount - 1].Cells[1].Value = "INFINITY";
            lensList.Rows[lensList.RowCount - 1].Cells[2].Value = 0;
            lensList.Rows[lensList.RowCount - 1].Cells[3].Value = 1;
            lensList.Rows[lensList.RowCount - 1].Cells[4].Value = 1;
            lensList.Rows[lensList.RowCount - 1].Cells[5].Value = 1;
            
        }

        private void OnInsertKeyUp()
        {
            AddRow();
        }

        private void OnDeleteKeyUp()
        {
            if (lensList.SelectedRows.Count>0)
            {
                DeleteRow(lensList.SelectedRows[0]);
            }
        }

        private void Frm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Insert:
                    OnInsertKeyUp();
                    return;
                case Keys.Delete:
                    OnDeleteKeyUp();
                    return;
                default:
                    return;
            }
        }
    }
}