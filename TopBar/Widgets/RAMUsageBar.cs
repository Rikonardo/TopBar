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
    class RAMUsageBar : iWidgetBase
    {
        private int Width = 60;
        private int Height = 12;
        private int Radius = 2;
        private string MainColor = "#BBBBBB";
        private string BackgroudColor = "#BBBBBB66";
        private Grid grid;
        private Rectangle innerRect;
        private Timer timer;

        public RAMUsageBar(dynamic settings)
        {
            if (settings["color"] != null) this.MainColor = (string)settings["color"];
            if (settings["background-color"] != null) this.BackgroudColor = (string)settings["background-color"];
            if (settings["width"] != null) this.Width = (int)settings["width"];
            if (settings["height"] != null) this.Height = (int)settings["height"];
            if (settings["radius"] != null) this.Radius = (int)settings["radius"];
            this.grid = new Grid();
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush((Color) ColorConverter.ConvertFromString(this.BackgroudColor.ToUpper()));
            rect.RadiusX = Radius;
            rect.RadiusY = Radius;
            this.grid.Children.Add(rect);
            this.grid.Width = this.Width;
            this.grid.Margin = new Thickness(0, (18 - this.Height) / 2, 0, (18 - this.Height) / 2);
            this.innerRect = new Rectangle();
            this.innerRect.HorizontalAlignment = HorizontalAlignment.Left;
            if (!this.MainColor.Equals("from-percentage")) innerRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(this.MainColor.ToUpper()));
            this.innerRect.RadiusX = Radius;
            this.innerRect.RadiusY = Radius;
            this.innerRect.Width = 0;
            this.grid.Children.Add(this.innerRect);

            this.timer = new Timer();
            this.timer.Interval = 1000;
            this.timer.Elapsed += delegate {
                PerfomanceInfoData data = PsApiWrapper.GetPerformanceInfo();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    double percentage = ((double)1 - ((double)data.PhysicalAvailableBytes / (double)data.PhysicalTotalBytes));
                    if (this.MainColor.Equals("from-percentage")) this.innerRect.Fill = new SolidColorBrush(this.PercentColor(100 - percentage * 100));
                    this.innerRect.Width = percentage * (double)this.Width;
                }));
            };
            this.timer.Start();
        }

        public void Dispose()
        {
            this.timer.Stop();
            this.timer.Dispose();
        }

        public FrameworkElement GetComponent()
        {
            return this.grid;
        }

        private Color PercentColor(double pct)
        {
            double red = (pct <= 50) ? 210 : 211 - (pct - 50) * 5.12;
            double green = (pct >= 50) ? 210 : pct * 5.12;
            return Color.FromRgb((byte)red, (byte)green, 100);
        }

        private class PerfomanceInfoData
        {
            public Int64 CommitTotalPages;
            public Int64 CommitLimitPages;
            public Int64 CommitPeakPages;
            public Int64 PhysicalTotalBytes;
            public Int64 PhysicalAvailableBytes;
            public Int64 SystemCacheBytes;
            public Int64 KernelTotalBytes;
            public Int64 KernelPagedBytes;
            public Int64 KernelNonPagedBytes;
            public Int64 PageSizeBytes;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        private static class PsApiWrapper
        {
            [DllImport("psapi.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetPerformanceInfo([Out] out PsApiPerformanceInformation PerformanceInformation, [In] int Size);

            [StructLayout(LayoutKind.Sequential)]
            public struct PsApiPerformanceInformation
            {
                public int Size;
                public IntPtr CommitTotal;
                public IntPtr CommitLimit;
                public IntPtr CommitPeak;
                public IntPtr PhysicalTotal;
                public IntPtr PhysicalAvailable;
                public IntPtr SystemCache;
                public IntPtr KernelTotal;
                public IntPtr KernelPaged;
                public IntPtr KernelNonPaged;
                public IntPtr PageSize;
                public int HandlesCount;
                public int ProcessCount;
                public int ThreadCount;
            }

            public static PerfomanceInfoData GetPerformanceInfo()
            {
                PerfomanceInfoData data = new PerfomanceInfoData();
                PsApiPerformanceInformation perfInfo = new PsApiPerformanceInformation();
                if (GetPerformanceInfo(out perfInfo, Marshal.SizeOf(perfInfo)))
                {
                    /// data in pages
                    data.CommitTotalPages = perfInfo.CommitTotal.ToInt64();
                    data.CommitLimitPages = perfInfo.CommitLimit.ToInt64();
                    data.CommitPeakPages = perfInfo.CommitPeak.ToInt64();

                    /// data in bytes
                    Int64 pageSize = perfInfo.PageSize.ToInt64();
                    data.PhysicalTotalBytes = perfInfo.PhysicalTotal.ToInt64() * pageSize;
                    data.PhysicalAvailableBytes = perfInfo.PhysicalAvailable.ToInt64() * pageSize;
                    data.SystemCacheBytes = perfInfo.SystemCache.ToInt64() * pageSize;
                    data.KernelTotalBytes = perfInfo.KernelTotal.ToInt64() * pageSize;
                    data.KernelPagedBytes = perfInfo.KernelPaged.ToInt64() * pageSize;
                    data.KernelNonPagedBytes = perfInfo.KernelNonPaged.ToInt64() * pageSize;
                    data.PageSizeBytes = pageSize;

                    /// counters
                    data.HandlesCount = perfInfo.HandlesCount;
                    data.ProcessCount = perfInfo.ProcessCount;
                    data.ThreadCount = perfInfo.ThreadCount;
                }
                return data;
            }
        }

        private class ColorUtils
        {
            public struct RGB
            {
                private byte _r;
                private byte _g;
                private byte _b;

                public RGB(byte r, byte g, byte b)
                {
                    this._r = r;
                    this._g = g;
                    this._b = b;
                }

                public byte R
                {
                    get { return this._r; }
                    set { this._r = value; }
                }

                public byte G
                {
                    get { return this._g; }
                    set { this._g = value; }
                }

                public byte B
                {
                    get { return this._b; }
                    set { this._b = value; }
                }

                public bool Equals(RGB rgb)
                {
                    return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
                }
            }

            public struct HSL
            {
                private int _h;
                private float _s;
                private float _l;

                public HSL(int h, float s, float l)
                {
                    this._h = h;
                    this._s = s;
                    this._l = l;
                }

                public int H
                {
                    get { return this._h; }
                    set { this._h = value; }
                }

                public float S
                {
                    get { return this._s; }
                    set { this._s = value; }
                }

                public float L
                {
                    get { return this._l; }
                    set { this._l = value; }
                }

                public bool Equals(HSL hsl)
                {
                    return (this.H == hsl.H) && (this.S == hsl.S) && (this.L == hsl.L);
                }
            }

            public static RGB HSLToRGB(HSL hsl)
            {
                byte r = 0;
                byte g = 0;
                byte b = 0;

                if (hsl.S == 0)
                {
                    r = g = b = (byte)(hsl.L * 255);
                }
                else
                {
                    float v1, v2;
                    float hue = (float)hsl.H / 360;

                    v2 = (hsl.L < 0.5) ? (hsl.L * (1 + hsl.S)) : ((hsl.L + hsl.S) - (hsl.L * hsl.S));
                    v1 = 2 * hsl.L - v2;

                    r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                    g = (byte)(255 * HueToRGB(v1, v2, hue));
                    b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
                }

                return new RGB(r, g, b);
            }

            private static float HueToRGB(float v1, float v2, float vH)
            {
                if (vH < 0)
                    vH += 1;

                if (vH > 1)
                    vH -= 1;

                if ((6 * vH) < 1)
                    return (v1 + (v2 - v1) * 6 * vH);

                if ((2 * vH) < 1)
                    return v2;

                if ((3 * vH) < 2)
                    return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

                return v1;
            }
        }
    }
}
