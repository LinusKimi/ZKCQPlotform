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
        public BindingList<string> _netDeviceList { get; set; }

        public MsgServer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _usbBindList = new BindingList<string>();
            _netBindList = new BindingList<string>();
            _uartBindList = new BindingList<string>();
            _usbDeviceList = new BindingList<string>();
            _netDeviceList = new BindingList<string>();           
        }

        public void AddMsg(BindingList<string> ts, string msg)
        {
            _dispatcher.Invoke(()=> 
            {
                if (ts.Count > 50)
                    ts.RemoveAt(0);
                ts.Add(msg);
            });

        }

        public void AddDevice(BindingList<string> ts, string dev)
        {
            _dispatcher.Invoke(()=>
            {
                ts.Add(dev);
            });
        }

        public void AddWindowsMsg(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }

    }
}
