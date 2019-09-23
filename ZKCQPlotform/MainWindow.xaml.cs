using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using LibUsbDotNet;
using LibUsbDotNet.Main;

using PlotformUSB;
using PlotformMSG;

namespace ZKCQPlotform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UsbServer _usbServer;

        public MainWindow()
        {
            InitializeComponent();

            var _messageServer = new MsgServer(Dispatcher.CurrentDispatcher);
            _usbServer = new UsbServer(_messageServer);

            usbmsgbox.ItemsSource = _messageServer._usbBindList;
            usbdevlist.ItemsSource = _messageServer._usbDeviceList;

        }

        private byte[] TextToByteArry(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //usbconnect.IsEnabled = false;
            //usbsavedata.IsEnabled = false;
            //usbfilepath.IsEnabled = false;
            //usbsetting.IsEnabled = false;

            bytecnt.Text = Properties.Settings.Default.usbbytecnt;
            framecnt.Text = Properties.Settings.Default.usbframecnt;
            uabsendcom.Text = Properties.Settings.Default.usbstartcom;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.usbbytecnt = bytecnt.Text;
            Properties.Settings.Default.usbframecnt = framecnt.Text;
            Properties.Settings.Default.usbstartcom = uabsendcom.Text;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Searchdevice_Click(object sender, RoutedEventArgs e)
        {

            if (_usbServer.UsbDeviceFinder())
            {
                //usbconnect.IsEnabled = true;
                //usbsavedata.IsEnabled = true;
                //usbfilepath.IsEnabled = true;
                //usbsetting.IsEnabled = true;
            }
            else
                ;
        }

        private void Usbconnect_Click(object sender, RoutedEventArgs e)
        {
            //if ((bool)usbconnect.IsChecked)
            //{
            //    if (_usbServer.UsbConnect((bool)usbconnect.IsChecked, int.Parse(bytecnt.Text.Trim())))
            //    {

            //    }
            //}
            //else
            //{

            //}

            
        }

        private void Usbsavedata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Usbfilepath_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Usbsetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Netconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Netsavedata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Netchangepath_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
