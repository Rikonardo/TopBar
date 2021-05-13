using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    class CPULoad : iWidgetBase
    {
        private int LineWidth = 5;
        private int Radius = 2;
        private string MainColor = "#BBBBBB";
        private Timer timer;
        private StackPanel panel;

        public CPULoad(dynamic settings)
        {
            if (settings["color"] != null) this.MainColor = (string)settings["color"];
            if (settings["line-width"] != null) this.LineWidth = (int)settings["line-width"];
            if (settings["radius"] != null) this.Radius = (int)settings["radius"];
            this.panel = new StackPanel();
            this.panel.Orientation = Orientation.Horizontal;
            var pc = new PerformanceCounter("Processor Information", "% Processor Time");
            var cat = new PerformanceCounterCategory("Processor Information");
            var instances = cat.GetInstanceNames();
            var cs = new Dictionary<string, CounterSample>();
            var actual = new Dictionary<string, double>();

            var cores = new Dictionary<int, Rectangle>();

            foreach (var s in instances)
            {
                pc.InstanceName = s;
                cs.Add(s, pc.NextSample());
                if (this.isCore(s))
                {
                    cores.Add(this.coreNum(s), null);
                }
            }

            for (int i = 0; i < cores.Count; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Width = this.LineWidth;
                rect.Height = 0;
                rect.RadiusX = this.Radius;
                rect.RadiusY = this.Radius;
                rect.VerticalAlignment = VerticalAlignment.Bottom;
                rect.Margin = new Thickness(3, 3, 3, 3);
                if (this.MainColor.Equals("rainbow"))
                {
                    ColorUtils.RGB rgb = ColorUtils.HSLToRGB(new ColorUtils.HSL((int) (360 * ((double) i / cores.Count)), 200, 200));
                    rect.Fill = new SolidColorBrush(Color.FromRgb(rgb.R, rgb.G, rgb.B));
                }
                else if (!this.MainColor.Equals("from-percentage"))
                {
                    rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(this.MainColor));
                }
                cores[i] = rect;
                this.panel.Children.Add(rect);
            }

            this.timer = new Timer();
            this.timer.Interval = 1000;
            this.timer.Elapsed += delegate {
                actual.Clear();
                foreach (var s in instances)
                {
                    pc.InstanceName = s;
                    actual.Add(s, Calculate(cs[s], pc.NextSample()));
                    cs[s] = pc.NextSample();
                }
                for (int i = 0; i < cores.Count; i++)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                    {
                        try {
                            if (this.MainColor.Equals("from-percentage")) cores[i].Fill = new SolidColorBrush(this.PercentColor(100 - actual["0," + i]));
                            cores[i].Height = actual["0," + i] / 100 * 12;
                        } catch { }
                    }));
                }
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
            return this.panel;
        }

        private double Calculate(CounterSample oldSample, CounterSample newSample)
        {
            double difference = newSample.RawValue - oldSample.RawValue;
            double timeInterval = newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec;
            if (timeInterval != 0) return 100 * (1 - (difference / timeInterval));
            return 0;
        }

        private bool isCore(string s)
        {
            return !s.EndsWith("Total");
        }

        private int coreNum(string s)
        {
            try
            {
                return int.Parse(s.Split(',')[1]);
            }
            catch {
                return 0;
            }
        }

        private Color PercentColor(double pct)
        {
            double red = (pct <= 50) ? 255 : 256 - (pct - 50) * 5.12;
            double green = (pct >= 50) ? 255 : pct * 5.12;
            return Color.FromRgb((byte)red, (byte)green, 0);
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
