using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;
using PlotformMSG;
using System.IO;
using System.IO.Ports;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlotformUart
{
    public enum Datastate { start, stop }
    public class UartServer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SerialPort _uartserport;

        private MsgServer _msgserver;
        private ActionBlock<byte[]> _actionBlock;

        private List<byte> _uartrecvbuf = null;
        private string[] _uartportname;

        public byte[] _uartheardflag = null;
        public int _uartheardlen = 0;
        public int _uartdatalen = 0;

        public int _uartsavecnt = 0;
        public int _uartframecnt = 0;
        public string _uartfilepath = "";
        public FileStream _uartfilestream = null;
        public BinaryWriter _uartsw = null;
        public Datastate _uartdatastate = Datastate.stop;

        public string[] PortNames
        {
            get => _uartportname;
            set
            {
                _uartportname = value;
                NotifyPropertyChanged();
            }
        }

        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }

        public UartServer(MsgServer msgserver, ActionBlock<byte[]> actionBlock)
        {
            _msgserver = msgserver;
            _actionBlock = actionBlock;

            _uartserport = new SerialPort();

            _uartserport.DataReceived += _serialPort_DataReceived;

            PortNames = SerialPort.GetPortNames();
            DataBits = _uartserport.DataBits;
            Parity = _uartserport.Parity;
            StopBits = _uartserport.StopBits;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int length = _uartserport.BytesToRead;
            for (int i = 0; i < length; ++i)
                _uartrecvbuf.Add((byte)_uartserport.ReadByte());

            while (_uartrecvbuf.Count >= 2)
            {
                if (_uartrecvbuf[0] != _uartheardflag[0] || _uartrecvbuf[1] != _uartheardflag[1])
                {
                    _uartrecvbuf.RemoveAt(0);
                    continue;
                }
                if (_uartrecvbuf.Count >= (_uartheardlen + _uartdatalen))
                {
                    byte[] data = new byte[_uartdatalen];

                    for (int i = 0; i < data.Length; ++i)
                        data[i] = _uartrecvbuf[i + _uartheardlen];

                    _msgserver.AddMsg(_msgserver._uartBindList, $"OnReceive -> ({data.Length + 4}) bytes");
                    _actionBlock.Post(data);
                    _uartrecvbuf.RemoveRange(0, _uartheardlen + _uartdatalen);
                }
            }
        }

        public void CheckPort() => PortNames = SerialPort.GetPortNames();

        public void OpenPort(string portname, int baudrate, int rev)
        {
            _uartserport.PortName = portname;
            _uartserport.BaudRate = baudrate;
            _uartserport.ReceivedBytesThreshold = rev;

            _uartserport.DataBits = DataBits;
            _uartserport.Parity = Parity;
            _uartserport.StopBits = StopBits;

            _uartserport.Open();
        }

        public void ClosePort() => _uartserport.Close();

        public void SendData(byte[] data)
        {
            if(_uartserport.IsOpen == false)
                throw new Exception("请打开串口！");

            _uartserport.Write(data, 0, data.Length);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
