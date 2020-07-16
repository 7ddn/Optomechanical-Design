using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Opt_Summer.Properties;

namespace Opt_Summer
{
    public partial class DataAnalysis : Form
    {
        public DataAnalysis()
        {
            InitializeComponent();
        }

        public string Data1
        {
            get => data1.Text;
            set => data1.Text = value;
        }

        public string Data2
        {
            get => data2.Text;
            set => data2.Text = value;
        }

        public string Data3
        {
            get => data3.Text;
            set => data3.Text = value;
        }

        public string Data4
        {
            get => data4.Text;
            set => data4.Text = value;
        }

        public string Data5
        {
            get => data5.Text;
            set => data5.Text = value;
        }

        public string Data6
        {
            get => data6.Text;
            set => data6.Text = value;
        }

        public string Data7
        {
            get => data7.Text;
            set => data7.Text = value;
        }

        public string Data8
        {
            get => data8.Text;
            set => data8.Text = value;
        }

        public string Data9
        {
            get => data9.Text;
            set => data9.Text = value;
        }

        public string Data10
        {
            get => data10.Text;
            set => data10.Text = value;
        }

        public string Data11
        {
            get => data11.Text;
            set => data11.Text = value;
        }

        public string Data12
        {
            get => data12.Text;
            set => data12.Text = value;
        }

        public string Data13
        {
            get => data13.Text;
            set => data13.Text = value;
        }

        public string Data14
        {
            get => data14.Text;
            set => data14.Text = value;
        }

        public string Data15
        {
            get => data15.Text;
            set => data15.Text = value;
        }

        public string Data16
        {
            get => data16.Text;
            set => data16.Text = value;
        }

        public string Data17
        {
            get => data17.Text;
            set => data17.Text = value;
        }

        public string Data18
        {
            get => data18.Text;
            set => data18.Text = value;
        }

        public string Data19
        {
            get => data19.Text;
            set => data19.Text = value;
        }

        public string Data20
        {
            get => data20.Text;
            set => data20.Text = value;
        }

        public string Data21
        {
            get => data21.Text;
            set => data21.Text = value;
        }

        public string Data22
        {
            get => data22.Text;
            set => data22.Text = value;
        }

        public string Data23
        {
            get => data23.Text;
            set => data23.Text = value;
        }


        public string DataD1
        {
            get => datad1.Text;
            set => datad1.Text = value;
        }

        public string DataD2
        {
            get => datad2.Text;
            set => datad2.Text = value;
        }

        public string DataD3
        {
            get => datad3.Text;
            set => datad3.Text = value;
        }

        public string DataD4
        {
            get => datad4.Text;
            set => datad4.Text = value;
        }

        public string DataD5
        {
            get => datad5.Text;
            set => datad5.Text = value;
        }

        public string DataC1
        {
            get => datac1.Text;
            set => datac1.Text = value;
        }

        public string DataC2
        {
            get => datac2.Text;
            set => datac2.Text = value;
        }

        public string DataC3
        {
            get => datac3.Text;
            set => datac3.Text = value;
        }

        public string DataC4
        {
            get => datac4.Text;
            set => datac4.Text = value;
        }

        public string DataC5
        {
            get => datac5.Text;
            set => datac5.Text = value;
        }

        public string DataF1
        {
            get => dataf1.Text;
            set => dataf1.Text = value;
        }

        public string DataF2
        {
            get => dataf2.Text;
            set => dataf2.Text = value;
        }

        public string DataF3
        {
            get => dataf3.Text;
            set => dataf3.Text = value;
        }

        public string DataF4
        {
            get => dataf4.Text;
            set => dataf4.Text = value;
        }

        public string DataF5
        {
            get => dataf5.Text;
            set => dataf5.Text = value;
        }

        private bool ExportReport()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = Resources.exportReportFormat,
                InitialDirectory = Application.StartupPath
            };
            saveFileDialog.ShowDialog();

