using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Opt_Summer.Calculate;

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
            //lensList.DataSource = createSource();
            lensList.ColumnCount = 4;
            lensList.ColumnHeadersVisible = true;

            lensList.Columns[0].Name = "表面类型";
            lensList.Columns[1].Name = "曲率半径";
            lensList.Columns[2].Name = "厚度";
            lensList.Columns[3].Name = "折射率";

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
            }

            lensList.Rows[0].Cells[2].Value = "INFINITY";
            
            lensList.AllowUserToAddRows = false;
            lensList.AllowUserToDeleteRows = false;
            lensList.MultiSelect = false;
            lensList.Columns[0].ReadOnly = true;
        }

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
                MessageBox.Show(message,"错误");
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var lenses = new List<Lens>();
            foreach (DataGridViewRow row in lensList.Rows)
            {
                var radius = row.Cells[1].Value.ToString()=="INFINITY"?Utility.Infinity:double.Parse(row.Cells[1].Value.ToString());
                var refraction = row.Cells[3].Value.ToString() == "INFINITY"
                    ? Utility.Infinity
                    : double.Parse(row.Cells[3].Value.ToString());
                var thickness = row.Cells[2].Value.ToString()=="INFINITY"?Utility.Infinity:double.Parse(row.Cells[2].Value.ToString());
                
                
                lenses.Add(new Lens(radius, refraction, thickness));
            }
            
            var W = double.Parse(textBox2.Text);
            var D = double.Parse(textBox1.Text);
            var lp = double.Parse(textBox3.Text);
            
            //计算第一近轴光线
            var l1 = lenses[0].Thickness;
            var u1 = Math.Sin(Math.Atan(D / 2 / l1));
            var light = new Light(1, l1, u1);
            for (var i = 1; i < lenses.Count; i++)
            {
                light = light.Paraxial(lenses[i], D / 2);
            }

            var message = "First Paraxial: Lk = " + light.L + " mm and Uk = " + light.U;
            MessageBox.Show(message);
            
            //计算第二近轴光线
            l1 = lp;
            u1 = Math.Sin(W);
            light = new Light(1, l1, u1);
            for (var i = 1; i < lenses.Count; i++)
            {
                light = light.Paraxial(lenses[i], D / 2);
            }
            message = "Second Paraxial: Lk = " + light.L + " mm and Uk = " + light.U;
            MessageBox.Show(message);
            
            if (Math.Abs(lenses[0].Thickness - Utility.Infinity) < 10E-3)
            {
                // 计算无穷远实际光路
                var KEta = 1; //取点系数
                l1 = Utility.Infinity;
                u1 = 0;
                light = new Light(1, l1, u1);
                for (var i = 1; i < lenses.Count; i++)
                {
                    light = light.Actual(lenses[i], KEta * D / 2);
                }
                message = "Infinity Actual: Lk = " + light.L + " mm and Uk = " + light.U;
                MessageBox.Show(message);
            }
            else
            {
                // 计算轴外光实际光路
                var Kw = 1; //视场取点系数
                var KEta = 1; //孔径取点系数
                u1 = Kw * W;
                l1 = lenses[0].Radius+KEta*D/2/Math.Tan(u1);
                light = new Light(1, l1, u1);
                for (var i = 1; i < lenses.Count; i++)
                {
                    light = light.Actual(lenses[i], KEta * D / 2);
                }
                message = "Off-axis Actual: Lk = " + light.L + " mm and Uk = " + light.U;
                MessageBox.Show(message);
                // 计算物面轴上点实际光路
                KEta = 1; //孔径取点系数
                l1 = lenses[0].Radius;
                u1 = Math.Asin(KEta * D/2/l1);
            }
            
            

        }
    }
}