using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.JSInterop;
using System.ServiceProcess;

namespace BlazorApp7.Data
{
    public class LiveEditor
    {
        IJSRuntime jsrun { get; set; }
        private string syncPackName { get; set; } = "Syncfusion.EJ2.Blazor";
        public string getPageText()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pages", "pagecode.txt");
            string text = File.ReadAllText(file);
            return text;
        }
        public string getHostHtmlText()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pages", "host.txt");
            string text = File.ReadAllText(file);
            return text;
        }
        public string getStartUpText()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pages", "startup.txt");
            string text = File.ReadAllText(file);
            return text;
        }

        
        public string templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "Samples/BlazorAppServer/BlazorAppServer");

        Process process = new Process();

        internal void BuildEngine(DataValue data, IJSRuntime jsRuntime=null)
        {
            this.jsrun = jsRuntime;
            //this.isAlreadyInstalled();
            var startUpCode = data.startup;
            var hostCode = data.host;
            var pageCode = data.pages;
            System.IO.File.WriteAllText(templateLocation + @"\Startup.cs", startUpCode);
            System.IO.File.WriteAllText(templateLocation + @"\Pages\_Host.cshtml", hostCode);
            System.IO.File.WriteAllText(templateLocation + @"\Pages\Index.razor", pageCode);
            
            List<string> cmds = new List<string>();

            Process process = new Process();
            string dllVersion = "17.4.0.39";
            string version = " -v " + dllVersion;
            string dotnetPack = "dotnet add package" + " " + this.syncPackName + version;
            string dotnetRestore = "dotnet restore";
            string dotnetClean = "dotnet clean";
            string dotnetRun = "dotnet run";
            string processName = "Imagename eq BlazorAppServer.exe";
            string stopExistingProcess = "tasklist.exe /nh /fi "+ '"' + processName + '"' + " && taskkill /f /im BlazorAppServer.exe";
            //string publishFile = Path.Combine(Directory.GetCurrentDirectory(), "Publish" + "\\" + "Result");
            //string dotnetPublish = "dotnet publish -c Release -o " + publishFile;
            cmds.Add(stopExistingProcess);
            cmds.Add(dotnetClean);
            cmds.Add(dotnetPack);
            cmds.Add(dotnetRestore);
            cmds.Add(dotnetRun);
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Windows\System32\cmd.exe";
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = templateLocation;
            process.StartInfo = psi;
            process.Start();
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data !=null && e.Data.Trim() == "Application started. Press Ctrl+C to shut down.")
                {
                    this.jsrun.InvokeVoidAsync("exampleJsFunctions.renderIframe", DotNetObjectReference.Create(this));
                } else
                {
                    this.jsrun.InvokeVoidAsync("exampleJsFunctions.outputLog", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                Console.WriteLine(e.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            using (StreamWriter sw = process.StandardInput)
            {
                foreach (var cmd in cmds)
                {
                    sw.WriteLine(cmd);
                }
            }
        }
    }

    

    public class DataValue
    {
        public string pages { get; set; }
        public string startup { get; set; }
        public string host { get; set; }
    }
}
