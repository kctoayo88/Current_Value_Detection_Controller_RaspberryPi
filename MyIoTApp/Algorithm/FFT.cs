using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFTWSharp;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace FFTTEST
{
    public partial class Form1 : Form
    {
        ToolTip tooltip = new ToolTip();
        Point? clickPosition = null;
        double[] plotarr;
        List<double> peakarr =new List<double>();
        
        string wrresultpath;
        double[] F;

        string RawfilePath1 = @"C:\Users\Ying\Desktop\FFT\SAXDATA1.txt"; //RAW DATA 檔案路徑
        string RawfilePath2 = @"C:\Users\Ying\Desktop\FFT\SAXDATA2.txt"; //RAW DATA 檔案路徑
        string RawfilePath3 = @"C:\Users\Ying\Desktop\FFT\SAXDATA3.txt"; //RAW DATA 檔案路徑
        string RawfilePath4 = @"C:\Users\Ying\Desktop\FFT\SAXDATA4.txt"; //RAW DATA 檔案路徑

        string ResultfilePath1 = @"C:\Users\Ying\Desktop\FFT\result-complexabs1.txt"; //轉換後 DATA 檔案路徑
        string ResultfilePath2 = @"C:\Users\Ying\Desktop\FFT\result-complexabs2.txt"; //轉換後 DATA 檔案路徑
        string ResultfilePath3 = @"C:\Users\Ying\Desktop\FFT\result-complexabs3.txt"; //轉換後 DATA 檔案路徑
        string ResultfilePath4 = @"C:\Users\Ying\Desktop\FFT\result-complexabs4.txt"; //轉換後 DATA 檔案路徑


        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            ffttt(RawfilePath1, ResultfilePath1);
            ffttt(RawfilePath2, ResultfilePath2);
            ffttt(RawfilePath3, ResultfilePath3);
            ffttt(RawfilePath4, ResultfilePath4);
        }

        private void ffttt(string path, string resultpath)
        {
            // Test 1: Real input
            var arrlenght = GetRows(path);
            // Define an array of double-precision numbers
            double[] dataarr = new double[arrlenght];
            string line = "";
            int counter = 0;
            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                //Insert data into arry
                dataarr[counter] = Convert.ToDouble(line);
                counter++;
            }
            file.Close();
            // Compute the FFT
            var dft = Fft(dataarr, true);     // true = real input
            // Format and display the results of the FFT
            Console.WriteLine("\nTest 1: Real input");
            Console.WriteLine("FFT =");
            plotarr = new double[dft.Length]; //用於plot的陣列變數
            wrresultpath = resultpath;
            DisplayComplexABS(dft);
            Console.WriteLine("總共" + counter + "筆");
            Console.WriteLine();
            this.chart.Series["value"].Points.Clear();
            fftplot();
            FindPeaks(peakarr);
            MessageBox.Show("Done!");
            peakarr.Clear();
        }

        /// <summary>
        /// Computes the fast Fourier transform of a 1-D array of real or complex numbers.
        /// </summary>
        /// <param name="data">Input data.</param>
        /// <param name="real">Real or complex input flag.</param>
        /// <returns>Returns the FFT.</returns>
        private static double[] Fft(double[] data, bool real)
        {
            // If the input is real, make it complex
            if (real)
                data = ToComplex(data);
            // Get the length of the array
            int n = data.Length;
            /* Allocate an unmanaged memory block for the input and output data.
             * (The input and output are of the same length in this case, so we can use just one memory block.) */
            IntPtr ptr = fftw.malloc(n * sizeof(double));
            // Pass the managed input data to the unmanaged memory block
            Marshal.Copy(data, 0, ptr, n);
            // Plan the FFT and execute it (n/2 because complex numbers are stored as pairs of doubles)
            IntPtr plan = fftw.dft_1d(n / 2, ptr, ptr, fftw_direction.Forward, fftw_flags.Estimate);
            fftw.execute(plan);
            // Create an array to store the output values
            var fft = new double[n];
            // Pass the unmanaged output data to the managed array
            Marshal.Copy(ptr, fft, 0, n);
            // Do some cleaning
            fftw.destroy_plan(plan);
            fftw.free(ptr);
            fftw.cleanup();
            // Return the FFT output
            return fft;
        }

        /// <summary>
        /// Interlaces an array with zeros to match the FFTW convention of representing complex numbers.
        /// </summary>
        /// <param name="real">An array of real numbers.</param>
        /// <returns>Returns an array of complex numbers.</returns>
        private static double[] ToComplex(double[] real)
        {
            int n = real.Length;
            var comp = new double[n * 2];
            for (int i = 0; i < n; i++)
                comp[2 * i] = real[i];
            return comp;
        }

        /// <summary>
        /// Displays complex numbers in the form a +/- bi.
        /// </summary>
        /// <param name="x">An array of complex numbers.</param>
        private void DisplayComplexABS(double[] x)  //結果顯示實數加虛數的絕對值 ((rea^2+ima^2)^0.5)
        {

            //逐行讀取txt
            StreamWriter sw = new StreamWriter(wrresultpath);
            if (x.Length % 2 != 0)
                throw new Exception("The number of elements must be even.");
            for (int i = 0, n = x.Length; i < n; i += 2)
            {
                double re;
                if (x[i + 1] < 0)
                {
                    Console.WriteLine(Math.Pow(x[i], 2) + Math.Pow(Math.Abs(x[i + 1]), 2));
                    re = Math.Sqrt(Math.Pow(x[i], 2) + Math.Pow(Math.Abs(x[i + 1]), 2));
                }
                else
                {
                    Console.WriteLine(Math.Pow(x[i], 2) + Math.Pow(x[i + 1], 2));
                    re = Math.Sqrt((Math.Pow(x[i], 2) + Math.Pow(x[i + 1], 2)));
                }
                //寫入txt
                sw.WriteLine(re);
                plotarr[i] = re;

            }
            //關閉txt
            sw.Close();
        }

        //取得DATA的長度
        public int GetRows(string FilePath)
        {
            using (StreamReader read = new StreamReader(FilePath, Encoding.Default))
            {
                return read.ReadToEnd().Split('\n').Length;
            }
        }

        //Plot FFT圖形
        private void fftplot()
        {
            contofre(plotarr);

            //計算 F[]
            int fs = 5; //取樣頻率
            F = new double[plotarr.Length];
            double Fcounter = 0;

            for (int i = 0; i < plotarr.Length - 1; i++)
            {
                F[i] = ((Fcounter * fs / plotarr.Length)) + 1;
                Fcounter++;
            }


            //將 F[],dataarr[] 從double轉換為string array
            string[] F2 = new string[F.Length];
            string[] dataarr2 = new string[plotarr.Length];

            for (int i = 0; i < plotarr.Length; i++)
            {
                F2[i] = F[i].ToString();
                dataarr2[i] = plotarr[i].ToString();

            }

            for (int i = 0; i < plotarr.Length; i+=2)
            {
                peakarr.Add(Convert.ToDouble(dataarr2[i]));              
            }

            Debug.WriteLine("peakarr");
            foreach (double i in peakarr)
            {
                Debug.WriteLine(i);
            }



            //控制圖形允許選取放大X軸
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;


            //將數值新增至序列
            for (int i = 1; i < dataarr2.Length / 2; i++)
            {
                this.chart.Series["value"].Points.AddXY(F2[i], dataarr2[i]);
            }
        }

        private void contofre (double[] dataarr)
        {
            for (int i = 0; i < dataarr.Length; i++)
            {
                dataarr[i] = dataarr[i] * 2 / dataarr.Length;
            }

            dataarr[0] = (dataarr[0]) / 10;

            return;
        }


        //滑鼠選擇放大X軸
        private void chart_MouseClick(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            clickPosition = pos;
            var results = chart.HitTest(pos.X, pos.Y, false, ChartElementType.PlottingArea);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.PlottingArea)
                {
                    var yVal = result.ChartArea.AxisY.PixelPositionToValue(pos.Y);
                    tooltip.Show("Y=" + yVal, this.chart, e.Location.X, e.Location.Y - 15);
                }
            }
        }


        //找3個Peak
        public void FindPeaks(List<double> peakarr)
        {
            var restrict = peakarr.Count / 100;

            double[] Peaks = new double[] { 0, 0, 0 };
            int[] Peaks_Index = new int[] { 0, 0, 0 };

            for (int k = 0; k < Peaks.Length; k++)
            {
                for (int i = 1; i < peakarr.Count/2; i++)
                {
                    if (Peaks[k] < peakarr[i])
                    {
                        switch (k)
                        {
                            case 0:
                                Peaks[0] = peakarr[i];
                                Peaks_Index[0] = i;
                                break;

                            case 1:
                                if (Math.Abs(i - Peaks_Index[0]) < restrict / 2)
                                {
                                    continue;
                                }
                                Peaks[1] = peakarr[i];
                                Peaks_Index[1] = i;
                                break;

                            case 2:
                                if ((Math.Abs(i - Peaks_Index[0]) < restrict / 2) || (Math.Abs(i - Peaks_Index[1]) < restrict / 2))
                                {
                                    continue;
                                }
                                Peaks[2] = peakarr[i];
                                Peaks_Index[2] = i;
                                break;
                        }
                    }
                }
            }

            for (int i = 0; i < Peaks.Length; i++)
            {
                Debug.WriteLine("Index:{0}  Value:{1}  X:{2}",Peaks_Index[i],Peaks[i],F[Peaks_Index[i]]);
            }
            
        }

        private void chart_Click(object sender, EventArgs e)
        {

        }

    }
}

