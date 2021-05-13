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
    class LineSpacer : iWidgetBase
    {
        private int Width = 1;
        private int Height = 12;
        private string MainColor = "#888888";
        private Grid grid;

        public LineSpacer(dynamic settings)
        {
            if (settings["color"] != null) this.MainColor = (string)settings["color"];
            if (settings["width"] != null) this.Width = (int)settings["width"];
            if (settings["height"] != null) this.Height = (int)settings["height"];
            this.grid = new Grid();
            this.grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(this.MainColor.ToUpper()));
            this.grid.Width = this.Width;
            this.grid.Margin = new Thickness(0, (18 - this.Height) / 2, 0, (18 - this.Height) / 2);
        }

        public void Dispose()
        {

        }

        public FrameworkElement GetComponent()
        {
            return this.grid;
        }
    }
}
