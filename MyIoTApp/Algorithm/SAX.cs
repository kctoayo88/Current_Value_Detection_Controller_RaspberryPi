using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;

namespace SAX
{
    //double[] datalist = new double[Getrow(filePath)]; 讀入資料的矩陣
    //double[] z = new double[Getrow(filePath)]; Z值的矩陣
    public partial class Form1 : Form
    {
        string filePath = @"C:\Users\Ying\Desktop\data.txt";//檔案路徑
        int datanum = 0;//txt讀入data的筆數
        double spreadteam = 0;//幾筆資料一組
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            spreadteam = Convert.ToInt64(textBox5.Text);//輸入(幾筆資料一組)
            openTxt(filePath);
            datanum = 0;//重計data筆數
        }

        public int Getrow(string filePath)
        {
            using (StreamReader filein = new StreamReader(filePath))
            {
                return filein.ReadToEnd().Split('\n').Length;
            }
        }

        public void openTxt(string filePath)
        {
            double[] datalist = new double[Getrow(filePath)];//讀入資料的矩陣
            double[] saxf = new double[Getrow(filePath)];//第一次SAX的矩陣(未切割)
            int[] arrstr;
            double[] orstr;//原始資料每段起點
            List<List<double>> list = new List<List<double>>();//FFT要用的(原始DATA切割)
            List<List<double>> newsax = new List<List<double>>();
            List<List<double>> saxout = new List<List<double>>();//第二次SAX結果
            List<double> newpresax = new List<double>();
            List<List<double>> find = new List<List<double>>();
            List<List<double>> find2 = new List<List<double>>();
            List<List<double>> distance = new List<List<double>>();
            List<List<double>> period = new List<List<double>>();
            List<List<double>> saxlist = new List<List<double>>();//要做第二次SAX
            double[] presax = new double[Convert.ToInt16(datalist.Length / spreadteam)];
            int spread = 0;
            int spreadnum = 0;
            double sum = 0;
            double presigma = 0;
            double[] z = new double[Getrow(filePath)];//Z矩陣
            double sigma = 0;
            double[] sax = new double[Getrow(filePath)];//分組平均值矩陣
            int xxx = 0;
            int[] county;//切割後矩陣實際資料量
            int[] countyor;//原始陣列切割
            double casenum = Convert.ToInt64(textBox6.Text);
            int finnum = 0;
            int k = 0;//第一次切x計量變數
            double limit1=2;
            double xx;
            double saxsum2 = 0;
            double saxmean2 = 0;

            int[] karray;
            void beging()
            {
                //讀入資料
                sax = new double[Convert.ToInt16(datalist.Length/spreadteam)];
                saxf = new double[Convert.ToInt16(datalist.Length / spreadteam)];
                datanum = 0;
                spread = 0;
                
                StreamReader sr = new StreamReader(filePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    datalist[datanum] = Convert.ToDouble(line);
                    //textBox1.AppendText(Convert.ToString(datalist[datanum]) + Environment.NewLine);
                    datanum++;
                }
                //textBox1.AppendText(Convert.ToString(datanum) + Environment.NewLine);


                //初始資料平均值(算Z用)
                for (int i = 0; i < datanum; i++)
                {
                    sum = sum + datalist[i];
                }
                
                double mean = 0;
                mean = sum / datanum;

                //初始資料標準差(算Z用)
                double[] xu = new double[Getrow(filePath)];
                double xusum = 0;
                
                
                for (int i = 0; i < datanum; i++)
                {
                    xu[i] = Math.Pow(datalist[i] - mean, 2);
                    xusum = xusum + xu[i];
                    presigma = xusum / datanum;
                    sigma = Math.Pow(presigma, 0.5);
                }
                //Z值
                int xs = 0;
                double zsum = 0;
                for (int i = 0; i < datanum; i++)
                {
                    z[i] = (datalist[i] - mean) / sigma;
                    zsum = zsum + z[i];
                    xs++;
                }
                //分組
                spreadnum = 0;
                for (int i = 0; i < Math.Floor(datanum / spreadteam); i++)
                {
                    for (int j = 0; j < spreadteam; j++)
                    {
                        presax[i] = presax[i] + z[spread];
                        spread++;
                    }
                    spreadnum++;
                }
                //各組平均值
                for (int i = 0; i < datanum / spreadteam; i++)
                {
                    sax[i] = presax[i] / spreadteam;
                }
                for (int i = 0; i < datanum / spreadteam; i++)//第一次SAX的結果
                {
                    switch (casenum)
                    {
                        case 2:
                            if (sax[i] <= 0)
                            {
                                saxf[i] = 1;
                            }
                            else
                            {
                                saxf[i] = 2;
                            }
                            break;
                        case 3:
                            if (sax[i] <= -0.43)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -0.43 & sax[i] <= 0.43)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= 0.43)
                            {
                                saxf[i] = 3;
                            }
                            break;
                        case 4:
                            if (sax[i] <= -0.67)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -0.67 & sax[i] <= 0)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= 0 & sax[i] <= 0.67)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= 0.67)
                            {
                                saxf[i] = 4;
                            }
                            break;
                        case 5:
                            if (sax[i] <= -0.84)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -0.84 & sax[i] <= -0.25)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.25 & sax[i] <= 0.25)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= 0.25 & sax[i] <= 0.84)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= 0.84)
                            {
                                saxf[i] = 5;
                            }
                            break;
                        case 6:
                            if (sax[i] <= -0.97)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -0.97 & sax[i] <= -0.43)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.43 & sax[i] <= 0)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.43)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= 0.43 & sax[i] <= 0.97)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= 0.97)
                            {
                                saxf[i] = 6;
                            }
                            break;
                        case 7:
                            if (sax[i] <= -1.07)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.07 & sax[i] <= -0.57)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.57 & sax[i] <= -0.18)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.18 & sax[i] <= 0.18)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= 0.18 & sax[i] <= 0.57)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= 0.57 & sax[i] <= 1.07)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 1.07)
                            {
                                saxf[i] = 7;
                            }
                            break;
                        case 8:
                            if (sax[i] <= -1.15)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.15 & sax[i] <= -0.67)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.67 & sax[i] <= -0.32)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.32 & sax[i] <= 0)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.32)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= 0.32 & sax[i] <= 0.67)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 0.67 & sax[i] <= 1.15)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 1.15)
                            {
                                saxf[i] = 8;
                            }
                            break;
                        case 9:
                            if (sax[i] <= -1.22)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.22 & sax[i] <= -0.76)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.76 & sax[i] <= -0.43)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.43 & sax[i] <= -0.14)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.14 & sax[i] <= 0.14)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= 0.14 & sax[i] <= 0.43)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 0.43 & sax[i] <= 0.76)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0.76 & sax[i] <= 1.22)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 1.22)
                            {
                                saxf[i] = 9;
                            }
                            break;
                        case 10:
                            if (sax[i] <= -1.28)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.28 & sax[i] <= -0.84)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.84 & sax[i] <= -0.52)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.52 & sax[i] <= -0.25)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.25 & sax[i] <= 0)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.25)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 0.25 & sax[i] <= 0.52)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0.52 & sax[i] <= 0.84)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.84 & sax[i] <= 1.28)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 1.28)
                            {
                                saxf[i] = 10;
                            }
                            break;
                        case 11:
                            if (sax[i] <= -1.34)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.34 & sax[i] <= -0.91)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.91 & sax[i] <= -0.6)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.6 & sax[i] <= -0.35)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.35 & sax[i] <= -0.11)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.11 & sax[i] <= 0.11)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 0.11 & sax[i] <= 0.35)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0.35 & sax[i] <= 0.6)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.6 & sax[i] <= 0.91)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.91 & sax[i] <= 1.34)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 1.34)
                            {
                                saxf[i] = 11;
                            }
                            break;
                        case 12:
                            if (sax[i] <= -1.38)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.38 & sax[i] <= -0.97)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -0.97 & sax[i] <= -0.67)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.67 & sax[i] <= -0.43)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.43 & sax[i] <= -0.21)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.21 & sax[i] <= 0)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.21)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0.21 & sax[i] <= 0.43)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.43 & sax[i] <= 0.67)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.67 & sax[i] <= 0.97)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.97 & sax[i] <= 1.38)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 1.38)
                            {
                                saxf[i] = 12;
                            }
                            break;
                        case 13:
                            if (sax[i] <= -1.43)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.43 & sax[i] <= -1.02)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.02 & sax[i] <= -0.74)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.74 & sax[i] <= -0.5)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.5 & sax[i] <= -0.29)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.29 & sax[i] <= -0.1)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.1 & sax[i] <= 0.1)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0.1 & sax[i] <= 0.29)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.29 & sax[i] <= 0.5)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.5 & sax[i] <= 0.74)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.74 & sax[i] <= 1.02)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 1.02 & sax[i] <= 1.43)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 1.43)
                            {
                                saxf[i] = 13;
                            }
                            break;
                        case 14:
                            if (sax[i] <= -1.47)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.47 & sax[i] <= -1.07)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.07 & sax[i] <= -0.79)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.79 & sax[i] <= -0.57)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.57 & sax[i] <= -0.37)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.37 & sax[i] <= -0.18)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.18 & sax[i] <= 0)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.18)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.18 & sax[i] <= 0.37)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.37 & sax[i] <= 0.57)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.57 & sax[i] <= 0.79)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.79 & sax[i] <= 1.07)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 1.07 & sax[i] <= 1.47)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 1.47)
                            {
                                saxf[i] = 14;
                            }
                            break;
                        case 15:
                            if (sax[i] <= -1.5)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.5 & sax[i] <= -1.11)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.11 & sax[i] <= -0.84)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.84 & sax[i] <= -0.62)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.62 & sax[i] <= -0.43)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.43 & sax[i] <= -0.25)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.25 & sax[i] <= -0.08)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.08 & sax[i] <= 0.08)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0.08 & sax[i] <= 0.25)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.25 & sax[i] <= 0.43)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.43 & sax[i] <= 0.62)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.62 & sax[i] <= 0.84)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.84 & sax[i] <= 1.11)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 1.11 & sax[i] <= 1.5)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 1.5)
                            {
                                saxf[i] = 15;
                            }
                            break;
                        case 16:
                            if (sax[i] <= -1.53)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.53 & sax[i] <= -1.15)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.15 & sax[i] <= -0.89)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.89 & sax[i] <= -0.67)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.67 & sax[i] <= -0.49)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.49 & sax[i] <= -0.32)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.32 & sax[i] <= -0.16)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.16 & sax[i] <= 0)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.16)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.16 & sax[i] <= 0.32)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.32 & sax[i] <= 0.49)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.49 & sax[i] <= 0.67)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.67 & sax[i] <= 0.89)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 0.89 & sax[i] <= 1.15)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 1.15 & sax[i] <= 1.53)
                            {
                                saxf[i] = 15;
                            }
                            if (sax[i] >= 1.53)
                            {
                                saxf[i] = 16;
                            }
                            break;
                        case 17:
                            if (sax[i] <= -1.56)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.56 & sax[i] <= -1.19)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.19 & sax[i] <= -0.93)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.93 & sax[i] <= -0.72)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.72 & sax[i] <= -0.54)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.54 & sax[i] <= -0.38)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.38 & sax[i] <= -0.22)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.22 & sax[i] <= -0.07)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= -0.07 & sax[i] <= 0.07)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0.07 & sax[i] <= 0.22)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.22 & sax[i] <= 0.38)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.38 & sax[i] <= 0.54)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.54 & sax[i] <= 0.72)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 0.72 & sax[i] <= 0.93)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 0.93 & sax[i] <= 1.19)
                            {
                                saxf[i] = 15;
                            }
                            if (sax[i] >= 1.19 & sax[i] <= 1.56)
                            {
                                saxf[i] = 16;
                            }
                            if (sax[i] >= 1.56)
                            {
                                saxf[i] = 17;
                            }
                            break;
                        case 18:
                            if (sax[i] <= -1.59)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.59 & sax[i] <= -1.22)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.22 & sax[i] <= -0.97)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -0.97 & sax[i] <= -0.76)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.76 & sax[i] <= -0.59)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.59 & sax[i] <= -0.43)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.43 & sax[i] <= -0.28)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.28 & sax[i] <= -0.14)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= -0.14 & sax[i] <= 0)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.14)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.14 & sax[i] <= 0.28)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.28 & sax[i] <= 0.43)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.43 & sax[i] <= 0.59)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 0.59 & sax[i] <= 0.76)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 0.76 & sax[i] <= 0.97)
                            {
                                saxf[i] = 15;
                            }
                            if (sax[i] >= 0.97 & sax[i] <= 1.22)
                            {
                                saxf[i] = 16;
                            }
                            if (sax[i] >= 1.22 & sax[i] <= 1.59)
                            {
                                saxf[i] = 17;
                            }
                            if (sax[i] >= 1.59)
                            {
                                saxf[i] = 18;
                            }
                            break;
                        case 19:
                            if (sax[i] <= -1.62)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.62 & sax[i] <= -1.25)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.25 & sax[i] <= -1)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -1 & sax[i] <= -0.8)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.8 & sax[i] <= -0.63)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.63 & sax[i] <= -0.48)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.48 & sax[i] <= -0.34)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.34 & sax[i] <= -0.2)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= -0.2 & sax[i] <= -0.07)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= -0.07 & sax[i] <= 0.07)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0.07 & sax[i] <= 0.2)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.2 & sax[i] <= 0.34)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.34 & sax[i] <= 0.48)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 0.48 & sax[i] <= 0.63)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 0.63 & sax[i] <= 0.8)
                            {
                                saxf[i] = 15;
                            }
                            if (sax[i] >= 0.8 & sax[i] <= 1)
                            {
                                saxf[i] = 16;
                            }
                            if (sax[i] >= 1 & sax[i] <= 1.25)
                            {
                                saxf[i] = 17;
                            }
                            if (sax[i] >= 1.25 & sax[i] <= 1.62)
                            {
                                saxf[i] = 18;
                            }
                            if (sax[i] >= 1.62)
                            {
                                saxf[i] = 19;
                            }
                            break;
                        case 20:
                            if (sax[i] <= -1.64)
                            {
                                saxf[i] = 1;
                            }
                            else if (sax[i] >= -1.64 & sax[i] <= -1.28)
                            {
                                saxf[i] = 2;
                            }
                            else if (sax[i] >= -1.28 & sax[i] <= -1.04)
                            {
                                saxf[i] = 3;
                            }
                            if (sax[i] >= -1.04 & sax[i] <= -0.84)
                            {
                                saxf[i] = 4;
                            }
                            if (sax[i] >= -0.84 & sax[i] <= -0.67)
                            {
                                saxf[i] = 5;
                            }
                            if (sax[i] >= -0.67 & sax[i] <= -0.52)
                            {
                                saxf[i] = 6;
                            }
                            if (sax[i] >= -0.52 & sax[i] <= -0.39)
                            {
                                saxf[i] = 7;
                            }
                            if (sax[i] >= -0.39 & sax[i] <= -0.25)
                            {
                                saxf[i] = 8;
                            }
                            if (sax[i] >= -0.25 & sax[i] <= -0.13)
                            {
                                saxf[i] = 9;
                            }
                            if (sax[i] >= -0.13 & sax[i] <= 0)
                            {
                                saxf[i] = 10;
                            }
                            if (sax[i] >= 0 & sax[i] <= 0.13)
                            {
                                saxf[i] = 11;
                            }
                            if (sax[i] >= 0.13 & sax[i] <= 0.25)
                            {
                                saxf[i] = 12;
                            }
                            if (sax[i] >= 0.25 & sax[i] <= 0.39)
                            {
                                saxf[i] = 13;
                            }
                            if (sax[i] >= 0.39 & sax[i] <= 0.52)
                            {
                                saxf[i] = 14;
                            }
                            if (sax[i] >= 0.52 & sax[i] <= 0.67)
                            {
                                saxf[i] = 15;
                            }
                            if (sax[i] >= 0.67 & sax[i] <= 0.84)
                            {
                                saxf[i] = 16;
                            }
                            if (sax[i] >= 0.84 & sax[i] <= 1.04)
                            {
                                saxf[i] = 17;
                            }
                            if (sax[i] >= 1.04 & sax[i] <= 1.28)
                            {
                                saxf[i] = 18;
                            }
                            if (sax[i] >= 1.28 & sax[i] <= 1.64)
                            {
                                saxf[i] = 19;
                            }
                            if (sax[i] >= 1.64)
                            {
                                saxf[i] = 20;
                            }
                            break;
                    }
                    finnum++;
                }
                for(int i=0;i<saxf.Length;i++)
                {
                    textBoxnew1.AppendText(Convert.ToString(saxf[i]) + Environment.NewLine);
                }
            }

            void b2()//第一次SAX切割區段
            {
                
                xx = datanum / spreadteam;//組數
                double mmax = saxf.Max();
                double mmin = saxf.Min();
                double msum = saxf.Sum();
                double mav = msum / Math.Ceiling(datanum / spreadteam);

                //切割區段
                /*switch(Convert.ToInt16(textBox5.Text))//切割界線
                {
                    case 12:
                        limit1 = mav - 7;
                        break;
                    case 13:
                        limit1 = mav - 7;
                        break;
                    case 14:
                        limit1 = mav - 7;
                        break;
                    case 15:
                        limit1 = mav - 8;
                        break;
                    case 16:
                        limit1 = mav - 8;
                        break;
                    case 17:
                        limit1 = mav - 8;
                        break;
                    case 18:
                        limit1 = mav - 9;
                        break;
                    case 19:
                        limit1 = 5;
                        break;
                    case 20:
                        limit1 = 5;
                        break;
                }*/
                saxlist.Add(new List<double>());
                for (int i = 0; i < datanum / spreadteam; i++)
                {
                    saxlist[k].Add(saxf[i]);
                    if (saxf[i] < limit1)
                    {
                        saxlist.Add(new List<double>());
                        k++;
                        saxlist[k].Add(saxf[i]);
                        do
                        {
                            i++;
                        }
                        while (saxf[i] < limit1);
                            i = i - 1;
                    }
                }
            }

            void respread()//反推回原始data
            {
                arrstr = new int[k + 1];//每段起始點
                int constrnum = 1;
                arrstr[0] = 0;
                for (int i = 0; i < Convert.ToInt64(xx); i++)
                {
                    if (saxf[i] < limit1)
                    {
                        do
                        {
                            i++;
                        }
                        while (saxf[i] < limit1);
                        
                        arrstr[constrnum] = i;
                        constrnum++;
                    }
                }
                orstr = new double[arrstr.Length];
                orstr[0] = 0;
                for(int i=0;i<orstr.Length;i++)
                {
                    orstr[i] = arrstr[i] * spreadteam;
                }
                karray = new int[k];
                for (int i = 0; i < k; i++)
                {
                    int kint = 1;
                    karray[i] = kint;
                    kint++;
                }
                //算出組(切割後)資料量

                county = new int[k+1];//每段的實際資料量
                county[0] = 0;
                for (int i = 0; i < k+1; i++)
                {
                    for (int r = 0; r < saxlist[i].Count; r++)
                    {
                        if (saxlist[i][r] != 0)
                        {
                            county[i]++;
                        }

                    }
                }
                countyor = new int[k + 1];
                for(int i=0;i<countyor.Length;i++)
                {
                    countyor[i] = saxlist[i].Count * 4;
                }
            }

            void listspread()
            {
                for (int i = 0; i <= k; i++)
                {
                    list.Add(new List<double>());
                    for (int r =Convert.ToInt16(orstr[i]); r < (orstr[i] + countyor[i]); r++)
                    {
                        if(r<datalist.Length)
                        {
                            list[i].Add(datalist[r]);
                        }
                    }
                }

                //顯示第一切割結果的四段在textbox中
                /*for (int i = 0; i < list[0].Count; i++)
                {
                    textBoxC1.AppendText(Convert.ToString(list[0][i]) + Environment.NewLine);
                }
                for (int i = 0; i < list[1].Count; i++)
                {
                    textBoxC2.AppendText(Convert.ToString(list[1][i]) + Environment.NewLine);
                }
                for (int i = 0; i < list[2].Count; i++)
                {
                    textBoxC3.AppendText(Convert.ToString(list[2][i]) + Environment.NewLine);
                }
                for (int i = 0; i < list[3].Count; i++)
                {
                    textBoxC4.AppendText(Convert.ToString(list[3][i]) + Environment.NewLine);
                }*/

                Debug.WriteLine("第一次SAX各組結果");
                for (int i = 0; i < saxlist.Count; i++)
                {
                    Debug.WriteLine("第{0}段", i + 1);

                    for (int x = 0; x < saxlist[i].Count; x++)
                    {
                        Debug.Write(saxlist[i][x] + " ");
                    }

                    Debug.WriteLine("");
                }
                Debug.WriteLine("");
            }

            void sax2()
            {
                //都是運算
                double newpresigma = 0;
                double newsigma = 0;
                int newspead = 0;
                for(int e=0;e<saxlist.Count;e++)
                {
                    for (int i = 0; i < saxlist[e].Count; i++)
                    {
                        saxsum2 = saxsum2 + saxlist[e][i];
                    }
                    saxmean2 = saxsum2 / saxlist[e].Count;
                    double tttsigsum = 0;
                    List<double> sax2sig = new List<double>();
                    for (int i = 0; i < saxlist[e].Count; i++)
                    {
                        sax2sig.Add(Math.Pow(saxlist[e][i] - saxmean2, 2));
                        tttsigsum = tttsigsum + sax2sig[i];
                        newpresigma = tttsigsum / saxlist[e].Count;
                        newsigma = Math.Pow(newpresigma, 0.5);
                    }
                    List<double> newz = new List<double>();
                    for (int i = 0; i < saxlist[e].Count; i++)
                    {
                        newz.Add((saxlist[e][i] - saxmean2) / newsigma);
                    }
                    //find peak
                    find.Add(new List<double>());
                    distance.Add(new List<double>());
                    for (int i = 1; i < saxlist[e].Count-1; i++)
                    {
                        if (saxlist[e][i] > saxlist[e][i-1])
                        {
                            do
                            {
                                if(i<saxlist[e].Count)
                                {
                                    i++;
                                }
                            }
                            while (saxlist[e][i] >= saxlist[e][i-1]);
                            find[e].Add(saxlist[e][i-1]);
                            distance[e].Add(i-1);
                        }
                    }
                    period.Add(new List<double>());
                    for(int i=1;i<distance[e].Count;i++)
                    {
                    period[e].Add(distance[e][i] - distance[e][i-1]);
                    }
                    //找眾數(LinQ)

                    /*
                    var groups = period[e].GroupBy(v => v);
                    int maxCount = groups.Max(g => g.Count());
                    int mode = Convert.ToInt16(groups.First(g => g.Count() == maxCount).Key);
                    var q =
                    from p in period[e]
                    group p by p.ToString() into g
                    select new
                    {
                        g.Key,
                        NumProducts = g.Count()
                    };
                    foreach (var x in q)
                    {
                        Console.WriteLine(x);//陣列中 每個數字出現的數量
                    }
                    Console.WriteLine(mode);
                    Console.WriteLine("---------------------------------");
                    Console.ReadLine();
                    */
                    double mode = period[e].GroupBy(v => v)
                                .OrderByDescending(g => g.Count())
                                .First()
                                .Key;

                    //開始第二次SAX
                    double spreadteam2 = spreadteam*Convert.ToInt16(mode);
                    double[] use1 = new double[Convert.ToInt16(newz.Count / spreadteam2)];
                    newspead = 0;
                    for (int i = 0; i < Convert.ToInt16(newz.Count/spreadteam2); i++)
                    {
                        for (int j = 0; j < Convert.ToInt16(spreadteam2); j++)
                        {
                            use1[i] = use1[i] + newz[newspead];
                            if(newspead<newz.Count-1)
                            {
                                newspead++;
                            }
                        }
                        newpresax.Add(use1[i]);
                    }
                    //各組平均值2
                    newsax.Add(new List<double>());
                    xxx = 0;
                    for (int i = 0; i < newpresax.Count; i++)
                    {
                        xxx++;
                        newsax[e].Add(newpresax[i] / spreadteam2);
                    }
                    newpresax = new List<double>();
                }
            }

            void team2(List<List<double>> inlist)//第二次SAX給代號
            {
                for(int e=0;e<list.Count;e++)
                {
                    saxout.Add(new List<double>());
                    for (int i = 0; i <inlist[e].Count; i++)
                    {
                        switch (casenum)
                        {
                            case 2:
                                if (inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(1);
                                }
                                else
                                {
                                    saxout[e].Add(2);
                                }
                                break;
                            case 3:
                                if (inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -0.43 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= 0.43)
                                {
                                    saxout[e].Add(3);
                                }
                                break;
                            case 4:
                                if (inlist[e][i] <= -0.67)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -0.67 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= 0 & inlist[e][i] <= 0.67)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= 0.67)
                                {
                                    saxout[e].Add(4);
                                }
                                break;
                            case 5:
                                if (inlist[e][i] <= -0.84)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -0.84 & inlist[e][i] <= -0.25)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.25 & inlist[e][i] <= 0.25)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= 0.25 & inlist[e][i] <= 0.84)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= 0.84)
                                {
                                    saxout[e].Add(5);
                                }
                                break;
                            case 6:
                                if (inlist[e][i] <= -0.97)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -0.97 & inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.43 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= 0.43 & inlist[e][i] <= 0.97)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= 0.97)
                                {
                                    saxout[e].Add(6);
                                }
                                break;
                            case 7:
                                if (inlist[e][i] <= -1.07)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.07 & inlist[e][i] <= -0.57)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.57 & inlist[e][i] <= -0.18)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.18 & inlist[e][i] <= 0.18)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= 0.18 & inlist[e][i] <= 0.57)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= 0.57 & inlist[e][i] <= 1.07)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 1.07)
                                {
                                    saxout[e].Add(7);
                                }
                                break;
                            case 8:
                                if (inlist[e][i] <= -1.15)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.15 & inlist[e][i] <= -0.67)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.67 & inlist[e][i] <= -0.32)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.32 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.32)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= 0.32 & inlist[e][i] <= 0.67)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 0.67 & inlist[e][i] <= 1.15)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 1.15)
                                {
                                    saxout[e].Add(8);
                                }
                                break;
                            case 9:
                                if (inlist[e][i] <= -1.22)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.22 & inlist[e][i] <= -0.76)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.76 & inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.43 & inlist[e][i] <= -0.14)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.14 & inlist[e][i] <= 0.14)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= 0.14 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 0.43 & inlist[e][i] <= 0.76)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0.76 & inlist[e][i] <= 1.22)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 1.22)
                                {
                                    saxout[e].Add(9);
                                }
                                break;
                            case 10:
                                if (inlist[e][i] <= -1.28)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.28 & inlist[e][i] <= -0.84)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.84 & inlist[e][i] <= -0.52)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.52 & inlist[e][i] <= -0.25)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.25 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.25)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 0.25 & inlist[e][i] <= 0.52)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0.52 & inlist[e][i] <= 0.84)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.84 & inlist[e][i] <= 1.28)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 1.28)
                                {
                                    saxout[e].Add(10);
                                }
                                break;
                            case 11:
                                if (inlist[e][i] <= -1.34)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.34 & inlist[e][i] <= -0.91)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.91 & inlist[e][i] <= -0.6)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.6 & inlist[e][i] <= -0.35)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.35 & inlist[e][i] <= -0.11)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.11 & inlist[e][i] <= 0.11)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 0.11 & inlist[e][i] <= 0.35)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0.35 & inlist[e][i] <= 0.6)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.6 & inlist[e][i] <= 0.91)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.91 & inlist[e][i] <= 1.34)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 1.34)
                                {
                                    saxout[e].Add(11);
                                }
                                break;
                            case 12:
                                if (inlist[e][i] <= -1.38)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.38 & inlist[e][i] <= -0.97)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -0.97 & inlist[e][i] <= -0.67)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.67 & inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.43 & inlist[e][i] <= -0.21)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.21 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.21)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0.21 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.43 & inlist[e][i] <= 0.67)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.67 & inlist[e][i] <= 0.97)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.97 & inlist[e][i] <= 1.38)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 1.38)
                                {
                                    saxout[e].Add(12);
                                }
                                break;
                            case 13:
                                if (inlist[e][i] <= -1.43)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.43 & inlist[e][i] <= -1.02)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.02 & inlist[e][i] <= -0.74)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.74 & inlist[e][i] <= -0.5)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.5 & inlist[e][i] <= -0.29)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.29 & inlist[e][i] <= -0.1)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.1 & inlist[e][i] <= 0.1)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0.1 & inlist[e][i] <= 0.29)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.29 & inlist[e][i] <= 0.5)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.5 & inlist[e][i] <= 0.74)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.74 & inlist[e][i] <= 1.02)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 1.02 & inlist[e][i] <= 1.43)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 1.43)
                                {
                                    saxout[e].Add(13);
                                }
                                break;
                            case 14:
                                if (inlist[e][i] <= -1.47)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.47 & inlist[e][i] <= -1.07)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.07 & inlist[e][i] <= -0.79)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.79 & inlist[e][i] <= -0.57)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.57 & inlist[e][i] <= -0.37)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.37 & inlist[e][i] <= -0.18)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.18 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.18)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.18 & inlist[e][i] <= 0.37)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.37 & inlist[e][i] <= 0.57)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.57 & inlist[e][i] <= 0.79)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.79 & inlist[e][i] <= 1.07)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 1.07 & inlist[e][i] <= 1.47)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 1.47)
                                {
                                    saxout[e].Add(14);
                                }
                                break;
                            case 15:
                                if (inlist[e][i] <= -1.5)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.5 & inlist[e][i] <= -1.11)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.11 & inlist[e][i] <= -0.84)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.84 & inlist[e][i] <= -0.62)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.62 & inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.43 & inlist[e][i] <= -0.25)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.25 & inlist[e][i] <= -0.08)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.08 & inlist[e][i] <= 0.08)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0.08 & inlist[e][i] <= 0.25)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.25 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.43 & inlist[e][i] <= 0.62)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.62 & inlist[e][i] <= 0.84)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.84 & inlist[e][i] <= 1.11)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 1.11 & inlist[e][i] <= 1.5)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 1.5)
                                {
                                    saxout[e].Add(15);
                                }
                                break;
                            case 16:
                                if (inlist[e][i] <= -1.53)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.53 & inlist[e][i] <= -1.15)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.15 & inlist[e][i] <= -0.89)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.89 & inlist[e][i] <= -0.67)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.67 & inlist[e][i] <= -0.49)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.49 & inlist[e][i] <= -0.32)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.32 & inlist[e][i] <= -0.16)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.16 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.16)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.16 & inlist[e][i] <= 0.32)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.32 & inlist[e][i] <= 0.49)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.49 & inlist[e][i] <= 0.67)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.67 & inlist[e][i] <= 0.89)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 0.89 & inlist[e][i] <= 1.15)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 1.15 & inlist[e][i] <= 1.53)
                                {
                                    saxout[e].Add(15);
                                }
                                if (inlist[e][i] >= 1.53)
                                {
                                    saxout[e].Add(16);
                                }
                                break;
                            case 17:
                                if (inlist[e][i] <= -1.56)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.56 & inlist[e][i] <= -1.19)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.19 & inlist[e][i] <= -0.93)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.93 & inlist[e][i] <= -0.72)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.72 & inlist[e][i] <= -0.54)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.54 & inlist[e][i] <= -0.38)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.38 & inlist[e][i] <= -0.22)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.22 & inlist[e][i] <= -0.07)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= -0.07 & inlist[e][i] <= 0.07)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0.07 & inlist[e][i] <= 0.22)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.22 & inlist[e][i] <= 0.38)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.38 & inlist[e][i] <= 0.54)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.54 & inlist[e][i] <= 0.72)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 0.72 & inlist[e][i] <= 0.93)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 0.93 & inlist[e][i] <= 1.19)
                                {
                                    saxout[e].Add(15);
                                }
                                if (inlist[e][i] >= 1.19 & inlist[e][i] <= 1.56)
                                {
                                    saxout[e].Add(16);
                                }
                                if (inlist[e][i] >= 1.56)
                                {
                                    saxout[e].Add(17);
                                }
                                break;
                            case 18:
                                if (inlist[e][i] <= -1.59)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.59 & inlist[e][i] <= -1.22)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.22 & inlist[e][i] <= -0.97)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -0.97 & inlist[e][i] <= -0.76)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.76 & inlist[e][i] <= -0.59)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.59 & inlist[e][i] <= -0.43)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.43 & inlist[e][i] <= -0.28)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.28 & inlist[e][i] <= -0.14)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= -0.14 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.14)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.14 & inlist[e][i] <= 0.28)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.28 & inlist[e][i] <= 0.43)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.43 & inlist[e][i] <= 0.59)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 0.59 & inlist[e][i] <= 0.76)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 0.76 & inlist[e][i] <= 0.97)
                                {
                                    saxout[e].Add(15);
                                }
                                if (inlist[e][i] >= 0.97 & inlist[e][i] <= 1.22)
                                {
                                    saxout[e].Add(16);
                                }
                                if (inlist[e][i] >= 1.22 & inlist[e][i] <= 1.59)
                                {
                                    saxout[e].Add(17);
                                }
                                if (inlist[e][i] >= 1.59)
                                {
                                    saxout[e].Add(18);
                                }
                                break;
                            case 19:
                                if (inlist[e][i] <= -1.62)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.62 & inlist[e][i] <= -1.25)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.25 & inlist[e][i] <= -1)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -1 & inlist[e][i] <= -0.8)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.8 & inlist[e][i] <= -0.63)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.63 & inlist[e][i] <= -0.48)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.48 & inlist[e][i] <= -0.34)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.34 & inlist[e][i] <= -0.2)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= -0.2 & inlist[e][i] <= -0.07)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= -0.07 & inlist[e][i] <= 0.07)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0.07 & inlist[e][i] <= 0.2)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.2 & inlist[e][i] <= 0.34)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.34 & inlist[e][i] <= 0.48)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 0.48 & inlist[e][i] <= 0.63)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 0.63 & inlist[e][i] <= 0.8)
                                {
                                    saxout[e].Add(15);
                                }
                                if (inlist[e][i] >= 0.8 & inlist[e][i] <= 1)
                                {
                                    saxout[e].Add(16);
                                }
                                if (inlist[e][i] >= 1 & inlist[e][i] <= 1.25)
                                {
                                    saxout[e].Add(17);
                                }
                                if (inlist[e][i] >= 1.25 & inlist[e][i] <= 1.62)
                                {
                                    saxout[e].Add(18);
                                }
                                if (inlist[e][i] >= 1.62)
                                {
                                    saxout[e].Add(19);
                                }
                                break;
                            case 20:
                                if (inlist[e][i] <= -1.64)
                                {
                                    saxout[e].Add(1);
                                }
                                else if (inlist[e][i] >= -1.64 & inlist[e][i] <= -1.28)
                                {
                                    saxout[e].Add(2);
                                }
                                else if (inlist[e][i] >= -1.28 & inlist[e][i] <= -1.04)
                                {
                                    saxout[e].Add(3);
                                }
                                if (inlist[e][i] >= -1.04 & inlist[e][i] <= -0.84)
                                {
                                    saxout[e].Add(4);
                                }
                                if (inlist[e][i] >= -0.84 & inlist[e][i] <= -0.67)
                                {
                                    saxout[e].Add(5);
                                }
                                if (inlist[e][i] >= -0.67 & inlist[e][i] <= -0.52)
                                {
                                    saxout[e].Add(6);
                                }
                                if (inlist[e][i] >= -0.52 & inlist[e][i] <= -0.39)
                                {
                                    saxout[e].Add(7);
                                }
                                if (inlist[e][i] >= -0.39 & inlist[e][i] <= -0.25)
                                {
                                    saxout[e].Add(8);
                                }
                                if (inlist[e][i] >= -0.25 & inlist[e][i] <= -0.13)
                                {
                                    saxout[e].Add(9);
                                }
                                if (inlist[e][i] >= -0.13 & inlist[e][i] <= 0)
                                {
                                    saxout[e].Add(10);
                                }
                                if (inlist[e][i] >= 0 & inlist[e][i] <= 0.13)
                                {
                                    saxout[e].Add(11);
                                }
                                if (inlist[e][i] >= 0.13 & inlist[e][i] <= 0.25)
                                {
                                    saxout[e].Add(12);
                                }
                                if (inlist[e][i] >= 0.25 & inlist[e][i] <= 0.39)
                                {
                                    saxout[e].Add(13);
                                }
                                if (inlist[e][i] >= 0.39 & inlist[e][i] <= 0.52)
                                {
                                    saxout[e].Add(14);
                                }
                                if (inlist[e][i] >= 0.52 & inlist[e][i] <= 0.67)
                                {
                                    saxout[e].Add(15);
                                }
                                if (inlist[e][i] >= 0.67 & inlist[e][i] <= 0.84)
                                {
                                    saxout[e].Add(16);
                                }
                                if (inlist[e][i] >= 0.84 & inlist[e][i] <= 1.04)
                                {
                                    saxout[e].Add(17);
                                }
                                if (inlist[e][i] >= 1.04 & inlist[e][i] <= 1.28)
                                {
                                    saxout[e].Add(18);
                                }
                                if (inlist[e][i] >= 1.28 & inlist[e][i] <= 1.64)
                                {
                                    saxout[e].Add(19);
                                }
                                if (inlist[e][i] >= 1.64)
                                {
                                    saxout[e].Add(20);
                                }
                                break;
                        }
                    }
                }
                for(int r=0;r<saxout.Count;r++)
                {
                    for (int i = 0; i < saxout[r].Count; i++)
                    {
                        textBox3.AppendText(Convert.ToString(saxout[r][i]) + Environment.NewLine);
                    }
                }

                //顯示第二切割結果的四段在textbox中
                
                /*for(int i=0;i<saxout[0].Count;i++)
                {
                    textBoxC1.AppendText(Convert.ToString(saxout[0][i]) + Environment.NewLine);
                }
                for (int i = 0; i < saxout[1].Count; i++)
                {
                    textBoxC2.AppendText(Convert.ToString(saxout[1][i]) + Environment.NewLine);
                }
                for (int i = 0; i < saxout[2].Count; i++)
                {
                    textBoxC3.AppendText(Convert.ToString(saxout[2][i]) + Environment.NewLine);
                }
                for (int i = 0; i < saxout[3].Count; i++)
                {
                    textBoxC4.AppendText(Convert.ToString(saxout[3][i]) + Environment.NewLine);
                }*/

                Debug.WriteLine("第二次SAX各組結果");
                for (int i = 0; i < saxout.Count; i++)
                {
                    Debug.WriteLine("第{0}段", i + 1);

                    for (int x = 0; x < saxout[i].Count; x++)
                    {
                        Debug.Write(saxout[i][x] + " ");
                    }

                    Debug.WriteLine("");
                }
                Debug.WriteLine("");
            }

            beging();
            b2();
            respread();
            listspread();
            sax2();
            team2(newsax);
        }
    }
}
