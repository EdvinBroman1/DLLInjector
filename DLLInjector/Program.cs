using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DLLInjector
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
        IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        static void Main(string[] args)
        {
            Console.WriteLine("DLL Injector by Edvin\n\nEither type the Process name you wanna inject into or use {List} to see a list of all processes running..\n\n");

            string procName = Console.ReadLine();
            if (procName.ToLower() == "list")
            {
                foreach (Process p in Process.GetProcesses())
                {
                    Console.WriteLine(p.ProcessName);
                }
            }
            Process[] procs = Process.GetProcessesByName(procName);
            Process proc;
            while (procs.Length < 1)
            {
                Console.Clear();
                Console.Write("Invalid Process name, try again: ");

                procName = Console.ReadLine();
                if (procName.ToLower() == "list")
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        Console.WriteLine(p.ProcessName);
                    }
                    Console.Write("\nInject Into: ");
                   procName = Console.ReadLine();
                }
                procs = Process.GetProcessesByName(procName);
            }
            proc = procs.FirstOrDefault();

            Console.WriteLine("Process: " + proc.ProcessName + " found with MainModule BaseAddress as 0x" + proc.MainModule.BaseAddress.ToString());
            Console.Write("Please submit the path to your DLL: ");
            string dllPath = Console.ReadLine();
            Inject:
            Console.WriteLine("Injecting...");
            int bytesToAllocate = dllPath.Length;

            IntPtr loadlibAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA"); // Locating pointer to LoadLibraryA...
            IntPtr loadAddress = VirtualAllocEx(proc.Handle, (IntPtr)null, (IntPtr)bytesToAllocate, (0x1000 | 0x2000), 0x40); // Allocating memory for dllpath in process, returning a pointer to memory region
            byte[] dllPathInBytes = ASCIIEncoding.ASCII.GetBytes(dllPath); // Preparing the WPM parameters...

            int AmountOfBytesWritten = 0; // used for debugging purposes
            WriteProcessMemory(proc.Handle, loadAddress, dllPathInBytes, (uint)dllPathInBytes.Length, AmountOfBytesWritten); // Writing into the codecave
            CreateRemoteThread(proc.Handle, (IntPtr)null, IntPtr.Zero, loadlibAddress, loadAddress, 0, (IntPtr)null);
            CloseHandle(proc.Handle);


            Console.WriteLine("Your DLL has been Injected..");
            Console.ReadLine();
            Console.Clear();
            goto Inject;

        }

    }
}

