using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TopBar.Native;

namespace TopBar
{
    public class UnsupportedConfigVersionException : Exception
    {
        public UnsupportedConfigVersionException(int version) : base ($"Config version \"{version}\" is not supported by TopBar ${TopBarWindow.Version.ToString()}")
        {
            
        }
    }
}