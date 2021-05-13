using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TopBar.Widgets
{
    class Clock : iWidgetBase
    {
        private bool ShowDate = false;
        private bool ShowSeconds = false;
        private string TextColor = "#BBBBBB";
        private TextBlock textBlock;
        private Timer timer;

        public Clock(dynamic settings)
        {
            if (settings["date"] != null) this.ShowDate = (bool) settings["date"];
            if (settings["seconds"] != null) this.ShowSeconds = (bool)settings["seconds"];
            if (settings["text-color"] != null) this.TextColor = (string)settings["text-color"];
            this.textBlock = new TextBlock();
            this.textBlock.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString(this.TextColor.ToUpper()));
            this.UpdateTime();
            this.timer = new Timer();
            this.timer.Interval = 1000;
            this.timer.Elapsed += delegate {
                this.UpdateTime();
            };
            this.timer.Start();
        }

        private void UpdateTime()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                this.textBlock.Text = DateTime.Now.ToString(this.ShowDate ? (this.ShowSeconds ? "G" : "g") : (this.ShowSeconds ? "T" : "t"));
            }));
        }

        public void Dispose()
        {
            this.timer.Stop();
            this.timer.Dispose();
        }

        public FrameworkElement GetComponent()
        {
            return this.textBlock;
        }
    }
}
