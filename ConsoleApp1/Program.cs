using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AntiHytServiceStarted!");
          
            
            Thread.Sleep(3000);
        }

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);
        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        public static List<string> GetRunApplicationList(Form form)
        {
            List<string> appString = new List<string>();
            try
            {
                int handle = (int)form.Handle;
                int hwCurr;
                hwCurr = GetWindow(handle, GW_HWNDFIRST);
                while (hwCurr > 0)
                {
                    int isTask = (WS_VISIBLE | WS_BORDER);
                    int lngStyle = GetWindowLongA(hwCurr, GWL_STYLE);
                    bool taskWindow = ((lngStyle & isTask) == isTask);
                    if (taskWindow)
                    {
                        int length = GetWindowTextLength(new IntPtr(hwCurr));
                        StringBuilder sb = new StringBuilder(2 * length + 1);
                        GetWindowText(hwCurr, sb, sb.Capacity);
                        string strTitle = sb.ToString();
                        if (!string.IsNullOrEmpty(strTitle))
                        {
                            appString.Add(strTitle);
                        }
                    }
                    hwCurr = GetWindow(hwCurr, GW_HWNDNEXT);
                }
                return appString;
            }
            catch (Exception ex)
            {

            }
            return appString;
        }

    }
}
