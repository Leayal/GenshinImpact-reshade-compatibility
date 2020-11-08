using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GenshinImpact_reshade_compatibility.Classes
{
    class ProcessWatcher : IDisposable
    {
        private ManagementEventWatcher wmiEventWatcher;

        private static Lazy<ManagementScope> lazyScope = new Lazy<ManagementScope>(() => new ManagementScope("root\\CIMV2"));

        public ProcessWatcher() : this(null, null) { }

        public ProcessWatcher(TimeSpan? interval) : this(null, interval) { }

        public ProcessWatcher(string targetName) : this(targetName, null) { }

        public ProcessWatcher(string targetName, TimeSpan? interval)
        {
            WqlEventQuery query;
            if (interval.HasValue)
            {
                if (string.IsNullOrEmpty(targetName))
                {
                    query = new WqlEventQuery("__InstanceCreationEvent", interval.Value, "TargetInstance Isa 'Win32_Process'");
                }
                else
                {
                    query = new WqlEventQuery("__InstanceCreationEvent", interval.Value, $"TargetInstance Isa 'Win32_Process' And TargetInstance.Name = '{targetName}'");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(targetName))
                {
                    query = new WqlEventQuery("Select * From __InstanceCreationEvent Where TargetInstance Isa 'Win32_Process'");
                }
                else
                {
                    query = new WqlEventQuery($"Select * From __InstanceCreationEvent Where TargetInstance Isa 'Win32_Process' And TargetInstance.Name = '{targetName}'");
                }
            }
            this.wmiEventWatcher = new ManagementEventWatcher(lazyScope.Value, query);
            this.wmiEventWatcher.EventArrived += this.WmiEventWatcher_EventArrived;
        }

        private void WmiEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var instance = e.NewEvent.Properties["TargetInstance"];
            if (instance != null)
            {
                var win32_process = (ManagementBaseObject)instance.Value;
                uint procId = 0;
                var processIDObj = win32_process.Properties["ProcessId"].Value;
                if (processIDObj is uint unsignedNumber)
                {
                    procId = unsignedNumber;
                }
                else
                {
                    procId = Convert.ToUInt32(processIDObj);
                }
                var procPath = Utils.ProcessHelper.GetProcessFilename(procId);
                this.ProcessStarted?.Invoke(new ProcessStartedInfoArgs(procId, procPath));
            }
        }

        public event Action<ProcessStartedInfoArgs> ProcessStarted;

        public void StartListen()
        {
            this.wmiEventWatcher.Start();
        }
        public void StopListen() => this.wmiEventWatcher.Stop();

        public void Dispose()
        {
            if (this.wmiEventWatcher != null)
            {
                this.wmiEventWatcher.Stop();
                this.wmiEventWatcher.Dispose();
                this.wmiEventWatcher = null;
            }
        }
    }

    public struct ProcessStartedInfoArgs
    {
        public readonly uint ProcessID;
        public readonly string ProcessPath;

        public ProcessStartedInfoArgs(uint id, string path)
        {
            this.ProcessID = id;
            this.ProcessPath = path;
        }
    }
}
