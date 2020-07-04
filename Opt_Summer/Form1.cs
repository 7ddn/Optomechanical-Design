using System;
using System.Windows.Forms;

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
                    default:
                        break;
                }

                this.lensList.Rows[index].Cells[1].Value = "INFINITY";
                this.lensList.Rows[index].Cells[2].Value = "0";
                this.lensList.Rows[index].Cells[3].Value = "1";
            }
            
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
    }
}