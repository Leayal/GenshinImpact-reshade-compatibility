using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;

namespace GenshinImpact_reshade_compatibility.Utils
{
    static class ProcessHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        const uint QueryLimitedInformation = 0x1000;

        public static string GetProcessFilename(uint processId) => GetProcessFilename(processId, 4096);

        public static string GetProcessFilename(uint processId, int bufferSize)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                // QueryLimitedInformation to avoid Access denied when we are not elevated.
                // And this is what we shoul do. Requires elevation just to get filename is overkill and not so secure.
                handle = OpenProcess(QueryLimitedInformation, false, processId);
                if (handle == IntPtr.Zero)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                int capacity = bufferSize;
                StringBuilder sb = new StringBuilder(capacity);
                if (QueryFullProcessImageName(handle, 0, sb, ref capacity) == 0)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                return sb.ToString(0, capacity);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    CloseHandle(handle);
                }
            }
        }
    }
}
