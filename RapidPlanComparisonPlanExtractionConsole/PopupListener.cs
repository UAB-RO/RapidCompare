using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using NLog;

namespace RapidPlanComaprisonPlanExtractionConsole
{
    public class PopupListener
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private const int BmClick = 0x00F5;
        private readonly Timer t = new Timer(1000);

        public void Start()
        {
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            var pid = Process.GetCurrentProcess().Id;
            List<WindowInformation> windowListExtended = WinAPI.GetAllWindowsExtendedInfo();
            WindowInformation popup = windowListExtended.Find(
                           w => w.Process.Id == pid && w.Class == "#32770"
                       );
            if (popup != null)
            {
                var msgText = popup.ChildWindows.FirstOrDefault(w => w.Class == "Static");
                string caption = "??? Could not determine popup message!";
                if (msgText.Handle != IntPtr.Zero)
                    caption = msgText.Caption;
                _logger.Warn($"Closing popup: {caption}");
                var okButton = popup.ChildWindows.FirstOrDefault(w => w.Caption.Contains("OK"));
                if (okButton.Handle != IntPtr.Zero)
                {
                    WinAPI.PostMessage(okButton.Handle, BmClick, 0, 0);
                    WinAPI.PostMessage(okButton.Handle, BmClick, 0, 0);
                }
            }
        }

        public void Stop()
        {
            t.Elapsed -= t_Elapsed;
            t.Stop();
        }
    }
}