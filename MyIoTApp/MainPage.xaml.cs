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
using System.Threading;
using Windows.UI.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using ADCTESTREAD;

namespace MyIoTApp
{
    public sealed partial class MainPage : Page
    {
        ADCRead ADCRead = new ADCRead();
        SQLiteConnection conn;
        string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");  //建立資料庫


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
                ADCRead.InitSpi();



                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                timer.Tick += Timer_Tick;
                timer.Start();

                bgwWorker.DoWork += new DoWorkEventHandler(bgwWorker_DoWorkAsync);
                bgwWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwWorker_RunWorkerCompleted);
                bgwWorker.ProgressChanged += new ProgressChangedEventHandler(bgwWorker_ProgressChanged);
                bgwWorker.WorkerReportsProgress = true;
                bgwWorker.WorkerSupportsCancellation = true;
                bgwWorker.RunWorkerAsync(); //開始背景執行作業

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
        private async void bgwWorker_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //查詢資料表內所有資料
                List<Data> datalist = conn.Query<Data>("select * from Data");
                foreach (var item in datalist)
                {
                    result = item.Id + " " + item.Time + " " + item.Value + "\r\n";
                    number = item.Id;
                    time = item.Time;
                    data = Convert.ToSingle(item.Value);


                    //連結至php新增資料到資料庫
                    var addresult = new
                    {
                        Number = number,
                        Time = time,
                        Data = data
                    };


                    string post = JsonConvert.SerializeObject(addresult);
                    HttpClient gogo = new HttpClient();
                    HttpContent contentPost = new StringContent(post, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = gogo.PostAsync("https://ntutiem.000webhostapp.com/insert_ntutiemproject.php", contentPost).Result;
             
                    if (response.IsSuccessStatusCode)
                    {
                        //刪除已上傳紀錄的SQLite內資料
                        conn.Execute("delete from Data where Id = ?", number);

                        //背景執行作業進度回報
                        bgwWorker.ReportProgress(50);
                    }
                }

                    //執行續延遲
                    Task.Delay(300);                
            }
        }




        //背景執行作業進度改變時
        private void bgwWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //增加item進到listview並顯示上傳成功字樣
            this.Listview.Items.Add(result);
            this.Textbox.Text = "Upload：Done !";
        }

        //背景執行作業完成時
        private void bgwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }


        private void Timer_Tick(object sender, object e)
        {       
            double result = Math.Abs(ADCRead.CalcIrms(1480) - 62.9);
            Message.Text = string.Format("Current Value ={0}", result);
            conn.Insert(new Data() { Time = DateTime.Now.ToString() ,Value = result.ToString() });  //新增一筆資料
            Debug.WriteLine(path);
        }
        

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, new TimeSpan(0));
        }


        private void DelAll_Click(object sender, RoutedEventArgs e)
        {
            conn.DeleteAll<Data>();
        }
    }
}
