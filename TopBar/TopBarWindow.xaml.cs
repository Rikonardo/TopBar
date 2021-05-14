using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace TopBar
{
    public partial class TopBarWindow : Window
    {
        // TopBar version
        public static Version Version = Assembly.GetEntryAssembly().GetName().Version;
        // Store handle of current window
        IntPtr handle;
        // Is TopBar hidden
        bool isHidden = false;
        // List of objects, protected from garbage collector
        public List<object> Uncollectable = new List<object>();
        // List of active widgets
        public List<iWidgetBase> Widgets = new List<iWidgetBase>();
        // List of loaded extentions
        public List<Assembly> Extentions = new List<Assembly>();
        // Config
        public dynamic Config;
        // Is blur enabled
        private bool isBlured = false;
        // Protection against accidental closure
        private bool allowClosing = false;
        public TopBarWindow()
        {
            // Prevent launching multiple instances
            Process current = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.Equals(current.ProcessName) && p.StartInfo.FileName.Equals(current.StartInfo.FileName) && !p.Id.Equals(current.Id)) p.Kill();
            }
            // Enable visual styles for WinForms controls
            System.Windows.Forms.Application.EnableVisualStyles();
            // Initialize WPF components
            InitializeComponent();
            // Hide window from Alt+Tab
            this.HideWindow();
            // Saving handle
            this.handle = new WindowInteropHelper(this).Handle;
            // Subscribe events
            this.SetupEvents();
            // Setup timer
            Timer timer = new Timer();
            timer.Tick += Tick;
            timer.Interval = 500;
            timer.Start();
            // Reset before seutp
            this.Reset();
            // Run seutp
            this.Build();
        }

        private void HideWindow()
        {
            // Create helper window
            Window w = new Window();
            // Location of new window is outside of visible part of screen
            w.Top = -100;
            w.Left = -100;
            // Size of window is enough small to avoid its appearance at the beginning
            w.Width = 1;
            w.Height = 1;

            // Set window style as ToolWindow to avoid its icon in AltTab
            w.WindowStyle = WindowStyle.ToolWindow;
            w.ShowInTaskbar = false;
            w.Show();
            this.Owner = w;
            w.Hide();
            // Close helper window when main window is closed
            this.Closed += delegate { w.Close(); };
        }

        private void UpdateCurrentWindowTitle()
        {
            string title = WinApi.GetActiveWindowTitle();
            this.WindowName.Text = title.Length == 0 ? "Desktop" : title;
        }

        private void SetupEvents()
        {
            this.Closing += delegate (object sender, CancelEventArgs e) {
                if (this.allowClosing)
                {
                    this.Reset();
                }
                else e.Cancel = true;
            };
            this.StateChanged += delegate { this.WindowState = WindowState.Normal; };
            WinApi.WinEventDelegate windowSwitched = new WinApi.WinEventDelegate(
                delegate (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
                    this.UpdateCurrentWindowTitle();
                }
            );
            Uncollectable.Add(windowSwitched);
            WinApi.SetWinEventHook(3, 3, IntPtr.Zero, windowSwitched, 0, 0, 0);
        }

        private void Build()
        {
            try
            {
                // Setup window
                this.WindowState = WindowState.Normal;
                this.Left = Screen.PrimaryScreen.WorkingArea.Left;
                this.Top = Screen.PrimaryScreen.WorkingArea.Top;
                this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                this.Height = 18;
                // Use Windows API to create margin for bar
                WinApi.RECT rect = new WinApi.RECT();
                rect.left = Screen.PrimaryScreen.WorkingArea.Left;
                rect.top = Screen.PrimaryScreen.WorkingArea.Top;
                rect.right = Screen.PrimaryScreen.WorkingArea.Right;
                rect.bottom = Screen.PrimaryScreen.WorkingArea.Bottom;
                WinApi.APPBARDATA appbardata = new WinApi.APPBARDATA();
                appbardata.rc = rect;
                appbardata.cbSize = Marshal.SizeOf((object)appbardata);
                appbardata.hWnd = this.handle;
                appbardata.uEdge = 1;
                WinApi.SHAppBarMessage(0, ref appbardata);
                WinApi.SHAppBarMessage(2, ref appbardata);
                appbardata.rc.bottom = checked(appbardata.rc.top + Convert.ToInt32(18));
                WinApi.SHAppBarMessage(3, ref appbardata);
                // Get config
                dynamic cfg = null;
                try
                {
                    cfg = this.ReadConfig();
                }
                catch (Exception err)
                {
                    System.Windows.MessageBox.Show(err.Message, "Failed to load config!");
                    throw err;
                }
                // Load extentions
                foreach (string dll in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TopBar", "extensions"), "*.dll"))
                    this.Extentions.Add(Assembly.LoadFile(dll));
                // Alert if blur disabled
                // if (this.Config != null && (bool)this.Config.settings["use-blur"] && !(bool)cfg.settings["use-blur"])
                //     System.Windows.MessageBox.Show("You must restart TopBar to disable blur!", "Restart to disable blur");
                // Save config
                this.Config = cfg;
                // Enable blur
                if(!this.isBlured && (bool)this.Config.settings["use-blur"])
                {
                    this.isBlured = true;
                    WindowBlur.SetIsEnabled(this, true);
                }
                // Set backgroud
                this.Background = new SolidColorBrush(Color.FromArgb(
                    (byte) this.Config.settings.color.alpha,
                    (byte) this.Config.settings.color.red,
                    (byte) this.Config.settings.color.green,
                    (byte) this.Config.settings.color.blue
                    ));
                // Setup focused window title styles
                this.WindowName.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString((string) this.Config.settings["current-window"].color));
                // Setup widgets
                try
                {
                    this.SetupWidgets(this.Config);
                }
                catch (Exception err)
                {
                    System.Windows.MessageBox.Show(err.ToString(), "Failed to setup widgets!");
                    throw new Exception("");
                }
            }
            catch (Exception err) {
                if (err.Message.Length > 0)
                    System.Windows.MessageBox.Show(err.ToString(), "Failed to load TopBar!");
            }
        }

        private dynamic ReadConfig()
        {

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appfolder = Path.Combine(appdata, "TopBar");
            if (!Directory.Exists(appfolder)) Directory.CreateDirectory(appfolder);
            string extensions = Path.Combine(appfolder, "extensions");
            if (!Directory.Exists(extensions)) Directory.CreateDirectory(extensions);
            string configfile = Path.Combine(appfolder, "config.json");
            if (!File.Exists(configfile)) File.WriteAllBytes(configfile, Properties.Resources.config);
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(configfile));
            return json;
        }

        private void SetupWidgets(dynamic config)
        {
            switch ((int)config["config-version"])
            {
                case 1:
                    {
                        foreach (dynamic widget in config.widgets)
                        {
                            Type widgetType = Type.GetType((string) widget.type);
                            if(widgetType == null)
                            {
                                foreach (Assembly extention in this.Extentions)
                                {
                                    widgetType = extention.GetType((string)widget.type);
                                    if (widgetType != null) break;
                                }
                            }
                            if(widgetType == null)
                            {
                                System.Windows.MessageBox.Show($"Can't find \"{(string)widget.type}\" widget!", "Error");
                                continue;
                            };
                            iWidgetBase widgetObject = (iWidgetBase) widgetType.GetConstructor(new Type[] { typeof(object) })
                                .Invoke(new object[] { widget.settings == null ? new object() : (object) widget.settings });
                            this.WidgetsPanel.Children.Add(widgetObject.GetComponent());
                            Grid spacer = new Grid();
                            spacer.Width = 4;
                            this.WidgetsPanel.Children.Add(spacer);
                            this.Widgets.Add(widgetObject);
                        }
                        break;
                    }
                default:
                    throw new UnsupportedConfigVersionException((int) config["config-version"]);
            }
        }

        private void Reset()
        {
            WinApi.APPBARDATA appbardata = new WinApi.APPBARDATA();
            appbardata.cbSize = Marshal.SizeOf((object)appbardata);
            appbardata.hWnd = this.handle;
            WinApi.SHAppBarMessage(1, ref appbardata);

            foreach (iWidgetBase widget in Widgets)
            {
                widget.Dispose();
            }
            this.Widgets.Clear();
            this.WidgetsPanel.Children.Clear();

            this.Extentions.Clear();
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UpdateCurrentWindowTitle();
            if (WinApi.IsForegroundFullScreen(Screen.PrimaryScreen))
            {
                if (!isHidden)
                {
                    isHidden = true;
                    this.Hide();
                }
            }
            else
            {
                if (isHidden)
                {
                    isHidden = false;
                    this.Show();
                }
            }
        }

        private void OnMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.ContextMenu cm = this.FindResource("ContextMenu") as System.Windows.Controls.ContextMenu;
            cm.PlacementTarget = sender as System.Windows.Controls.Button;
            cm.IsOpen = true;
        }

        private void ContextMenu_ReloadTB(object sender, RoutedEventArgs e)
        {
            this.allowClosing = true;
            this.Close();
            System.Windows.Forms.Application.Restart();
        }

        private void ContextMenu_ExitTB(object sender, RoutedEventArgs e)
        {
            this.allowClosing = true;
            this.Close();
        }

        private void ContextMenu_ShowConfigTB(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TopBar", "config.json") + "\"");
        }
    }
}
