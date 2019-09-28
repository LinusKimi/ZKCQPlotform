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

    public partial class MainWindow : Window
    {
        private MsgServer _messageServer = new MsgServer(Dispatcher.CurrentDispatcher);
        private readonly UsbServer _usbServer;
        private readonly NetServer _netServer;

        private string _usbstartcom = null;
        private string _usbstopcom = null;
        private string _netstartcom = null;
        private string _netstopcom = null;

        private ActionBlock<byte[]> _usbactionblock;
        private ActionBlock<byte[]> _netactionblock;


        public MainWindow()
        {
            InitializeComponent();

            _usbactionblock = RegisterMethod<byte[]>(UsbSaveAction);
            _netactionblock = RegisterMethod<byte[]>(NetSaveAction);

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
            if (_usbServer._usbdatastate == PlotformUSB.Datastate.start)
            {
                if (_usbServer._usbfilepath != string.Empty)
                {
                    _usbServer._usbsw.Write(data);
                    _usbServer._usbsw.Flush();
                }
                _usbServer._usbframecnt--;
                if (_usbServer._usbframecnt <= 0)
                {
                    _usbServer._usbdatastate = PlotformUSB.Datastate.stop;
                    _usbServer._usbsw.Dispose();
                    _usbServer._usbfilestream.Dispose();

                    Dispatcher.Invoke(() =>
                    {
                        usbsavedata.IsEnabled = true;
                        usbfilepath.IsEnabled = true;
                        usbsetting.IsEnabled = true;
                    });
                }

            }
        }

        private void NetSaveAction(byte[] data)
        {
            if (_netServer._netdatastate == PlotformNET.Datastate.start)
            {
                if (_netServer._netfilepath != string.Empty)
                {
                    _netServer._netsw.Write(data);
                    _netServer._netsw.Flush();
                }
                _netServer._netframecnt--;
                if (_netServer._netframecnt <= 0)
                {
                    _netServer._netdatastate = PlotformNET.Datastate.stop;
                    _netServer._netsw.Dispose();
                    _netServer._netfilestream.Dispose();

                    Dispatcher.Invoke(()=>
                    {

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

            iptextbox.Text = Properties.Settings.Default.netip;
            porttextbox.Text = Properties.Settings.Default.netport;
            netstartcom.Text = Properties.Settings.Default.netstartcom;
            netstopcom.Text = Properties.Settings.Default.netstopcom;

            _usbstartcom = Properties.Settings.Default.usbstartcom;
            _usbstopcom = Properties.Settings.Default.usbstopcom;

            _netstartcom = Properties.Settings.Default.netstartcom;
            _netstopcom = Properties.Settings.Default.netstopcom;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.usbbytecnt = bytecnt.Text;
            Properties.Settings.Default.usbframecnt = framecnt.Text;
            Properties.Settings.Default.usbstartcom = _usbstartcom;
            Properties.Settings.Default.usbstopcom = _usbstopcom;

            Properties.Settings.Default.netstartcom = _netstartcom;
            Properties.Settings.Default.netstopcom = _netstopcom;

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
            if (_usbServer._usbfilepath == "")
            {
                var mDialog = new FolderBrowserDialog();
                DialogResult result = mDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                _usbServer._usbfilepath = mDialog.SelectedPath.Trim();
            }
            _usbServer._usbsavecnt++;
            _usbServer._usbframecnt = int.Parse(framecnt.Text.Trim());
            var filename = DateTime.Now.ToString("MM-dd第") + _usbServer._usbsavecnt.ToString() + "次usb数据";
            string path = _usbServer._usbfilepath + "\\" + filename;
            _usbServer._usbfilestream = new FileStream(path, FileMode.Create, FileAccess.Write);
            _usbServer._usbsw = new BinaryWriter(_usbServer._usbfilestream);

            usbsavedata.IsEnabled = false;
            usbfilepath.IsEnabled = false;
            usbsetting.IsEnabled = false;

            _usbServer._usbdatastate = PlotformUSB.Datastate.start;
        }

        private void Usbfilepath_Click(object sender, RoutedEventArgs e)
        {
            var mDialog = new FolderBrowserDialog();
            DialogResult result = mDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            _usbServer._usbfilepath = mDialog.SelectedPath.Trim();
        }

        private void Usbsetting_Click(object sender, RoutedEventArgs e)
        {
            _usbstartcom = uabstartcom.Text;
            _usbstopcom = uabstopcom.Text;
            _messageServer.AddWindowsMsg("协议参数已保存！");
        }

        private void Netconnect_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)netconnect.IsChecked)
            {
                if (iptextbox.Text == string.Empty || porttextbox.Text == string.Empty)
                    _messageServer.AddWindowsMsg("请正确填写网络信息");
                else
                {
                    if (_netServer.Start(iptextbox.Text, Convert.ToUInt16(porttextbox.Text)))
                    {
                        if (_netstartcom != string.Empty)
                        {
                            _netstartcom = netstartcom.Text;
                            if (_netServer.NetSend(TextToByteArry(_netstartcom), TextToByteArry(_netstartcom).Length))
                                _messageServer.AddMsg(_messageServer._netBindList, " > Send start commend !");
                        }
                        //_messageServer.AddWindowsMsg("网络成功 ！");
                    }
                    else
                    {
                        _messageServer.AddWindowsMsg("网络服务启动失败！");
                    }
                } 
            }
            else
            {
                if (_netstopcom != string.Empty)
                {
                    if (_netServer.NetSend(TextToByteArry(_netstopcom), TextToByteArry(_netstopcom).Length))
                    {
                        if (_netServer.Stop())
                        {
                            _netServer.Destroy();
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        _messageServer.AddWindowsMsg("发送结束指令错误！");
                    }
                }
                else
                {
                    if (_netServer.Stop())
                    {
                        _netServer.Destroy();
                    }
                    else
                    {

                    }
                }
            }
        }

        private void Netsavedata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Netchangepath_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
