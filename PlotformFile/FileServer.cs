using PlotformMSG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;

namespace PlotformFile
{
    public enum Datastate { start, stop }
    public class FileServer
    {
        private Dispatcher _uidispatcher;
        private List<Button> _uibuttons;

        private int _savecnt = 0;
        private int _framecnt = 0;
        private string _filepath = "";
        private FileStream _filestream = null;
        private BinaryWriter _sw = null;
        private Datastate _datastate = Datastate.stop;

        public FileServer(Dispatcher dt, List<Button> buttons)
        {
            _uidispatcher = dt;
            _uibuttons = buttons;
        }

        public void SaveData(byte[] data)
        {
            if (_datastate == Datastate.start)
            {
                if (_filepath != string.Empty)
                {
                    _sw.Write(data);
                    _sw.Flush();
                }
                _framecnt--;
                if (_framecnt <= 0)
                {
                    _datastate = Datastate.stop;
                    _sw.Dispose();
                    _filestream.Dispose();
                     
                    _uidispatcher.Invoke(()=>
                    {
                        for (int i = 0; i < _uibuttons.Count; ++i)
                            _uibuttons[i].Enabled = true;
                        
                    });
                }
            }
        }


    }
}
