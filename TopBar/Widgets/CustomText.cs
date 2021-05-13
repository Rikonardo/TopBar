using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TopBar.Widgets
{
    class CustomText : iWidgetBase
    {
        private string MainColor = "#BBBBBB";
        private string Text = "";
        private TextBlock textBlock;

        public CustomText(dynamic settings)
        {
            if (settings["color"] != null) this.MainColor = (string)settings["color"];
            if (settings["text"] != null) this.Text = (string)settings["text"];
            this.textBlock = new TextBlock();
            this.textBlock.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString(this.MainColor));
            this.textBlock.Text = this.Text;
        }

        public void Dispose()
        {

        }

        public FrameworkElement GetComponent()
        {
            return this.textBlock;
        }
    }
}
