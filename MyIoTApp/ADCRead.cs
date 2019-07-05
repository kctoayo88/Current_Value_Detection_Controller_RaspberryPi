using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.UI.Xaml;

namespace ADCTESTREAD
{
    class ADCRead
    {
        //spi定義類比轉數位晶片
        SpiDevice ADC;

        // The ADC response is 10 bits. We can fit it into 2 bytes. We add one byte padding. Hence, 
        byte[] responseBuffer = new byte[3] ;
        // In SPI communication, for every byte we want to receive, we send one byte
        // Therefore, the request buffers also are 3 bytes long
        byte[] range1Query = new byte[3] { 0x01, 0x80, 0 };
        // For sensor 1, we want to send 0000 0001 1000 xxxx xxxx xxxx
        // For sensor 2, we want to send 0000 0001 1001 xxxx xxxx xxxx
        // convert it to hex:               0    1    8    0    0    0  
        // and                              0    1    9    0    0    0


        //類比轉數位晶片Spi設定
        public async void InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(0)                         // Chip Select line 0
                {
                    ClockFrequency = 3600000,                                    // Don't exceed 3.6 MHz
                    Mode = SpiMode.Mode0,
                };

                string spiAqs = SpiDevice.GetDeviceSelector("SPI0");                /* Find the selector string for the SPI bus controller          */
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);     /* Find the SPI bus controller device with our selector string  */
                ADC = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);     /* Create an SpiDevice with our bus controller and SPI settings */
                Debug.WriteLine("InitSpi successful");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitSpi threw " + ex);
            }
        }


        int ADC_BITS = 10; //晶片bit數
        int sampleI,ADC_COUNTS;
        double offsetI,filteredI, sqI, sumI, Irms;
        double ICAL = 65.0;  // 110V 校準值
        int SupplyVoltage = 3300; //感測器輸入電壓(3.3V)


        //電流值計算
        public double CalcIrms(int NUMBER_OF_SAMPLES)
        {
            
            ADC_COUNTS = (1 << ADC_BITS);
            offsetI = ADC_COUNTS >> 1;

            sumI = 0;
            for (int n = 0; n < NUMBER_OF_SAMPLES; n++)
            {

                ADC.TransferFullDuplex(range1Query, responseBuffer);
                sampleI = ((responseBuffer[1] & 3) << 8) + responseBuffer[2];


                // Digital low pass filter extracts the 2.5 V or 1.65 V dc offset,
                //  then subtract this - signal is now centered on 0 counts.
                offsetI = (offsetI + (sampleI - offsetI) / 1024);
                filteredI = sampleI - offsetI;

                // Root-mean-square method current
                // 1) square current values
                sqI = filteredI * filteredI;
                // 2) sum
                sumI += sqI;
            }

            double I_RATIO = ICAL * ((SupplyVoltage / 1000.0) / (ADC_COUNTS));
            Irms = I_RATIO * Math.Sqrt(sumI / NUMBER_OF_SAMPLES);

            //Reset accumulators
            //sumI = 0;
            //--------------------------------------------------------------------------------------

            return Irms;
        }
    }
}
