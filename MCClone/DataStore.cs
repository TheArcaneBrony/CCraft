﻿#if CLIENT
//using MCClone.Client.UI;
#endif
using System.Collections.Generic;
//using System.Management;
using System.Reflection;
using System.Threading;

namespace MCClone
{
    public static class DataStore
    {
        public const string Ver = "Alpha 0.08_02130";
#if TERRAIN_GEN_TEST
        public const bool Logging = false;
#else
        public const bool Logging = true;
#endif
        public static readonly SystemInfo SystemInfo = new SystemInfo();
        public static readonly List<Thread> Threads = new List<Thread>();
        public static readonly List<Player> Players = new List<Player>();
        public static readonly Dictionary<string, string> GameArgs = new Dictionary<string, string>();
#if CLIENT
        //public static ActivityViewer activityViewer;
        public static Player Player;
#endif
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
            /*int i = 0;
            //ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            CPU = "";
            foreach (ManagementObject mo in mos.Get())
            {
                CPU += $"CPU{i++}: {(mo["Name"].ToString().Trim())} {mo["DataWidth"]}-bit @ {mo["CurrentClockSpeed"]} MHz ({mo["NumberOfEnabledCore"]}/{mo["NumberOfCores"]}C{mo["NumberOfLogicalProcessors"]}/{mo["ThreadCount"]}T)\n";
                TotalCPUCores += int.Parse(mo["NumberOfEnabledCore"] + "") ;
                TotalCPUThreads += int.Parse(mo["ThreadCount"]+"");
            }
            TotalCPUs = i++;
            mos.Dispose();
            i = 0;
            mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            GPU = "";
            foreach (ManagementObject mo in mos.Get())
            {
                GPU += $"GPU{i++}: {(mo["Name"])} (Driver: {mo["DriverVersion"]})\n";
            }
            TotalGPUs = i++;
            mos.Dispose();*/
        }
        public string CPU = "Unknown";
        public int TotalCPUs = 0;
        public int TotalCPUCores = 0;
        public int TotalCPUThreads = 0;
        public string GPU = "Unknown";
        public int TotalGPUs = 0;
    }
}
