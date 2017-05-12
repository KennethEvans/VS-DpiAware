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
            sb.AppendLine("Process Information");
            sb.AppendLine();

            // Get this process
            Process currentProcess = Process.GetCurrentProcess();
            sb.Append(getProcessInfo(currentProcess));
            sb.AppendLine("  Bounds: X=" + Bounds.Left + " Y=" + Bounds.Top
               + " Size: " + Bounds.Width + " x " + Bounds.Height);
            sb.AppendLine();

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
                    sb.AppendLine(" *** Exception: " + ex.Message);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// get information for a process.
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

            // Dpi awareness
            NativeMethods.PROCESS_DPI_AWARENESS awareness;
            int res = NativeMethods.GetProcessDpiAwareness(process.Handle,
                out awareness);
            if (res == NativeMethods.S_OK) {
                sb.AppendLine("  DPI Awareness: " + awareness);
            } else {
                sb.AppendLine("  DPI Awareness failed: res=" + res);
            }
            return sb.ToString();
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

        [DllImport("Shcore.dll")]
        internal static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

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

    }
}
