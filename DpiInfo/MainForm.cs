using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DpiInfo {
    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets information for all processes with a main window handle.
        /// </summary>
        /// <returns></returns>
        private String getAllProcessesInfo() {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("---- Display Information ----");
            sb.AppendLine(getDisplayInfo());

            // PreferExternalManifest
            sb.AppendLine("---- PreferExternalManifest ----");
            sb.AppendLine(getPreferExternalManifest());
            sb.AppendLine();

            sb.AppendLine("---- Process Information ----");

            //// Get this process
            //Process currentProcess = Process.GetCurrentProcess();
            //sb.Append(getProcessInfo(currentProcess));
            ////sb.AppendLine("  Bounds: X=" + Bounds.Left + " Y=" + Bounds.Top
            ////   + " Size: " + Bounds.Width + " x " + Bounds.Height);
            //sb.AppendLine();

            // Get all processes running on the local computer.
            Process[] localAll = Process.GetProcesses();

            // Sort them
            Array.Sort(localAll, delegate (Process process1, Process process2) {
                return process1.ProcessName.CompareTo(process2.ProcessName);
            });

            foreach (Process process in localAll) {
                try {
                    try {
                        if (process.MainWindowHandle == IntPtr.Zero) continue;
                    } catch (Exception) {
                        continue;
                    }
                    sb.Append(getProcessInfo(process));
                } catch (Exception ex) {
                    sb.AppendLine("Process: " + process.ProcessName +
                        " (" + process.Id + "): *** Exception: " + ex.Message);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets information for all monitors
        /// </summary>
        /// <returns></returns>
        private string getDisplayInfo() {
            StringBuilder sb = new StringBuilder();
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens) {
                var pnt = new System.Drawing.Point(
                    (screen.Bounds.Left + screen.Bounds.Right) / 2,
                    (screen.Bounds.Top + screen.Bounds.Bottom) / 2);
                var mon = NativeMethods.MonitorFromPoint(pnt,
                    NativeMethods.MONITOR_DEFAULTTONEAREST);
                uint dpiX, dpiY;
                NativeMethods.GetDpiForMonitor(mon,
                    NativeMethods.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
                    out dpiX, out dpiY);

                string primary = screen.Primary ? "  (Primary)" : "";
                sb.AppendLine("Device Name: " + screen.DeviceName + primary);
                sb.AppendLine("Bits per Pixel: " + screen.BitsPerPixel);
                sb.AppendLine("xDPI=" + dpiX + " yDPI=" + dpiY);
                sb.AppendLine("Bounds: " + screen.Bounds);
                sb.AppendLine("Working Area: " + screen.WorkingArea);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets information for a process.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string getProcessInfo(Process process) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Process: " + process.ProcessName);
            sb.AppendLine("  Main Window Title: " + process.MainWindowTitle);
            sb.AppendLine(String.Format("  Handle: {0} Main Window Handle: {1}",
                process.Handle.ToString("X8"),
                process.MainWindowHandle.ToString("X8")));
            sb.AppendLine("  PID: " + process.Id);

            // 32 or 64 bit
            if (!Environment.Is64BitOperatingSystem) {
                sb.AppendLine("  32-bit process");
            } else {
                Boolean isWow64;
                if (!NativeMethods.IsWow64Process(process.Handle, out isWow64)) {
                    throw new Win32Exception();
                }
                if (isWow64) {
                    sb.AppendLine("  32-bit process");
                } else {
                    sb.AppendLine("  64-bit process");
                }
            }

            // Main window size
            NativeMethods.RECT rect;
            if (!NativeMethods.GetWindowRect(new HandleRef(this, process.MainWindowHandle), out rect)) {
                sb.AppendLine("  GetWindowRect failed");
            } else {
                Rectangle rectangle = new Rectangle();
                rectangle.X = rect.Left;
                rectangle.Y = rect.Top;
                rectangle.Width = rect.Right - rect.Left;
                rectangle.Height = rect.Bottom - rect.Top;
                sb.AppendLine("  Window: X=" + rectangle.Left + " Y=" + rectangle.Top
                    + " Size: " + rectangle.Width + " x " + rectangle.Height);
                sb.AppendLine("  Window: Left=" + rect.Left + " Right=" + rect.Right
                    + " Top=" + rect.Top + " Bottom=" + rect.Bottom);
            }

            // Dpi
            int dpi = NativeMethods.GetDpiForWindow(process.MainWindowHandle);
            sb.AppendLine("  DPI: " + dpi);

            // Process DPI awareness
            NativeMethods.PROCESS_DPI_AWARENESS processDpiAwareness;
            int res = NativeMethods.GetProcessDpiAwareness(process.Handle,
                out processDpiAwareness);
            if (res == NativeMethods.S_OK) {
                sb.AppendLine("  Process DPI Awareness: " + processDpiAwareness);
            } else {
                sb.AppendLine("  Process DPI Awareness failed: res=" + res);
            }

            // DPI awareness context
            IntPtr dpiAwarenessContext = NativeMethods.GetWindowDpiAwarenessContext(process.MainWindowHandle);
            if (NativeMethods.AreDpiAwarenessContextsEqual(dpiAwarenessContext, (IntPtr)(-1))) {
                sb.AppendLine("  DPI Awareness Context: (" + dpiAwarenessContext +
                    ") DPI_AWARENESS_CONTEXT_UNAWARE");
            } else if (NativeMethods.AreDpiAwarenessContextsEqual(dpiAwarenessContext, (IntPtr)(-2))) {
                sb.AppendLine("  DPI Awareness Context: (" + dpiAwarenessContext +
                    ") DPI_AWARENESS_CONTEXT_SYSTEM_AWARE");
            } else if (NativeMethods.AreDpiAwarenessContextsEqual(dpiAwarenessContext, (IntPtr)(-3))) {
                sb.AppendLine("  DPI Awareness Context: (" + dpiAwarenessContext +
                    ") DPI_AWARENESS_CONTEXT_SYSTEM_AWARE");
            } else if (NativeMethods.AreDpiAwarenessContextsEqual(dpiAwarenessContext, (IntPtr)(-4))) {
                sb.AppendLine("  DPI Awareness Context: (" + dpiAwarenessContext +
                    ") DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2");
            } else {
                sb.AppendLine("  DPI Awareness Context: " + dpiAwarenessContext);
            }

            // DPI awareness
            NativeMethods.DPI_AWARENESS dpiAwareness =
            (NativeMethods.DPI_AWARENESS)NativeMethods.GetAwarenessFromDpiAwarenessContext(dpiAwarenessContext);
            sb.AppendLine("  DPI Awareness: " + dpiAwareness);

            return sb.ToString();
        }

        /// <summary>
        /// Get the value of HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\SideBySide
        /// </summary>
        /// <returns></returns>
        private string getPreferExternalManifest() {
            const string hive = @"HKEY_LOCAL_MACHINE\";
            const string subKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\SideBySide";
            const string valueName = "PreferExternalManifest";
            try {
                RegistryKey baseKey;
                if (Environment.Is64BitOperatingSystem) {
                    baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                } else {
                    baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey regKey = baseKey.OpenSubKey(subKeyName, false);
                if (regKey == null) {
                    return "Key does not exist: " + hive + subKeyName;
                }
                var value = regKey.GetValue(valueName);
                if (value == null) {
                    return "Name/Value pair does not exist: " + hive + subKeyName + @"\" + valueName;
                }
                return hive + subKeyName + @"\" + valueName + "=" + value;
            } catch (Exception ex) {
                return "Error getting " + hive + subKeyName + @"\" + valueName + "\n"
                    + ex.Message;
            }
        }

        /// <summary>
        /// Refreshes the information.
        /// </summary>
        private void refresh() {
            textBoxInfo.Text = getAllProcessesInfo();
        }

        /// <summary>
        /// Handler for Refresh button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onRefreshClick(object sender, EventArgs e) {
            refresh();
        }

        /// <summary>
        /// Handler for Quit button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnQuitClick(object sender, EventArgs e) {
            Close();
        }

        /// <summary>
        /// Handler for Form activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Activated(object sender, EventArgs e) {
            refresh();
        }

#if false
        private void onTestClick(object sender, EventArgs e) {
        SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Configuration Files|*.config";
            dlg.Title = "Select a Configuration File";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                
            }
        }
#endif
    }

    /// <summary>
    /// Class for native methods.
    /// </summary>
    internal static class NativeMethods {
        internal const int S_OK = 0;

        internal enum PROCESS_DPI_AWARENESS {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        internal enum DPI_AWARENESS {
            DPI_AWARENESS_INVALID = -1,
            DPI_AWARENESS_UNAWARE = 0,
            DPI_AWARENESS_SYSTEM_AWARE = 1,
            DPI_AWARENESS_PER_MONITOR_AWARE = 2
        }

        [DllImport("SHcore.dll")]
        internal static extern int GetProcessDpiAwareness(IntPtr hWnd, out PROCESS_DPI_AWARENESS value);

        [DllImport("user32.dll")]
        internal static extern int GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int GetAwarenessFromDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA,
            IntPtr dpiContextB);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        internal const int MONITOR_DEFAULTTONULL = 0;
        internal const int MONITOR_DEFAULTTOPRIMARY = 1;
        internal const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        internal enum MONITOR_DPI_TYPE {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        [DllImport("Shcore.dll")]
        internal static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor,
            [In]MONITOR_DPI_TYPE dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
    }
}
