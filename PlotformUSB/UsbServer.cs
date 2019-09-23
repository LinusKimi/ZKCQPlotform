using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;

using PlotformMSG;

namespace PlotformUSB
{
    public class UsbServer
    {
        private readonly UsbServer _usbServer;
        private readonly UsbDeviceFinder _usbDeviceFinder = new UsbDeviceFinder(0x0483, 0x572B);    // 使用的 stm 默认的参数
        private UsbRegDeviceList _usbRegistries;
        private UsbEndpointReader _usbreader;
        private UsbEndpointWriter _usbwriter;


    }
}
