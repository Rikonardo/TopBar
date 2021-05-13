using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TopBar.Widgets
{
    class Shortcut : iWidgetBase
    {
        private string Command = "explorer.exe";
        private string ToolTip = "explorer.exe";
        private string Icon = "";
        private Grid grid;

        public Shortcut(dynamic settings)
        {
            if (settings["command"] != null) this.Command = (string)settings["command"];
            this.ToolTip = this.Command;
            if (settings["title"] != null) this.ToolTip = (string)settings["title"];
            if (settings["icon"] != null) this.Icon = (string)settings["icon"];
            this.grid = new Grid();
            this.grid.Width = 16;
            this.grid.Margin = new Thickness(0, 1, 0, 1);
            this.grid.ToolTip = ToolTip;
            Image image = new Image();
            if (Icon.Length > 0)
                image.Source = new BitmapImage(new Uri(Icon));
            else
            {
                System.Drawing.Icon icon = this.IconFromFilePath(Command);
                if (icon != null)
                    image.Source = this.ToImageSource(icon);
            }
            image.Stretch = Stretch.Fill;
            this.grid.Children.Add(image);
            this.grid.MouseLeftButtonUp += delegate
            {
                System.Diagnostics.Process.Start(Command);
            };
            this.grid.MouseEnter += delegate
            {
                image.Opacity = 0.75;
            };
            this.grid.MouseLeave += delegate
            {
                image.Opacity = 1;
            };
        }

        private System.Drawing.Icon IconFromFilePath(string path)
        {
            System.Drawing.Icon result = null;
            try
            {
                result = System.Drawing.Icon.ExtractAssociatedIcon(path);
            }
            catch { }
            return result;
        }

        public void Dispose()
        {

        }

        public FrameworkElement GetComponent()
        {
            return this.grid;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private ImageSource ToImageSource(System.Drawing.Icon icon)
        {
            System.Drawing.Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }
    }
}
