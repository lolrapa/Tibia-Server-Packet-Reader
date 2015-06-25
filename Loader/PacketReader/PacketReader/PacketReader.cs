using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


namespace PacketReader
{
    class PacketReader
    {
        #region dllimports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFileMapping(
        [In] uint hFile,
        [In][Optional] ref IntPtr lpAttributes,
        [In] int flProtect,
        [In] int dwMaximumSizeHigh,
        [In] int dwMaximumSizeLow,
        [In][Optional] string lpName
        );
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
        [In] IntPtr hFileMappingObject,
        [In] int dwDesiredAccess,
        [In] int dwFileOffsetHigh,
        [In] int dwFileOffsetLow,
        [In] int dwNumberOfBytesToMap
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenFileMapping(
             uint dwDesiredAccess,
             bool bInheritHandle,
             string lpName);


        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        const UInt32 SECTION_QUERY = 0x0001;
        const UInt32 SECTION_MAP_WRITE = 0x0002;
        const UInt32 SECTION_MAP_READ = 0x0004;
        const UInt32 SECTION_MAP_EXECUTE = 0x0008;
        const UInt32 SECTION_EXTEND_SIZE = 0x0010;
        const UInt32 SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE);
        const UInt32 FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;

        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;
        #endregion
        private Process tibiaProc;
        private IntPtr memoryMap;
          

        
        public PacketReader(Process p)
        {
            tibiaProc = p;

        }
        public bool Inject()
        {
            string name = "packetReaderMemoryFile";
            IntPtr MemHan1 = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, name);
            IntPtr MapPoint1 = MapViewOfFile(MemHan1, 0x02, 0, 0, 0);
            if (MapPoint1 != IntPtr.Zero)
            {
                memoryMap = MapPoint1;
                return true;
            }
            bool res = true;

            string DllPath = AppDomain.CurrentDomain.BaseDirectory + @"InjectedDll.dll";
            IntPtr handle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, tibiaProc.Id);
            IntPtr loadLibAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr addrAllocatedMemory = VirtualAllocEx(handle, IntPtr.Zero, (uint)((DllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            UIntPtr bytesWritten;
            res = WriteProcessMemory(handle, addrAllocatedMemory, Encoding.Default.GetBytes(DllPath), (uint)((DllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
            CreateRemoteThread(handle, IntPtr.Zero, 0, loadLibAddr, addrAllocatedMemory, 0, IntPtr.Zero);

            Thread.Sleep(200); //give time to the dll to create memory map

            IntPtr MemHan = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, name);
            IntPtr MapPoint = MapViewOfFile(MemHan, 0x02, 0, 0, 0);
            memoryMap = MapPoint;
            if (memoryMap == IntPtr.Zero)
            {
                res = false;
            }
            if (res)
            {
               
            }
            return res;
        }
        public  void readPacket(ref List<byte> res)
        {
            res.Clear();
            byte bufferState = Marshal.ReadByte(memoryMap);
            if (bufferState == 1)
            {

                int lenght = Marshal.ReadInt32(memoryMap + 1);
                Byte[] buffer = new Byte[lenght];
                Marshal.Copy(memoryMap + 5, buffer, 0, lenght);
                for (int i = 0; i < buffer.Length; i++)
                {
                    res.Add(buffer[i]);
                }
                Marshal.WriteByte(memoryMap, 0x0);
            }
        }

       


    }
}
