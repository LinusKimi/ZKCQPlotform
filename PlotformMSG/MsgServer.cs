using System;
using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace PlotformMSG
{
    public class MsgServer
    {
        private readonly Dispatcher _dispatcher;
        public BindingList<string> _usbBindList { get; set; }
        public BindingList<string> _netBindList { get; set; }
        public BindingList<string> _uartBindList { get; set; }
        public BindingList<string> _usbDeviceList { get; set; }


        public MsgServer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _usbBindList = new BindingList<string>();
            _netBindList = new BindingList<string>();
            _uartBindList = new BindingList<string>();
            _usbDeviceList = new BindingList<string>();
        }

        public void AddUsbMsg(string msg)
        {
            _dispatcher.Invoke(() => 
            {
                if (_usbBindList.Count > 50)
                    _usbBindList.RemoveAt(0);
                _usbBindList.Add(msg);
            });
            
        }

        public void AddUsbDevice(string dev)
        {
            _dispatcher.Invoke(() =>
            {
                _usbDeviceList.Add(dev);
            });
            
        }

        public void AddNetMsg(string msg)
        {
            _dispatcher.Invoke(()=>
            {
                if (_netBindList.Count > 50)
                    _netBindList.RemoveAt(0);
                _netBindList.Add(msg);
            });
            
        }

        public void AddUartMsg(string msg)
        {
            _dispatcher.Invoke(()=>
            {
                if (_uartBindList.Count > 50)
                    _uartBindList.RemoveAt(0);
                _uartBindList.Add(msg);
            });
        }

        public void AddWindowsMsg(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }

    }
}
