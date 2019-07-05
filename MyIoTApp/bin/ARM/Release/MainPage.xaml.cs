using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.NetworkInformation;
using Windows.UI.Popups;
using Windows.System;

namespace MyIoTApp
{
    public sealed partial class MainPage : Page
    {
        SQLiteConnection conn;
        string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");  //建立資料庫

        private GpioPin Input0,Input1, Input2, Input3, Input4, Input5, Input6, Input7;

        string result;
        float number;
        string time;
        float data;

        private DispatcherTimer timer;


        BackgroundWorker bgwWorker = new BackgroundWorker();


        public MainPage()
        {
            this.InitializeComponent();

            bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();

            if (isInternetConnected == true)
            {
                conn = new SQLiteConnection(new SQLitePlatformWinRT(), path);  //資料庫連接
                conn.CreateTable<Data>();  //建立資料表

                GpioController gpio = GpioController.GetDefault();
                Input0 = gpio.OpenPin(4);
                Input0.SetDriveMode(GpioPinDriveMode.Input);
                Input1 = gpio.OpenPin(5);
                Input1.SetDriveMode(GpioPinDriveMode.Input);
                Input2 = gpio.OpenPin(17);
                Input2.SetDriveMode(GpioPinDriveMode.Input);
                Input3 = gpio.OpenPin(18);
                Input3.SetDriveMode(GpioPinDriveMode.Input);
                Input4 = gpio.OpenPin(22);
                Input4.SetDriveMode(GpioPinDriveMode.Input);
                Input5 = gpio.OpenPin(23);
                Input5.SetDriveMode(GpioPinDriveMode.Input);
                Input6 = gpio.OpenPin(24);
                Input6.SetDriveMode(GpioPinDriveMode.Input);
                Input7 = gpio.OpenPin(25);
                Input7.SetDriveMode(GpioPinDriveMode.Input);

                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(400)
                };
                timer.Tick += Timer_Tick;
                timer.Start();



                bgwWorker.DoWork += new DoWorkEventHandler(bgwWorker_DoWork);
                bgwWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwWorker_RunWorkerCompleted);
                bgwWorker.ProgressChanged += new ProgressChangedEventHandler(bgwWorker_ProgressChanged);
                bgwWorker.WorkerReportsProgress = true;
                bgwWorker.WorkerSupportsCancellation = true;

            }

            else
            {
                wifisettingAsync();
            }                
        }

        private async void wifisettingAsync ()
        {
            await LaunchAppAsync("wifisetting://");
        }

        private async Task LaunchAppAsync(string uriStr)
        {
            Uri uri = new Uri(uriStr);
            var promptOptions = new Windows.System.LauncherOptions();
            promptOptions.TreatAsUntrusted = false;

            bool isSuccess = await Windows.System.Launcher.LaunchUriAsync(uri, promptOptions);

            if (!isSuccess)
            {
                string msg = "Launch failed";
                await new MessageDialog(msg).ShowAsync();
            }
        }



        //背景執行作業
        private void bgwWorker_DoWork(object sender, DoWorkEventArgs e)
        {


            List<Data> datalist = conn.Query<Data>("select * from Data");   //查詢資料表內所有資料
            foreach (var item in datalist)
            {
                result =item.Id + " " + item.Time + " " + item.Value + "\r\n";
            
                number = item.Id;
                time = item.Time;
                data = Convert.ToSingle(item.Value);

            }

            bgwWorker.ReportProgress(99);  //背景執行作業進度回報

                               
        }

        //背景執行作業進度改變時
        private void bgwWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Listview.Items.Add(result);  //增加item進到listview

            //連結至php新增資料到資料庫php

            var addresult = new
            {
                Number = number,
                Time = time,
                Data = data
            };

            string post = JsonConvert.SerializeObject(addresult);
            HttpClient gogo = new HttpClient();
            HttpContent contentPost = new StringContent(post, Encoding.UTF8, "application/json");
            HttpResponseMessage response = gogo.PostAsync("https://ntutiem.000webhostapp.com/test.php", contentPost).Result;
            Textbox.Text = "Upload：Done !";


            //刪除已上傳紀錄的SQLite內資料
            conn.Execute("delete from Data where Id = ?", number);
        }

        //背景執行作業完成時
        private void bgwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }


        private void Timer_Tick(object sender, object e)
        {
            int value = 0;
            value += ReadInput(Input0);
            value += (ReadInput(Input1) << 1);
            value += (ReadInput(Input2) << 2);
            value += (ReadInput(Input3) << 3);
            value += (ReadInput(Input4) << 4);
            value += (ReadInput(Input5) << 5);
            value += (ReadInput(Input6) << 6);
            value += (ReadInput(Input7) << 7);
            value = value - 212+24;

            Message.Text = string.Format("Analog Value ={0}", value);

            conn.Insert(new Data() { Time = DateTime.Now.ToString() ,Value = value.ToString() });  //新增一筆資料

            Debug.WriteLine(path);

            bgwWorker.RunWorkerAsync();  //開始背景執行作業
        }


        private int ReadInput(GpioPin iGpioPin)
        {
            GpioPinValue value = iGpioPin.Read();
            if (value == GpioPinValue.High)
            {
                return 1;
            }
            return 0;
        }
        

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, new TimeSpan(0));
        }


        private void DelAll_Click(object sender, RoutedEventArgs e)
        {
            conn.DeleteAll<Data>();
        }
    }
}
