using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace WindowsService2
{
    public partial class IrSvc : ServiceBase
    {
        public IrSvc()
        {
            InitializeComponent();
        }
        #region 设置
        string logDictionary = @"d:\WinodwsSchematics\IrPrcService\LOST.DIR\Dumps\crashed\00001";
        string filePath = @"";
        #endregion


        FileStream fs;
        StreamWriter sw;
        List<string> processList=new List<string>();
        List<string> taskbarList=new List<string>();
        IntPtr tskbarFormHandle = IntPtr.Zero;

        private System.Timers.Timer m_mainTimer;



        protected override void OnStart(string[] args)
        {
            if (!Directory.Exists(logDictionary))
            {
                Directory.CreateDirectory(logDictionary);
            }
            filePath =  $"{logDictionary}\\{DateTime.Now.ToFileTime()}_schematic.dump";
            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            sw = new StreamWriter(fs);

            AppendLog("Service Started");

            m_mainTimer = new System.Timers.Timer();
            m_mainTimer.Interval = 10000;   // every one min
            m_mainTimer.Elapsed += m_mainTimer_Elapsed;
            m_mainTimer.AutoReset = true;  // makes it fire only once
            m_mainTimer.Start(); // Start



            AppendLog("Starting Scanning");

        }

        protected override void OnStop()
        {
            m_mainTimer.Stop();
            m_mainTimer.Dispose();
            m_mainTimer = null;
            AppendLog("Service Stopped");
            sw.Close();
            fs.Close();
        }

        #region HandleDLL
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

        public List<string> GetRunApplicationList(IntPtr formHandle)
        {
            List<string> appString = new List<string>();
            try
            {
                int handle = (int)formHandle;
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
                AppendLog($"Error occurred at:{ex.Message}");
            }
            return appString;
        }

        private static string[] GetWindowsInfo()
        {
            try
            {
                Process[] MyProcesses = Process.GetProcesses();
                string[] Minfo = new string[6];
                foreach (Process MyProcess in MyProcesses)
                {
                    if (MyProcess.MainWindowTitle.Length > 0)
                    {
                        Minfo[0] = MyProcess.MainWindowTitle;
                        Minfo[1] = MyProcess.Id.ToString();
                        Minfo[2] = MyProcess.ProcessName;
                        Minfo[3] = MyProcess.StartTime.ToString();
                    }
                }
                return Minfo;
            }
            catch { return null; }
        }
        #endregion

        private void AppendLog(string content)
        {
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine($"[{DateTime.Now.ToString()}] {content}\n");
            sw.Flush();
        }

        /// <summary>
        /// 时间间隔10000ms
        /// 轮询进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        void m_mainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                AppendLog($"Ticked");

                StringBuilder sb = new StringBuilder();

                foreach (Process p in Process.GetProcesses())//GetProcessesByName(strProcessesByName))
                {
                    if (!processList.Contains(p.ProcessName))
                    {
                        processList.Add(p.ProcessName);
                        AppendLog($"NewThread:(title:{p.MainWindowTitle},name:{p.ProcessName},id:{p.Id},handle:{p.Handle},startAt{p.StartTime})");
                    }
                }



                AppendLog(sb.ToString());
                sb.Clear();
            }
            catch (Exception ex)
            {

            }

        }




    }
}
