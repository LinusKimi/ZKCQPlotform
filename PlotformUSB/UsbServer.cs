using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using PlotformMSG;

namespace PlotformUSB
{
    public class UsbServer
    {
        private UsbDevice _usbdevice;
        private UsbDeviceFinder _usbDeviceFinder = new UsbDeviceFinder(0x0483, 0x572B);    // 使用的 stm 默认的参数
        private UsbRegDeviceList _usbRegistries;
        private UsbEndpointReader _usbreader;
        private UsbEndpointWriter _usbwriter;

        private MsgServer _msgserver;

        public UsbServer(MsgServer msgServer)
        {
            _msgserver = msgServer;
        }

        private void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {

        }

        public bool UsbDeviceFinder()
        {

            for (int i = 0; i < _msgserver._usbDeviceList.Count; ++i)
                _msgserver._usbDeviceList.RemoveAt(0);

            _usbRegistries = UsbDevice.AllDevices.FindAll(_usbDeviceFinder);
            if (_usbRegistries.Count == 0)
            {
                _msgserver.AddWindowsMsg("Device Not Found");
                return false;
            }
            else
            {
                foreach (UsbRegistry registry in _usbRegistries)
                    _msgserver.AddUsbDevice(registry.FullName);
                return true;
            }            
        }

        public bool UsbConnect(bool buttonclick, int datalength)
        {
            if (buttonclick)
            {
                _usbdevice = UsbDevice.OpenUsbDevice(_usbDeviceFinder);
                IUsbDevice wholeusbdevice = _usbdevice as IUsbDevice;

                if (!(wholeusbdevice is null))
                {
                    wholeusbdevice.SetConfiguration(1);
                    wholeusbdevice.ClaimInterface(0);
                }

                _usbreader = _usbdevice.OpenEndpointReader(ReadEndpointID.Ep01);
                _usbreader.DataReceived += OnRxEndPointData;
                _usbreader.ReadBufferSize = datalength;
                _usbreader.DataReceivedEnabled = true;

                _usbwriter = _usbdevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                return true;
                
            }
            else
            {
                //if (_usbdevice != null)
                //{
                    _usbreader.DataReceivedEnabled = false;
                    _usbreader.DataReceived -= OnRxEndPointData;

                    if (_usbdevice.IsOpen)
                    {
                        try
                        {
                            IUsbDevice wholeusbdevice = _usbdevice as IUsbDevice;
                            if (!(wholeusbdevice is null))
                            {
                                wholeusbdevice.ReleaseInterface(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }

                    _usbdevice = null;
                    UsbDevice.Exit();
                    _msgserver.AddWindowsMsg("采集设备需要重新上电");
                    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
        }

        public bool UsbStartComm(byte[] sendcomm)
        {

            return true;
        }
    }
}
