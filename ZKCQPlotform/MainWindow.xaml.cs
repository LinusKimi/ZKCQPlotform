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
using System.Threading.Tasks.Dataflow;

using PlotformUSB;
using PlotformNET;
using PlotformMSG;

using System.IO;
using System.Windows.Forms;

namespace ZKCQPlotform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public enum Datastate { start, stop}
    public partial class MainWindow : Window
    {
        private MsgServer _messageServer = new MsgServer(Dispatcher.CurrentDispatcher);
        private readonly UsbServer _usbServer;
        private readonly NetServer _netServer;

        private string _usbstartcom = null;
        private string _usbstopcom = null;

        private int _usbsavecnt = 0;
        private int _usbframecnt = 0;
        private string _usbfilepath = "";
        private FileStream _usbfilestream = null;
        private BinaryWriter _usbsw = null;
        private Datastate _usbdatastate = Datastate.stop;

        private ActionBlock<byte[]> _usbactionblock;
        private ActionBlock<byte[]> _netactionblock;

        public MainWindow()
        {
            InitializeComponent();

            _usbactionblock = RegisterMethod<byte[]>(UsbSaveAction);

            _usbServer = new UsbServer(_messageServer, _usbactionblock);
            _netServer = new NetServer(_messageServer, _netactionblock);

            usbmsgbox.ItemsSource = _messageServer._usbBindList;
            usbdevlist.ItemsSource = _messageServer._usbDeviceList;

            netmsgbox.ItemsSource = _messageServer._netBindList;
            netdevlist.ItemsSource = _messageServer._netDeviceList;
        }

        private ActionBlock<T> RegisterMethod<T>(Action<T> action) => new ActionBlock<T>(action);

        private void UsbSaveAction(byte[] data)
        {
            if (_usbdatastate == Datastate.start)
            {
                if (_usbfilepath != string.Empty)
                {
                    _usbsw.Write(data);
                    _usbsw.Flush();
                }
                _usbframecnt--;
                if (_usbframecnt <= 0)
                {
                    _usbdatastate = Datastate.stop;
                    _usbsw.Dispose();
                    _usbfilestream.Dispose();

                    Dispatcher.Invoke(() =>
                    {
                        usbsavedata.IsEnabled = true;
                        usbfilepath.IsEnabled = true;
                        usbsetting.IsEnabled = true;
                    });
                }

            }
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
            usbconnect.IsEnabled = false;
            usbsavedata.IsEnabled = false;
            usbfilepath.IsEnabled = false;
            usbsetting.IsEnabled = false;

            bytecnt.Text = Properties.Settings.Default.usbbytecnt;
            framecnt.Text = Properties.Settings.Default.usbframecnt;
            uabstartcom.Text = Properties.Settings.Default.usbstartcom;
            uabstopcom.Text = Properties.Settings.Default.usbstopcom;

            _usbstartcom = Properties.Settings.Default.usbstartcom;
            _usbstopcom = Properties.Settings.Default.usbstopcom;

 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.usbbytecnt = bytecnt.Text;
            Properties.Settings.Default.usbframecnt = framecnt.Text;
            Properties.Settings.Default.usbstartcom = _usbstartcom;
            Properties.Settings.Default.usbstopcom = _usbstopcom;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }


        private void Searchdevice_Click(object sender, RoutedEventArgs e)
        {

            if (_usbServer.UsbDeviceFinder())
            {
                usbconnect.IsEnabled = true;
                usbsavedata.IsEnabled = true;
                usbfilepath.IsEnabled = true;
                usbsetting.IsEnabled = true;
            }
            else
            {
                usbconnect.IsEnabled = false;
                usbsavedata.IsEnabled = false;
                usbfilepath.IsEnabled = false;
                usbsetting.IsEnabled = false;
            }
        }

        private void Usbconnect_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)usbconnect.IsChecked)
            {
                if (_usbServer.UsbConnect((bool)usbconnect.IsChecked, int.Parse(bytecnt.Text.Trim())))
                {
                    if (_usbstartcom != string.Empty)
                    {
                        if (!_usbServer.UsbStartComm(TextToByteArry(_usbstartcom)))
                            _messageServer.AddWindowsMsg("发送开始指令错误！");
                    }
                }
                else
                {
                    usbconnect.IsChecked = false;
                }
            }
            else
            {
                if (_usbstopcom != string.Empty)
                {           
                    if (_usbServer.UsbStopComm(TextToByteArry(_usbstopcom)))
                    {
                        if (!_usbServer.UsbConnect((bool)usbconnect.IsChecked, int.Parse(bytecnt.Text.Trim())))
                        {
                            _messageServer.AddWindowsMsg("设备断开失败，请重新上电！");
                        }
                        else
                        {
                            usbconnect.IsEnabled = false;
                            usbsavedata.IsEnabled = false;
                            usbfilepath.IsEnabled = false;
                            usbsetting.IsEnabled = false;
                        }
                    }
                    else
                    {
                        _messageServer.AddWindowsMsg("发送结束指令错误！");

                    }
                }
                else
                {
                    if (!_usbServer.UsbConnect((bool)usbconnect.IsChecked, int.Parse(bytecnt.Text.Trim())))
                    {
                        _messageServer.AddWindowsMsg("设备断开失败，请重新上电！");
                    }
                    else
                    {
                        usbconnect.IsEnabled = false;
                        usbsavedata.IsEnabled = false;
                        usbfilepath.IsEnabled = false;
                        usbsetting.IsEnabled = false;
                    }
                }

            }

        }

        private void Usbsavedata_Click(object sender, RoutedEventArgs e)
        {
            if (_usbfilepath == "")
            {
                var mDialog = new FolderBrowserDialog();
                DialogResult result = mDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                _usbfilepath = mDialog.SelectedPath.Trim();
            }
            _usbsavecnt++;
            _usbframecnt = int.Parse(framecnt.Text.Trim());
            var filename = DateTime.Now.ToString("MM-dd第") + _usbsavecnt.ToString() + "次usb数据";
            string path = _usbfilepath + "\\" + filename;
            _usbfilestream = new FileStream(path, FileMode.Create, FileAccess.Write);
            _usbsw = new BinaryWriter(_usbfilestream);

            usbsavedata.IsEnabled = false;
            usbfilepath.IsEnabled = false;
            usbsetting.IsEnabled = false;

            _usbdatastate = Datastate.start;
        }

        private void Usbfilepath_Click(object sender, RoutedEventArgs e)
        {
            var mDialog = new FolderBrowserDialog();
            DialogResult result = mDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            _usbfilepath = mDialog.SelectedPath.Trim();
        }

        private void Usbsetting_Click(object sender, RoutedEventArgs e)
        {
            _usbstartcom = uabstartcom.Text;
            _usbstopcom = uabstopcom.Text;
            _messageServer.AddWindowsMsg("协议参数已保存！");
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
