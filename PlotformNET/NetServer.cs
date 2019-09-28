using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;
using HPSocketCS;
using PlotformMSG;

namespace PlotformNET
{
    public enum Datastate { start, stop }
    public class NetServer
    {
        private TcpPackServer _tcppackserver;
        private IntPtr _connectId;

        private MsgServer _msgserver;
        private ActionBlock<byte[]> _actionBlock;

        public int _netsavecnt = 0;
        public int _netframecnt = 0;
        public string _netfilepath = "";
        public FileStream _netfilestream = null;
        public BinaryWriter _netsw = null;
        public Datastate _netdatastate = Datastate.stop;

        public NetServer(MsgServer msgserver, ActionBlock<byte[]> actionBlock)
        {
            _tcppackserver = new TcpPackServer();

            _msgserver = msgserver;
            _actionBlock = actionBlock;

            _tcppackserver.OnPrepareListen += _tcppackserver_OnPrepareListen;
            _tcppackserver.OnAccept += _tcppackserver_OnAccept;
            _tcppackserver.OnSend += _tcppackserver_OnSend;
            _tcppackserver.OnReceive += _tcppackserver_OnReceive;
            _tcppackserver.OnClose += _tcppackserver_OnClose;
            _tcppackserver.OnShutdown += _tcppackserver_OnShutdown;

        }

        private HandleResult _tcppackserver_OnShutdown(IServer sender)
        {
            return HandleResult.Ok;
        }

        private HandleResult _tcppackserver_OnClose(IServer sender, IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            return HandleResult.Ok;
        }

        private HandleResult _tcppackserver_OnReceive(IServer sender, IntPtr connId, byte[] bytes)
        {
            var clientInfo = _tcppackserver.GetExtra<ClientInfo>(connId);
            if (clientInfo != null)
            {
                _msgserver.AddMsg(_msgserver._netBindList,
                    $" > [{clientInfo.ConnId},OnReceive] -> {clientInfo.IpAddress}:{clientInfo.Port} ({bytes.Length} bytes)");
            }
            else
            {
                _msgserver.AddMsg(_msgserver._netBindList, $" > [{connId},OnReceive] -> ({bytes.Length} bytes)");
                return HandleResult.Error;
            }

            _actionBlock.Post(bytes);

            return HandleResult.Ok;
        }

        private HandleResult _tcppackserver_OnSend(IServer sender, IntPtr connId, byte[] bytes)
        {
            return HandleResult.Ok;
        }

        private HandleResult _tcppackserver_OnAccept(IServer sender, IntPtr connId, IntPtr pClient)
        {
            string ip = string.Empty;
            ushort port = 0;

            _msgserver.AddDevice(_msgserver._netDeviceList, _tcppackserver.GetRemoteAddress(connId, ref ip, ref port)
                ? $" > [{connId}, OnAccept] -> PASS({ip}:{port})"
                : $" > [{connId}, OnAccept] -> Server_GetClientAddress() Error");

            _connectId = connId;

            var clientinfo = new ClientInfo
            {
                ConnId = connId,
                IpAddress = ip,
                Port = port
            };

            if (_tcppackserver.SetExtra(connId, clientinfo) == false)
                _msgserver.AddWindowsMsg($" > [{connId},OnAccept] -> SetConnectionExtra fail");

            return HandleResult.Ok;
        }

        private HandleResult _tcppackserver_OnPrepareListen(IServer sender, IntPtr soListen)
        {
            return HandleResult.Ok;
        }

        public bool Start(string ip, ushort port)
        {
            _tcppackserver.IpAddress = ip;
            _tcppackserver.Port = port;
            _tcppackserver.PackHeaderFlag = 0xFF;
            _tcppackserver.MaxPackSize = 0x2FFFF;

            return _tcppackserver.Start();
        }

        public bool Stop() => _tcppackserver.Stop();

        public void Destroy() => _tcppackserver.Destroy();

        public bool NetSend(byte[] data, int size) => _tcppackserver.Send(_connectId, data, data.Length);


        [StructLayout(LayoutKind.Sequential)]
        public class ClientInfo
        {
            public IntPtr ConnId { get; set; }
            public string IpAddress { get; set; }
            public ushort Port { get; set; }
        }
    }
}