            var fileName = saveFileDialog.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show(Resources.BlankFileNameError);
                return false;
            }

            try
            {

                //save as excel
                IWorkbook workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Sheet0");
                // 表头
                Utility.CreateSingleReportRow(sheet, 0, "外部参数", "波长", "视场",
                    "孔径", "值");

                // 焦距
                Utility.CreateSingleReportRow(sheet, 1, "焦距f'", "d", "",
                    "", Data1);

                // 理想像距
                Utility.CreateSingleReportRow(sheet, 2, "理想像距l'(以透镜最后一面为参考)", "d", "0",
                    "0", DataD1);
                Utility.CreateSingleReportRow(sheet, 3, "", "C", "0",
                    "0", DataC1);
                Utility.CreateSingleReportRow(sheet, 4, "", "F", "0",
                    "0", DataF1);

                // 实际像位置
                Utility.CreateSingleReportRow(sheet, 5, "实际像位置(以透镜最后一面为参考)", "d", "0"
                    , "1", DataD2);
                Utility.CreateSingleReportRow(sheet, 6, "", "", "0",
                    "0.7", DataD3);
                Utility.CreateSingleReportRow(sheet, 7, "", "C", "0",
                    "1", DataC2);
                Utility.CreateSingleReportRow(sheet, 8, "", "", "0",
                    "0.7", DataC3);
                Utility.CreateSingleReportRow(sheet, 9, "", "F", "0",
                    "1", DataF2);
                Utility.CreateSingleReportRow(sheet, 10, "", "", "0",
                    "0.7", DataF3);

                // 像方主面位置l_H'
                Utility.CreateSingleReportRow(sheet, 11, "像方主面位置lH'(以透镜最后一面为参考)", "d", "",
                    "", Data2);

                // 出瞳距
                Utility.CreateSingleReportRow(sheet, 12, "出瞳距lp'(以透镜最后一面为参考)", "", "",
                    "", Data3);

                // 理想像高
                Utility.CreateSingleReportRow(sheet, 13, "理想像高y0'", "d", "1",
                    "0", Data4);
                Utility.CreateSingleReportRow(sheet, 14, "", "d", "0.7",
                    "0", Data5);

                // 球差
                Utility.CreateSingleReportRow(sheet, 15, "球差", "d", "0",
                    "0.7", Data7);
                Utility.CreateSingleReportRow(sheet, 16, "", "", "0",
                    "1", Data6);

                // 位置色差
                Utility.CreateSingleReportRow(sheet, 17, "位置色差", "F-C", "0",
                    "0.7", Data9);
                Utility.CreateSingleReportRow(sheet, 18, "", "", "0",
                    "1", Data8);
                Utility.CreateSingleReportRow(sheet, 19, "", "", "0",
                    "0", Data10);

                // 场曲像散
                Utility.CreateSingleReportRow(sheet, 20, "子午场曲", "d", "1",
                    "0", Data11);
                Utility.CreateSingleReportRow(sheet, 21, "弧矢场曲", "d", "1",
                    "0", Data12);
                Utility.CreateSingleReportRow(sheet, 22, "像散", "d", "1",
                    "0", Data13);

                // 实际像高
                Utility.CreateSingleReportRow(sheet, 23, "实际像高", "F", "0.7",
                    "0", DataF5);
                Utility.CreateSingleReportRow(sheet, 24, "", "", "1",
                    "0", DataF4);
                Utility.CreateSingleReportRow(sheet, 25, "", "d", "0.7",
                    "0", DataD5);
                Utility.CreateSingleReportRow(sheet, 26, "", "", "1",
                    "0", DataD4);
                Utility.CreateSingleReportRow(sheet, 27, "", "C", "0.7",
                    "0", DataC5);
                Utility.CreateSingleReportRow(sheet, 28, "", "", "1",
                    "0", DataC4);

                // 畸变
                Utility.CreateSingleReportRow(sheet, 29, "相对畸变", "d", "0.7",
                    "", Data15);
                Utility.CreateSingleReportRow(sheet, 30, "", "", "1",
                    "", Data14);
                Utility.CreateSingleReportRow(sheet, 31, "绝对畸变", "d", "0.7",
                    "", Data17);
                Utility.CreateSingleReportRow(sheet, 32, "", "", "1",
                    "", Data16);

                // 倍率色差
                Utility.CreateSingleReportRow(sheet, 33, "倍率色差", "F-C", "0.7",
                    "0", Data23);
                Utility.CreateSingleReportRow(sheet, 34, "", "", "1",
                    "0", Data22);

                // 子午慧差
                Utility.CreateSingleReportRow(sheet, 35, "子午彗差", "d", "0.7",
                    "0.7", Data21);
                Utility.CreateSingleReportRow(sheet, 36, "", "", "0.7",
                    "1", Data20);
                Utility.CreateSingleReportRow(sheet, 37, "", "", "1",
                    "0.7", Data19);
                Utility.CreateSingleReportRow(sheet, 38, "", "", "1",
                    "1", Data18);
                
                // 合并单元格
                sheet.AddMergedRegion(new CellRangeAddress(2, 4, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(5, 10, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(5, 6, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(7, 8, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(9, 10, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(13, 14, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(13, 14, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(15, 16, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(15, 16, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(17, 19, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(17, 19, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(23, 28, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(23, 24, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(25, 26, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(27,28, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(29, 30, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(29, 30, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(31, 32, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(31, 32, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(33, 34, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(33, 34, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(35, 38, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(35, 38, 1, 1));
                


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
        private void buttonExport_Click(object sender, EventArgs e)
        {
            ExportReport();
        }
    }
    
}