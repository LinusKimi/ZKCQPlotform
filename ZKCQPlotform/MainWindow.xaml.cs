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

namespace ZKCQPlotform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bytecnt.Text = Properties.Settings.Default.usbbytecnt;
            framecnt.Text = Properties.Settings.Default.usbframecnt;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.usbbytecnt = bytecnt.Text;
            Properties.Settings.Default.usbframecnt = framecnt.Text;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Searchdevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Usbconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Usbsavedata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Usbfilepath_Click(object sender, RoutedEventArgs e)
        {

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
