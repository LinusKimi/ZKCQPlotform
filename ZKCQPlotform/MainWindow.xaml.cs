﻿using System;
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
using PlotformUart;
using System.Windows.Controls.Primitives;

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
        private readonly UartServer _uartServer;

        private ActionBlock<byte[]> _usbactionblock;
        private ActionBlock<byte[]> _netactionblock;
        private ActionBlock<byte[]> _uartactionblock;

        public MainWindow()
        {
            InitializeComponent();

            _usbactionblock = RegisterMethod<byte[]>(UsbSaveAction);
            _netactionblock = RegisterMethod<byte[]>(NetSaveAction);
            _uartactionblock = RegisterMethod<byte[]>(UartSaveAction);

            _usbServer = new UsbServer(_messageServer, _usbactionblock);
            _netServer = new NetServer(_messageServer, _netactionblock);
            _uartServer = new UartServer(_messageServer, _uartactionblock);

            usbmsgbox.ItemsSource = _messageServer._usbBindList;
            usbdevlist.ItemsSource = _messageServer._usbDeviceList;

            netmsgbox.ItemsSource = _messageServer._netBindList;
            netdevlist.ItemsSource = _messageServer._netDeviceList;

            uartmsgbox.ItemsSource = _messageServer._uartBindList;
            
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
                        netconnect.IsEnabled = true;
                        startListen.IsEnabled = true;
                        stopListen.IsEnabled = true;
                        netsavedata.IsEnabled = true;
                        netchangepath.IsEnabled = true;
                    });
                }
            }
        }

        private void UartSaveAction(byte[] data)
        {
            if (_uartServer._uartdatastate == PlotformUart.Datastate.start)
            {
                if (_uartServer._uartfilepath != string.Empty)
                {
                    _uartServer._uartsw.Write(data);
                    _uartServer._uartsw.Flush();
                }
                _uartServer._uartframecnt--;
                if (_uartServer._uartframecnt <= 0)
                {
                    _uartServer._uartdatastate = PlotformUart.Datastate.stop;
                    _uartServer._uartsw.Dispose();
                    _uartServer._uartfilestream.Dispose();

                    Dispatcher.Invoke(()=>
                    {
                        uartsavedata.IsEnabled = true;
                        uartchangepath.IsEnabled = true;
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
            System.Windows.Data.Binding binding1 = new System.Windows.Data.Binding { Source = _uartServer, Path = new PropertyPath("PortNames") };
            System.Windows.Data.Binding binding2 = new System.Windows.Data.Binding { Source = _uartServer, Path = new PropertyPath("DataBits") };
            System.Windows.Data.Binding binding3 = new System.Windows.Data.Binding { Source = _uartServer, Path = new PropertyPath("Parity") };
            System.Windows.Data.Binding binding4 = new System.Windows.Data.Binding { Source = _uartServer, Path = new PropertyPath("StopBits") };

            uartport.SetBinding(ItemsControl.ItemsSourceProperty, binding1);
            uartdatabit.SetBinding(Selector.SelectedItemProperty, binding2);
            uartenparity.SetBinding(Selector.SelectedItemProperty, binding3);
            uartstopbit.SetBinding(Selector.SelectedItemProperty, binding4);

            usbconnect.IsEnabled = false;
            usbstartrecv.IsEnabled = false;
            usbstoprecv.IsEnabled = false;
            usbsavedata.IsEnabled = false;
            usbfilepath.IsEnabled = false;

            startListen.IsEnabled = false;
            stopListen.IsEnabled = false;

            bytecnt.Text = Properties.Settings.Default.usbbytecnt;
            framecnt.Text = Properties.Settings.Default.usbframecnt;
            uabstartcom.Text = Properties.Settings.Default.usbstartcom;
            uabstopcom.Text = Properties.Settings.Default.usbstopcom;

            iptextbox.Text = Properties.Settings.Default.netip;
            porttextbox.Text = Properties.Settings.Default.netport;
            netframecnt.Text = Properties.Settings.Default.netframecnt;
            netstartcom.Text = Properties.Settings.Default.netstartcom;
            netstopcom.Text = Properties.Settings.Default.netstopcom;

            uartframecnt.Text = Properties.Settings.Default.uartframecnt;
            uartheardflag.Text = Properties.Settings.Default.uartheartflag;
            uartheaderlength.Text = Properties.Settings.Default.uartheartlen;
            uartdatalength.Text = Properties.Settings.Default.uartdatelen;
            uartstartcom.Text = Properties.Settings.Default.uartstartcom;
            uartstopcom.Text = Properties.Settings.Default.uartstopcom;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.usbbytecnt = bytecnt.Text;
            Properties.Settings.Default.usbframecnt = framecnt.Text;
            Properties.Settings.Default.usbstartcom = uabstartcom.Text;
            Properties.Settings.Default.usbstopcom = uabstopcom.Text;

            Properties.Settings.Default.netip = iptextbox.Text;
            Properties.Settings.Default.netport = porttextbox.Text;
            Properties.Settings.Default.netframecnt = netframecnt.Text;
            Properties.Settings.Default.netstartcom = netstartcom.Text;
            Properties.Settings.Default.netstopcom = netstopcom.Text;

            Properties.Settings.Default.uartframecnt = uartframecnt.Text;
            Properties.Settings.Default.uartheartflag = uartheardflag.Text;
            Properties.Settings.Default.uartheartlen = uartheaderlength.Text;
            Properties.Settings.Default.uartdatelen = uartdatalength.Text;
            Properties.Settings.Default.uartstartcom = uartstartcom.Text;
            Properties.Settings.Default.uartstopcom = uartstopcom.Text;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_netServer != null)
                _netServer.Destroy();
        }

        private void Searchdevice_Click(object sender, RoutedEventArgs e)
        {

            if (_usbServer.UsbDeviceFinder())
            {
                usbconnect.IsEnabled = true;              
                usbsavedata.IsEnabled = true;
                usbfilepath.IsEnabled = true;
            }
            else
            {
                usbconnect.IsEnabled = false;
                usbsavedata.IsEnabled = false;
                usbfilepath.IsEnabled = false;
            }
        }

        private void Usbconnect_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)usbconnect.IsChecked)
            {
                if (_usbServer.UsbConnect((bool)usbconnect.IsChecked, int.Parse(bytecnt.Text.Trim())))
                {
                    usbstartrecv.IsEnabled = true;
                    usbstoprecv.IsEnabled = true;
                }
                else
                {
                    usbconnect.IsChecked = false;
                    usbstartrecv.IsEnabled = false;
                    usbstoprecv.IsEnabled = false;
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
                }

            }

        }

        private void Usbstartrecv_Click(object sender, RoutedEventArgs e)
        {
            uabstartcom.IsEnabled = false;
            uabstopcom.IsEnabled = false;
            if (uabstartcom.Text != string.Empty)
            {
                if (!_usbServer.UsbStartComm(TextToByteArry(uabstartcom.Text)))
                {
                    uabstartcom.IsEnabled = true;
                    uabstopcom.IsEnabled = true;
                    _messageServer.AddWindowsMsg("发送开始指令错误！");
                }
            }
        }

        private void Usbstoprecv_Click(object sender, RoutedEventArgs e)
        {
            uabstartcom.IsEnabled = true;
            uabstopcom.IsEnabled = true;

            if (uabstopcom.Text != string.Empty)
            {
                if (_usbServer.UsbStopComm(TextToByteArry(uabstopcom.Text)))
                {; }
                else
                {
                    _messageServer.AddWindowsMsg("发送结束指令错误！");
                    return;
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
            _usbServer._usbfilestream = new FileStream(@path, FileMode.Create, FileAccess.Write);
            _usbServer._usbsw = new BinaryWriter(_usbServer._usbfilestream);

            usbsavedata.IsEnabled = false;
            usbfilepath.IsEnabled = false;

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
                        _messageServer.AddMsg(_messageServer._netBindList, $" > Start Server success !");
                        startListen.IsEnabled = true;
                    }
                    else
                    {
                        _messageServer.AddWindowsMsg("网络服务启动失败！");
                    }
                } 
            }
            else
            {
                if (_netServer.Stop())
                {
                    ;
                }
                else
                {
                    _messageServer.AddWindowsMsg("网络服务停止失败！");
                }

            }
        }

        private void StartListen_Click(object sender, RoutedEventArgs e)
        {
            netstartcom.IsEnabled = false;
            netstopcom.IsEnabled = false;
            if (netstartcom.Text != string.Empty)
            {
                if (_netServer.NetSend(TextToByteArry(netstartcom.Text), TextToByteArry(netstartcom.Text).Length))
                {
                    _messageServer.AddMsg(_messageServer._netBindList, $" > Send Start commend success !");
                    startListen.IsEnabled = false;
                    stopListen.IsEnabled = true;
                }
                else
                {
                    netstartcom.IsEnabled = true;
                    netstopcom.IsEnabled = true;
                }
            }
            
        }

        private void StopListen_Click(object sender, RoutedEventArgs e)
        {
            netstartcom.IsEnabled = true;
            netstopcom.IsEnabled = true;

            if (netstopcom.Text != string.Empty)
            {
                if (_netServer.NetSend(TextToByteArry(netstopcom.Text), TextToByteArry(netstopcom.Text).Length))
                {
                    _messageServer.AddMsg(_messageServer._netBindList, $" > Send Stop commend success !");
                    startListen.IsEnabled = true;
                    stopListen.IsEnabled = false;
                }
            }
        }

        private void Netsavedata_Click(object sender, RoutedEventArgs e)
        {
            if (_netServer._netfilepath == "")
            {
                var mDialog = new FolderBrowserDialog();
                DialogResult result = mDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                _netServer._netfilepath = mDialog.SelectedPath.Trim();
            }
            _netServer._netsavecnt++;
            _netServer._netframecnt = int.Parse(netframecnt.Text.Trim());
            var filename = DateTime.Now.ToString("MM-dd第") + _netServer._netsavecnt.ToString() + "次net数据";
            string path = _netServer._netfilepath + "\\" + filename;
            _netServer._netfilestream = new FileStream(path, FileMode.Create, FileAccess.Write);
            _netServer._netsw = new BinaryWriter(_netServer._netfilestream);

            netconnect.IsEnabled = false;
            startListen.IsEnabled = false;
            stopListen.IsEnabled = false;
            netsavedata.IsEnabled = false;
            netchangepath.IsEnabled = false;

            _netServer._netdatastate = PlotformNET.Datastate.start;
        }

        private void Netchangepath_Click(object sender, RoutedEventArgs e)
        {
            var mDialog = new FolderBrowserDialog();
            DialogResult result = mDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;
            _netServer._netfilepath = mDialog.SelectedPath.Trim();
        }

        private void Uartport_DropDownOpened(object sender, EventArgs e)
        {
            _uartServer.CheckPort();
        }

        private void Uartconnect_Click(object sender, RoutedEventArgs e)
        {
            _uartServer._uartheardflag = TextToByteArry(uartheardflag.Text);
            _uartServer._uartheardlen = Convert.ToInt32(uartheaderlength.Text);
            _uartServer._uartdatalen = Convert.ToInt32(uartdatalength.Text);

            if ((bool)uartconnect.IsChecked)
            {
                try
                {
                    _uartServer.OpenPort((string)uartport.SelectedItem, Convert.ToInt32(uartbaudrate.SelectedItem), 8);
                }
                catch (Exception ex)
                {
                    _messageServer.AddWindowsMsg(ex.Message);
                    uartconnect.IsChecked = false;
                    return;
                }

            }
            else
            {               
                _uartServer.ClosePort();
            }
        }

        private void Uartsavedata_Click(object sender, RoutedEventArgs e)
        {
            if (_uartServer._uartfilepath == "")
            {
                var mDialog = new FolderBrowserDialog();
                DialogResult result = mDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                _uartServer._uartfilepath = mDialog.SelectedPath.Trim();
            }
            _uartServer._uartsavecnt++;
            _uartServer._uartframecnt = int.Parse(uartframecnt.Text.Trim());
            var filename = DateTime.Now.ToString("MM-dd第") + _uartServer._uartsavecnt.ToString() + "次uart数据";
            string path = _uartServer._uartfilepath + "\\" + filename;
            _uartServer._uartfilestream = new FileStream(path, FileMode.Create, FileAccess.Write);
            _uartServer._uartsw = new BinaryWriter(_uartServer._uartfilestream);

            _uartServer._uartheardflag = TextToByteArry(uartheardflag.Text);
            _uartServer._uartheardlen = Convert.ToInt32(uartheaderlength.Text);
            _uartServer._uartdatalen = Convert.ToInt32(uartdatalength.Text);

            uartsavedata.IsEnabled = false;
            uartchangepath.IsEnabled = false;

            _uartServer._uartdatastate = PlotformUart.Datastate.start;
        }

        private void Uartchangepath_Click(object sender, RoutedEventArgs e)
        {
            var mDialog = new FolderBrowserDialog();
            DialogResult result = mDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;
            _uartServer._uartfilepath = mDialog.SelectedPath.Trim();
        }

        private void Uartstartrecv_Click(object sender, RoutedEventArgs e)
        {
            _uartServer._uartheardflag = TextToByteArry(uartheardflag.Text);
            _uartServer._uartheardlen = Convert.ToInt32(uartheaderlength.Text);
            _uartServer._uartdatalen = Convert.ToInt32(uartdatalength.Text);

            uartheardflag.IsEnabled = false;
            uartdatalength.IsEnabled = false;
            uartheaderlength.IsEnabled = false;
            uartstartcom.IsEnabled = false;
            uartstopcom.IsEnabled = false;

            if (uartstartcom.Text != string.Empty)
            {
                if (!_uartServer.SendData(TextToByteArry(uartstartcom.Text)))
                {
                    uartheardflag.IsEnabled = true;
                    uartdatalength.IsEnabled = true;
                    uartheaderlength.IsEnabled = true;
                    uartstartcom.IsEnabled = true;
                    uartstopcom.IsEnabled = true;
                }
            }
        }

        private void Uartstoprecv_Click(object sender, RoutedEventArgs e)
        {
            uartheardflag.IsEnabled = true;
            uartdatalength.IsEnabled = true;
            uartheaderlength.IsEnabled = true;
            uartstartcom.IsEnabled = true;
            uartstopcom.IsEnabled = true;

            if (uartstopcom.Text != string.Empty)
                _uartServer.SendData(TextToByteArry(uartstopcom.Text));
        }
    }
}
