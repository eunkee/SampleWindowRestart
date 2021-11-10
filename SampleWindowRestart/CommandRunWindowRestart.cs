using System;

namespace SampleWindowRestart
{
    public static class CommandRunWindowRestart
    {
        //Restart Command
        public static void RunRestart()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo proInfo = new();
                System.Diagnostics.Process pro = new();
                proInfo.FileName = @"cmd";
                proInfo.CreateNoWindow = true;
                proInfo.UseShellExecute = false;
                proInfo.RedirectStandardOutput = true;
                proInfo.RedirectStandardInput = true;
                proInfo.RedirectStandardError = true;
                pro.StartInfo = proInfo;
                pro.Start();
                pro.StandardInput.Write(@"shutdown -r -f -t 0" + Environment.NewLine);
                pro.StandardInput.Close();
                pro.WaitForExit();
                pro.Close();
            }
            catch
            {
                System.Diagnostics.Trace.WriteLine("RunRestart: Failed run restart");
            }
        }
    }
}
