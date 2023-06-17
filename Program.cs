using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    public static extern bool GetThreadContext(IntPtr hThread, IntPtr lpContext);

    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT
    {
        public uint ContextFlags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public uint FloatSave;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    [Flags]
    public enum ThreadAccess : int
    {
        QueryInformation = 0x00000040,
        GetContext = 0x00000008,
        AllAccess = 0x001F0FFF
    }

    static void Main()
    {
        // Find the process by name
        Process[] processes = Process.GetProcessesByName("svchost");

        foreach (Process process in processes)
        {
            // Get all threads associated with the process
            ProcessThreadCollection threads = process.Threads;

            // Display information for each thread
            foreach (ProcessThread thread in threads)
            {
                Console.WriteLine("Thread ID: {0}\tStart Time: {1}\tPriority: {2}",
                    thread.Id, thread.StartTime, thread.PriorityLevel);

                IntPtr hThread = OpenThread(ThreadAccess.GetContext, false, thread.Id);
                if (hThread != IntPtr.Zero)
                {
                    IntPtr startAddress = IntPtr.Zero;

                    CONTEXT context = new CONTEXT();
                    context.ContextFlags = 0x10007; // CONTEXT_CONTROL

                    if (GetThreadContext(hThread, Marshal.AllocHGlobal(Marshal.SizeOf<CONTEXT>())))
                    {
                        startAddress = GetProcAddress(GetModuleHandle("ntdll.dll"), "NtQueryInformationThread");

                        Console.WriteLine("Start Address: {0}", startAddress);
                    }

                    CloseHandle(hThread);
                }
            }
        }

        Console.WriteLine("Process not found.");
    }
}
