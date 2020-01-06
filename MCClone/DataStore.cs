using System.Collections.Generic;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Windows.Documents;

namespace MCClone
{
    public static class DataStore
    {
        public const string Ver = "Alpha 0.08_01320";
        public const bool Multiplayer =
#if SERVER
            true,
#else
            false,
#endif
            Server =
#if SERVER
            true;
#else
            false;
#endif
        public static SystemInfo SystemInfo = new SystemInfo();
        public static List<Thread> Threads = new List<Thread>();
    }
    public class ModData
    {
        public string Name { get; set; } = "Unnamed mod";
        public object Instance { get; set; } = null;
        public MethodInfo OnLoad { get; set; } = null;
        public MethodInfo OnResize { get; set; } = null;
        public MethodInfo OnUpdateFrame { get; set; } = null;
        public MethodInfo OnRenderFrame { get; set; } = null;
    }
    public class SystemInfo
    {
        public SystemInfo()
        {
            int i = 0;
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in mos.Get())
            {
                CPU += $"CPU{i++}: {(mo["Name"].ToString().Trim())} {mo["DataWidth"]}-bit @ {mo["CurrentClockSpeed"]} MHz ({mo["NumberOfEnabledCore"]}/{mo["NumberOfCores"]}C{mo["NumberOfLogicalProcessors"]}/{mo["ThreadCount"]}T)\n";
                TotalCPUCores += int.Parse(mo["NumberOfEnabledCore"] + "") ;
                TotalCPUThreads += int.Parse(mo["ThreadCount"]+"");
            }
            TotalCPUs = (int)i++;
            mos.Dispose();
            i = 0;
            mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in mos.Get())
            {
                GPU += $"GPU{i++}: {(mo["Name"])} (Driver: {mo["DriverVersion"]})\n";
            }
            TotalGPUs = (int)i++;
            mos.Dispose();
        }
        public string CPU;
        public int TotalCPUs;
        public int TotalCPUCores;
        public int TotalCPUThreads;
        public string GPU;
        public int TotalGPUs;
    }
}
