using System;
using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;

namespace PlotformMSG
{
    public class MsgServer
    {
        private readonly Dispatcher _dispatcher;
        public BindingList<string> _usbBindList { get; set; }
        public BindingList<string> _netBindList { get; set; }
        public BindingList<string> _uartBindList { get; set; }

        public MsgServer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        private void addUsbMsg(string msg)
        {
            if (_usbBindList.Count > 50)
                _usbBindList.RemoveAt(0);
            _usbBindList.Add(msg);
        }

        private void addNetMsg(string msg)
        {
            if (_netBindList.Count > 50)
                _netBindList.RemoveAt(0);
            _netBindList.Add(msg);
        }

        private void addUartMsg(string msg)
        {
            if (_uartBindList.Count > 50)
                _uartBindList.RemoveAt(0);
            _uartBindList.Add(msg);
        }
    }
}
