using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Opt_Summer.Calculate;

namespace Opt_Summer
{
    public partial class Form1 : Form
    {
        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
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
                int index = this.lensList.Rows.Add();
                switch (i)
                {
                    case 0:
                        this.lensList.Rows[index].Cells[0].Value = "物面";
                        break;
                    case 1:
                        this.lensList.Rows[index].Cells[0].Value = "光阑";
                        break;
                    case 2:
                        this.lensList.Rows[index].Cells[0].Value = "像面";
                        break;
                }

                this.lensList.Rows[index].Cells[1].Value = "INFINITY";
                this.lensList.Rows[index].Cells[2].Value = "0";
                this.lensList.Rows[index].Cells[3].Value = "1";
                this.lensList.Rows[index].Cells[4].Value = "1";
                this.lensList.Rows[index].Cells[5].Value = "1";
            }

            lensList.Rows[0].Cells[2].Value = "INFINITY";

            lensList.AllowUserToAddRows = false;
            lensList.AllowUserToDeleteRows = false;
            lensList.MultiSelect = false;
            lensList.Columns[0].ReadOnly = true;

            //lensList.CellEndEdit += lensList_CellEndEdit;
        }

        /*private void lensList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var newValue = 
            if ((newValue != "INFINITY" && !Utility.IsNumberic(newValue)))
            {
                var message = "只能输入INFINITY或者数字";
                MessageBox.Show(message, "错误");
            }

        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            lensList.Rows.Insert(lensList.RowCount - 1, 1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var row = lensList.CurrentRow;
            if (row == null) return;
            if (row.Index == 1)
            {
                if (lensList.RowCount == 3)
                {
                    string message = "不能删除最后一个透镜";
                    MessageBox.Show(message, "错误");
                    return;
                }
                else
                {
                    lensList.Rows.Remove(row);
                    lensList.Rows[1].Cells[0].Value = "光阑";
                    return;
                }
            }

            if (row.Index != 0 && row.Index != lensList.RowCount - 1)
            {
                lensList.Rows.Remove(row);
            }
            else
            {
                string message = "不能删除" + (row.Index == 0 ? "物面" : "像面");
                MessageBox.Show(message, "错误");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var lenses = new List<Lens>();

            //获取透镜值
            foreach (DataGridViewRow row in lensList.Rows)
            {
                var radius = row.Cells[1].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[1].Value.ToString());
                var refractiond = row.Cells[3].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[3].Value.ToString());
                var refractionC = row.Cells[4].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[4].Value.ToString());
                var refractionF = row.Cells[5].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[5].Value.ToString());
                var thickness = row.Cells[2].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[2].Value.ToString());


                lenses.Add(new Lens(radius, refractiond, thickness, refractionC, refractionF));
            }

            // 获取外部参数
            var W = double.Parse(textBox2.Text);
            var D = double.Parse(textBox1.Text);
            var lp = double.Parse(textBox3.Text);
            var u = double.Parse(textBox4.Text) * Math.PI / 180;
            //MessageBox.Show(u.ToString());
            var y = double.Parse(textBox5.Text);

            //生成新窗口
            var dataAnalysis = new DataAnalysis();

            //计算焦距
            var l1 = Utility.Infinity;
            double u1 = 0;
            var light = new Light(1, l1, u1);
            double f_ = 1;
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                light = light.Paraxial(lenses[i], D / 2, 'd');
                f_ = f_ * (lenses[i].Thickness + light.L) / light.L;
            }

            f_ *= light.L; //乘上多除的最后一面物距

            // 更新焦距
            dataAnalysis.Data1 = Math.Round(f_, 10).ToString(); //焦距
            dataAnalysis.Data2 = Math.Round(light.L - f_, 10).ToString(); //像方主面位置
            //计算出瞳距
            l1 = lp;
            u1 = Math.Sin(W);
            light = new Light(1, l1, u1);

            // 更新出瞳距
            dataAnalysis.Data3 = RepeatedLightParaxial(light, 'd', lenses, D / 2).L.ToString(); // 出瞳距
            
            // 计算第一近轴光线

            l1 = lenses[0].Thickness;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = 0;
                dataAnalysis.Data4 = Math.Round(f_ * Math.Tan(W * Math.PI / 180), 10).ToString(); // 全视场理想像高
                dataAnalysis.Data5 = Math.Round(f_ * Math.Tan(W * 0.7 * Math.PI / 180), 10).ToString(); // 0.7视场理想像高
            }
            else
            {
                u1 = Math.Sin(u);
            }

            light = new Light(1, l1, u1);


            var lightD = RepeatedLightParaxial(light, 'd', lenses, D/2);
            var ld = lightD.L;
            dataAnalysis.DataD1 = Math.Round(ld, 10).ToString(); //d光理想像距
            var lf = RepeatedLightParaxial(light, 'F', lenses, D / 2).L;
            dataAnalysis.DataF1 = lf.ToString(); //F光理想像距
            var lc = RepeatedLightParaxial(light, 'C', lenses, D / 2).L;
            dataAnalysis.DataC1 = lc.ToString(); //C光理想像距
            if (Math.Abs(l1 - Utility.Infinity) > 1.0E-05)
            {
                double beta = 1;
                for (var i = 1; i < lenses.Count - 1; i++)
                {
                    beta = beta / light.L;
                    light = light.Paraxial(lenses[i], D / 2, 'd');
                    beta = beta * (light.L + lenses[i].Thickness);
                }
                dataAnalysis.Data4 = Math.Abs(y*beta).ToString(); // 全视场理想像高
                dataAnalysis.Data5 = Math.Abs(y*0.7*beta).ToString(); // 0.7视场理想像高
            }
            dataAnalysis.Data10 = Math.Round(double.Parse(dataAnalysis.DataF1) - double.Parse(dataAnalysis.DataC1), 10)
                .ToString(); //零孔径位置色差


            // 计算无穷远实际光路
            double kEta = 1; //取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                l1 = Utility.Infinity;
                u1 = 0;
            }
            else
            {
                l1 = lenses[0].Thickness;
                u1 = Math.Asin(kEta * Math.Sin(u));
            }
            
            light = new Light(1, l1, u1);

            // 更新全孔径实际光线相关信息
            dataAnalysis.DataD2 = RepeatedLightActual(light, 'd', lenses, kEta * D / 2).L.ToString(); // d光全孔径实际像距
            dataAnalysis.DataF2 = RepeatedLightActual(light, 'F', lenses, kEta * D / 2).L.ToString(); // F光全孔径实际像距
            dataAnalysis.DataC2 = RepeatedLightActual(light, 'C', lenses, kEta * D / 2).L.ToString(); // F光全孔径实际像距
            var paraxialL = double.Parse(dataAnalysis.DataD1);
            dataAnalysis.Data6 = Math.Round(double.Parse(dataAnalysis.DataD2) - paraxialL, 10).ToString(); //全孔径球差
            dataAnalysis.Data8 = Math.Round(double.Parse(dataAnalysis.DataF2) - double.Parse(dataAnalysis.DataC2), 10)
                .ToString(); // 全孔径位置色差


            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                l1 = Utility.Infinity;
                u1 = 0;
            }
            else
            {
                l1 = lenses[0].Thickness;
                u1 = Math.Asin(kEta * Math.Sin(u));
            }
            light = new Light(1, l1, u1);

            // 更新0.7孔径实际光线相关信息
            dataAnalysis.DataD3 = RepeatedLightActual(light, 'd', lenses, kEta * D / 2).L.ToString(); // d光0.7孔径实际像距
            dataAnalysis.DataF3 = RepeatedLightActual(light, 'F', lenses, kEta * D / 2).L.ToString(); // d光0.7孔径实际像距
            dataAnalysis.DataC3 = RepeatedLightActual(light, 'C', lenses, kEta * D / 2).L.ToString(); // d光0.7孔径实际像距
            dataAnalysis.Data7 = Math.Round(double.Parse(dataAnalysis.DataD3) - paraxialL, 10).ToString(); //0.7孔径球差
            dataAnalysis.Data9 = Math.Round(double.Parse(dataAnalysis.DataF3) - double.Parse(dataAnalysis.DataC3), 10)
                .ToString(); // 0.7孔径位置色差

            // 计算轴外光全视场实际光路
            double Kw = 1; //视场取点系数
            kEta = 0; //孔径取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            var dLight = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            ld = Math.Abs(Math.Round((dLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(dLight.U), 10));
            dataAnalysis.DataD4 = ld.ToString(); // d光全视场实际像高
            dataAnalysis.Data16 =
                Math.Round(ld - double.Parse(dataAnalysis.Data4), 10).ToString(); // d光全视场绝对畸变
            dataAnalysis.Data14 = Math.Round(double.Parse(dataAnalysis.Data16) / double.Parse(dataAnalysis.Data4), 10)
                .ToString(); //d光全视场相对畸变
            var cLight = RepeatedLightActual(light, 'C', lenses, kEta * D / 2);
            var hc = (cLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(cLight.U);
            dataAnalysis.DataC4 =
                Math.Abs(Math.Round(hc, 10)).ToString(); // C光全视场实际像高
            var fLight = RepeatedLightActual(light, 'F', lenses, kEta * D / 2);
            var hf = (fLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(fLight.U);
            dataAnalysis.DataF4 =
                Math.Abs(Math.Round(hf, 10)).ToString(); // F光全视场实际像高

            dataAnalysis.Data22 = Math.Round(hf - hc, 10).ToString(); // 全视场倍率色差

            // 计算全视场彗差
            // 0.7孔径
            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            var light1U = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            var lu = Math.Abs(Math.Round((light1U.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1U.U), 10));

            kEta = -0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            var light1d = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            var ldd = Math.Abs(Math.Round((light1d.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data19 = Math.Round((lu + ldd) / 2 - ld, 10).ToString();

            // 全孔径
            kEta = 1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1U = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            lu = Math.Abs(Math.Round((light1U.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1U.U), 10));

            kEta = -1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1d = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            ldd = Math.Abs(Math.Round((light1d.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data18 = Math.Round((lu + ldd) / 2 - ld, 10).ToString();


            // 计算轴外光0.7视场实际光路
            Kw = 0.7; //视场取点系数
            kEta = 0; //孔径取点系数
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            dLight = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            ld = Math.Abs(Math.Round((dLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(dLight.U), 10));
            dataAnalysis.DataD5 = ld.ToString(); // d光0.7视场实际像高
            dataAnalysis.Data17 =
                Math.Round(double.Parse(dataAnalysis.DataD5) - double.Parse(dataAnalysis.Data5), 10)
                    .ToString(); // d光0.7视场绝对畸变
            dataAnalysis.Data15 = Math.Round(double.Parse(dataAnalysis.Data17) / double.Parse(dataAnalysis.Data5), 10)
                .ToString(); //d光0.7视场相对畸变
            cLight = RepeatedLightActual(light, 'C', lenses, kEta * D / 2);
            dataAnalysis.DataC5 =
                Math.Abs(Math.Round((cLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(cLight.U), 10))
                    .ToString(); // C光0.7视场实际像高

            fLight = RepeatedLightActual(light, 'F', lenses, kEta * D / 2);
            dataAnalysis.DataF5 =
                Math.Abs(Math.Round((fLight.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(fLight.U), 10))
                    .ToString(); // F光0.7视场实际像高

            dataAnalysis.Data23 =
                Math.Round(double.Parse(dataAnalysis.DataF5) - double.Parse(dataAnalysis.DataC5), 10)
                    .ToString(); // 0.7视场倍率色差

            // 计算0.7视场彗差
            // 0.7孔径
            kEta = 0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1U = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            lu = Math.Abs(Math.Round((light1U.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1U.U), 10));

            kEta = -0.7;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1d = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            ldd = Math.Abs(Math.Round((light1d.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data21 = Math.Round((lu + ldd) / 2 - ld, 10).ToString();

            // 全孔径
            kEta = 1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1U = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            lu = Math.Abs(Math.Round((light1U.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1U.U), 10));

            kEta = -1;
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 1.0E-05)
            {
                u1 = Kw * W * Math.PI / 180;
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            else
            {
                u1 = Math.Atan((Kw * y - kEta * D / 2) / (lp - lenses[0].Thickness));
                l1 = lp + kEta * D / 2 / Math.Tan(u1);
            }
            light = new Light(1, l1, u1);

            light1d = RepeatedLightActual(light, 'd', lenses, kEta * D / 2);
            ldd = Math.Abs(Math.Round((light1d.L - double.Parse(dataAnalysis.DataD1)) * Math.Tan(light1d.U), 10));

            dataAnalysis.Data20 = Math.Round((lu + ldd) / 2 - ld, 10).ToString();


            // 轴外点沿主光线的细光束成像位置的计算
            var i1 = light.GetActualArgs(lenses[1], kEta * D / 2)[0];
            var h1 = lenses[1].Radius * Math.Sin(u1 + i1);
            var PA = light.L * Math.Sin(u1) / Math.Cos((i1 - u1) / 2);
            var x = PA * PA / 2 * lenses[1].Radius;
            double s, t;
            s = t = (light.L - x) / Math.Cos(u1);
            double t_ = 0, s_ = 0, U_ = 0;
            var args = light.GetActualArgs(lenses[1], 0);
            var x_ = x;
            for (int i = 1; i < lenses.Count - 1; i++)
            {
                x = x_;
                t_ = lenses[i].Refractiond * Math.Pow(Math.Cos(args[1]), 2) /
                     ((lenses[i].Refractiond * Math.Cos(args[1]) - light.NowRefraction * Math.Cos(args[0])) /
                         lenses[i].Radius + light.NowRefraction * Math.Pow(Math.Cos(args[0]), 2) / t);
                s_ = lenses[i].Refractiond /
                     ((lenses[i].Refractiond * Math.Cos(args[1]) - light.NowRefraction * Math.Cos(args[0])) /
                         lenses[i].Radius + light.NowRefraction / s);
                U_ = args[3];
                light = light.Actual(lenses[i], 0, 'd');
                args = light.GetActualArgs(lenses[i + 1], 0);
                PA = light.L * Math.Sin(light.U) / Math.Cos((args[0] - light.U) / 2);
                x_ = PA * PA / (2 * lenses[i + 1].Radius);
                var Dv = (lenses[i].Thickness - x + x_) / Math.Cos(U_);
                t = t_ - Dv;
                s = s_ - Dv;
            }

            var xt = t_ * Math.Cos(U_) + x - paraxialL;
            var xs = s_ * Math.Cos(U_) + x - paraxialL;
            var dx = xt - xs;
            dataAnalysis.Data11 = Math.Round(xt, 10).ToString();
            dataAnalysis.Data12 = Math.Round(xs, 10).ToString();
            dataAnalysis.Data13 = Math.Round(dx, 10).ToString();


            // 弹出分析窗口
            dataAnalysis.Show();
        }

        private bool SaveData()
        {
            var jArray = new JArray();
            foreach (DataGridViewRow row in lensList.Rows)
            {
                var jObject = new JObject
                {
                    {"Type", row.Cells[0].Value.ToString()},
                    {"Radius", row.Cells[1].Value.ToString()},
                    {"Thickness", row.Cells[2].Value.ToString()},
                    {"RefractionD", row.Cells[3].Value.ToString()},
                    {"RefractionC", row.Cells[4].Value.ToString()},
                    {"RefractionF", row.Cells[5].Value.ToString()}
                };
                jArray.Add(jObject);
            }

            _saveFileDialog = new SaveFileDialog
                {Filter = "Text|*.txt", Title = "保存配置数据", InitialDirectory = Application.StartupPath};
            var invokeThread = new Thread(InvokeMethod);
            invokeThread.SetApartmentState(ApartmentState.STA);
            invokeThread.Start();
            invokeThread.Join();

            MessageBox.Show("123");
            return true;
            /*var fileName = saveFileDialog.FileName;

            if (saveFileDialog.FileName != "")
            {
                var sw = new StreamWriter(fileName, true, Encoding.UTF8);
                sw.Write(jArray.ToString());
                sw.Close();
                MessageBox.Show("保存完成!");
                return true;
            }
            else
            {
                MessageBox.Show("文件名不能为空!");
                return false;
            }*/
        }

        private void InvokeMethod()
        {
            _saveFileDialog.ShowDialog();
        }

        private bool LoadData()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text|*.txt";
            openFileDialog.ShowDialog();
            return true;
            /* try
            {
                var openFileDialog = new OpenFileDialog()
                {
                    FileName = "选择要打开的文件",
                    Filter = "文本文件 (*.txt)|*.txt",
                    Title = "打开保存的配置信息"
                };
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var fileName = openFileDialog.FileName;
                        var sr = new StreamReader(fileName);
                        var jsonContent = sr.ReadToEnd();
                        var jArray = JArray.Parse(jsonContent);
                        lensList.Rows.Clear();
                        foreach (var rowContent in jArray)
                        {
                            var index = lensList.Rows.Add();
                            lensList.Rows[index].Cells[0].Value = rowContent["Type"]?.ToString();
                            lensList.Rows[index].Cells[1].Value = rowContent["Radius"]?.ToString();
                            lensList.Rows[index].Cells[2].Value = rowContent["Thickness"]?.ToString();
                            lensList.Rows[index].Cells[3].Value = rowContent["Refraction"]?.ToString();
                        }
                    }
                    catch(SecurityException ex)
                    {
                        MessageBox.Show($"权限问题 .\n\n 错误信息: {ex.Message}\n\n" + $"详细:\n\n{ex.StackTrace}");
                    }
                }
                return true;
            }
            catch
            {
                MessageBox.Show("读取失败");
                return false;
            }*/
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private Light RepeatedLightParaxial(Light startLight, char ty, List<Lens> lenses, double a)
        {
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                startLight = startLight.Paraxial(lenses[i], a, ty);
            }

            startLight.L = Math.Round(startLight.L, 6);

            return startLight;
        }

        private Light RepeatedLightActual(Light startLight, char ty, List<Lens> lenses, double a)
        {
            for (var i = 1; i < lenses.Count - 1; i++)
            {
                startLight = startLight.Actual(lenses[i], a, ty);
            }

            startLight.L = Math.Round(startLight.L, 6);

            return startLight;
        }
    }
}