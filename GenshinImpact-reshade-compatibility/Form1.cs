using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using GenshinImpact_reshade_compatibility.Classes;
using GenshinImpact_reshade_compatibility.Classes.miHoYo;
using GenshinImpact_reshade_compatibility.Utils;
using System.Diagnostics;

namespace GenshinImpact_reshade_compatibility
{
    public partial class Form1 : Form
    {
        private ProcessWatcher procWatcher;
        private string tmpFilename;
        private bool hideToTray;
        private Lazy<NotifyIcon> trayIcon;
        private TaskCompletionSource<bool> cancelSrc;

        public Form1()
        {
            this.hideToTray = false;
            InitializeComponent();
            this.trayIcon = new Lazy<NotifyIcon>(() =>
            {
                var result = new NotifyIcon();
                result.Icon = this.Icon;
                result.Text = this.Text;
                result.Visible = false;
                result.BalloonTipText = "Click to show the dialog";
                result.BalloonTipIcon = ToolTipIcon.Info;
                result.BalloonTipTitle = this.Text;
                result.Click += this.TrayIcon_Click;
                return result;
            });
            this.tmpFilename = null;
            this.procWatcher = new ProcessWatcher("GenshinImpact.exe", TimeSpan.FromMilliseconds(500));
            this.procWatcher.ProcessStarted += this.ProcWatcher_ProcessStarted;
        }

        private async void ProcWatcher_ProcessStarted(ProcessStartedInfoArgs procInfo)
        {
            var procPath = procInfo.ProcessPath;
            var procDir = Path.GetDirectoryName(procPath);
            if (GenshinImpact.IsGenshinImpactGameDirectory(procDir))
            {
                string reShadeFile = Path.Combine(procDir, "dxgi.dll");
                if (!File.Exists(reShadeFile))
                {
                    reShadeFile = Path.Combine(procDir, "GenshinImpact_Data", "Plugins", "dxgi.dll");
                    if (!File.Exists(reShadeFile))
                    {
                        Process proc = null;
                        try
                        {
                            proc = Process.GetProcessById((int)procInfo.ProcessID);
                            this.OnProcessFound(procDir);
                            this.OnReShadeStatusChange("Found game directory but cannot find ReShade installation.");
                            await Task.Factory.StartNew((obj) =>
                            {
                                var theProcess = (Process)obj;
                                theProcess.WaitForExit();
                            }, proc, TaskCreationOptions.LongRunning);
                        }
                        catch { }
                        finally
                        {
                            if (proc != null)
                            {
                                proc.Dispose();
                            }
                            this.OnProcessFound("Waiting for game to be started");
                            this.OnReShadeStatusChange("Unknown");
                        }
                        return;
                    }
                }

                List<Process> procUsingFile;
                int retryTimes = 0;
                while (true)
                {
                    if (retryTimes > 10000) return;
                    procUsingFile = FileUtil.WhoIsLocking(reShadeFile);
                    if (procUsingFile != null && procUsingFile.Count != 0)
                    {
                        break;
                    }
                    retryTimes++;
                    await Task.Delay(50);
                }
                Process targetProc = null;
                foreach (var proc in procUsingFile)
                {
                    if (targetProc == null)
                    {
                        if (string.Equals(ProcessHelper.GetProcessFilename((uint)proc.Id), procPath, StringComparison.OrdinalIgnoreCase))
                        {
                            targetProc = proc;
                        }
                        else
                        {
                            proc.Dispose();
                        }
                    }
                    else
                    {
                        proc.Dispose();
                    }
                }
                if (targetProc != null)
                {
                    this.procWatcher.StopListen();
                    var currentDllDir = Path.GetDirectoryName(reShadeFile);
                    var renameddxgi = Path.Combine(currentDllDir, Path.GetRandomFileName());
                    while (renameddxgi.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || File.Exists(renameddxgi))
                    {
                        renameddxgi = Path.Combine(currentDllDir, Path.GetRandomFileName());
                    }
                    this.tmpFilename = Path.GetFileName(renameddxgi);
                    File.Move(reShadeFile, renameddxgi);
                    this.OnReShadeStatusChange($"Reshade renamed to: {this.tmpFilename}");
                    this.OnProcessFound(procDir);
                    this.cancelSrc = new TaskCompletionSource<bool>();
                    this.SetCancelButtonState(true);
                    try
                    {
                        await Task.WhenAny(Task.Factory.StartNew((obj) =>
                        {
                            var proc = (Process)obj;
                            proc.WaitForExit();
                        }, targetProc, TaskCreationOptions.LongRunning), this.cancelSrc.Task);
                    }
                    catch (TaskCanceledException) { }
                    finally
                    {
                        targetProc.Dispose();
                        this.cancelSrc = null;
                        this.SetCancelButtonState(false);
                    }
                    try
                    {
                        File.Move(renameddxgi, reShadeFile);
                        this.OnReShadeStatusChange("Reshade renamed back to: dxgi.dll");
                    }
                    catch
                    {
                        this.OnReShadeStatusChange("Reshade failed to rename to dxgi.dll");
                    }
                    finally
                    {
                        this.tmpFilename = null;
                        this.OnProcessFound("Waiting for game to be started");
                        this.procWatcher.StartListen();
                    }
                }
            }
        }

        delegate void _OnProcessFound(string processPath);
        private void OnProcessFound(string processPath)
        {
            if (this.textBox1.InvokeRequired)
            {
                this.textBox1.BeginInvoke(new _OnProcessFound(OnProcessFound), processPath);
            }
            else
            {
                this.textBox1.Text = processPath;
            }
        }

        delegate void _SetCancelButtonState(bool enabled);
        private void SetCancelButtonState(bool enabled)
        {
            if (this.button1.InvokeRequired)
            {
                this.button1.BeginInvoke(new _SetCancelButtonState(SetCancelButtonState), enabled);
            }
            else
            {
                this.button1.Enabled = enabled;
            }
        }

        delegate void _OnReShadeStatusChange(string statusString);
        private void OnReShadeStatusChange(string statusString)
        {
            if (this.label3.InvokeRequired)
            {
                this.label3.BeginInvoke(new _OnReShadeStatusChange(OnReShadeStatusChange), statusString);
            }
            else
            {
                this.label3.Text = statusString;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            this.procWatcher.Dispose();
            if (this.trayIcon.IsValueCreated)
            {
                this.trayIcon.Value.Dispose();
            }
            base.OnFormClosed(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (this.tmpFilename != null)
                {
                    if (MessageBox.Show(this, $"Are you sure you want to exit the application before quitting the game?\r\nIf you close the application, you will need to rename '{this.tmpFilename}' back to 'dxgi.dll' later.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                }
            }
            base.OnFormClosing(e);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.procWatcher.StartListen();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Are you sure you want to rename '{this.tmpFilename}' back to 'dxgi.dll' right now?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                this.cancelSrc?.TrySetCanceled();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.hideToTray = true;
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (this.hideToTray)
                {
                    if (this.ShowInTaskbar != false)
                    {
                        this.ShowInTaskbar = false;
                    }
                    this.trayIcon.Value.Visible = true;
                }
                else
                {
                    if (this.ShowInTaskbar != true)
                    {
                        this.ShowInTaskbar = true;
                    }
                    this.trayIcon.Value.Visible = false;
                }
            }
            else
            {
                this.hideToTray = false;
                if (this.ShowInTaskbar != true)
                {
                    this.ShowInTaskbar = true;
                }
                this.trayIcon.Value.Visible = false;
            }
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            // this.RestoreBounds
            this.WindowState = FormWindowState.Normal;
        }
    }
}
