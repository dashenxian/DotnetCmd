using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotnetCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestCMD();
            var serviceFileName = "kestrel-dotnet.service";
            var workingDirectory = "/home/lkl/Desktop/publish-linux";
            CreateServiceFile(serviceFileName, workingDirectory);
            ExcuteCMD("sudo", $"systemctl enable {serviceFileName}");
            ExcuteCMD("sudo", $"systemctl start {serviceFileName}");
        }

        private static void TestCMD()
        {
            string fileName = "shell/";

            //根据系统使用不同的shell文件
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName += "win.bat";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName += "linux.sh";
                //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
                var psi1 = new ProcessStartInfo("chmod", $"+x {fileName}") {RedirectStandardOutput = true};
                //启动
                var proc1 = Process.Start(psi1);
                //proc1.WaitForExit();
            }
            else
            {
                fileName += "OSX.sh";
            }

            //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
            var psi = new ProcessStartInfo(fileName) {RedirectStandardOutput = true};
            //启动
            var proc = Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine("Can not exec.");
            }
            else
            {
                Console.WriteLine("-------------Start read standard output--------------");
                //开始读取
                using (var sr = proc.StandardOutput)
                {
                    while (!sr.EndOfStream)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }

                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }

                Console.WriteLine("---------------Read end------------------");
                //Console.WriteLine($"Exited Code ： {proc.ExitCode}");
            }
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        private static void ExcuteCMD(string command,string arguments=null)
        {
            var psi = new ProcessStartInfo(command,arguments) { RedirectStandardOutput = true };
            //启动
            var proc = Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine($"Can not exec. command:{command},arguments:{arguments}");
            }
            else
            {
                //Console.WriteLine("-------------Start read standard output--------------");
                //开始读取
                using (var sr = proc.StandardOutput)
                {
                    while (!sr.EndOfStream)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }

                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }

                //Console.WriteLine("---------------Read end------------------");
                //Console.WriteLine($"Exited Code ： {proc.ExitCode}");
            }
        }
        public static void CreateServiceFile(string serviceFileName,string workingDirectory)
        {
            var serviceFileFullPath = $"/etc/systemd/system/{serviceFileName}";
            var fileStr = GetServiceFileStr(workingDirectory);
            //File.Create(serviceFileName);
            File.WriteAllText(serviceFileFullPath, fileStr);
        }
        public static string GetServiceFileStr(string workingDirectory)
        {
            return $@"[Unit]
Description=Example GDBDCMappingDbService running on Uos

[Service]
WorkingDirectory={workingDirectory}
ExecStart=/usr/bin/dotnet GDBDCMappingDbService.HttpApi.Host.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target";
        }
    }
}
