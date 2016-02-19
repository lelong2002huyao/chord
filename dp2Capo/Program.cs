﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.ServiceProcess;

using dp2Capo.Properties;

using DigitalPlatform.ServiceProcess;
using DigitalPlatform;

namespace dp2Capo
{
    class Program : MyServiceBase
    {
        static Program()
        {
            // this.ServiceName = "dp2 Capo Service";
            ServiceShortName = "dp2capo";
        }

        static void Main(string[] args)
        {

            // 修改配置
            if (args.Length >= 1 && args[0].Equals("setting"))
            {
                ChangeSettings(args.Length > 1 ? args[1] : "");
                return;
            }

            if (args.Length == 1 && args[0].Equals("console"))
            {
                if (Initial() == false)
                    return;
                new Program().ConsoleRun();
            }
            else
            {
                // 这是从命令行启动的情况
                if (Environment.UserInteractive == true)
                {
                    Console.WriteLine("dp2capo 用法:");
                    Console.WriteLine("注册 Windows Service: dp2capo install");
                    Console.WriteLine("注销 Windows Service: dp2capo uninstall");
                    Console.WriteLine("以控制台方式运行: dp2capo console");
                    Console.WriteLine("修改配置参数: dp2capo setting");

                    Console.WriteLine("(按回车键返回)");
                    Console.ReadLine();
                    return;
                }

                if (Initial() == false)
                    return;

                // 这是被当作 service 启动的情况
                ServiceBase.Run(new Program());
            }
        }

        // return:
        //      true    初始化成功
        //      false   初始化失败
        static bool Initial()
        {
            try
            {
                ServerInfo.Initial(Settings.Default.DataDir);
                return true;
            }
            catch (Exception ex)
            {
                WriteWindowsLog(ExceptionUtil.GetAutoText(ex), EventLogEntryType.Error);
                Console.WriteLine("初始化失败: " + ex.Message);
                return false;
            }
        }

        static void SetOneParameter(string strPromptName, Settings obj, string strFieldName)
        {
            PropertyInfo info = obj.GetType().GetProperty(strFieldName);
            string value = (string)info.GetValue(obj, null);
            Console.WriteLine("请输入 "+strPromptName+": (当前值为 '" + value + "' )");
            string strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                info.SetValue(obj, strNewValue, null);
        }

        // 修改配置
        // parameters:
        //      strInstanceIndex    实例下标。从 1 开始计数。如果为空，表示仅仅设置 DataDir 参数。如果不为空，表示设置一个具体的实例的参数
        static void ChangeSettings(string strInstanceIndex)
        {
            Console.WriteLine("(直接回车表示不修改当前值)");

            if (string.IsNullOrEmpty(strInstanceIndex) == true)
            {
                SetOneParameter("数据目录", Settings.Default, "DataDir");

                Settings.Default.Save();
            }
            else
            {
                int index = Int32.Parse(strInstanceIndex) - 1;
                ChangeInstanceSettings(index);
            }

            Console.WriteLine();
            Console.WriteLine("注：修改将在服务重启以后生效");
            Console.WriteLine("(按回车键返回)");
            Console.ReadLine();
            return;
        }

        static void ChangeInstanceSettings(int index)
        {
            ServerInfo.ChangeInstanceSettings(Settings.Default.DataDir, index);
        }

        protected override void OnStart(string[] args)
        {
            StartServer();
        }

        protected override void OnStop()
        {
            this.Close();
        }

        public override void Close()
        {
            base.Close();

            ServerInfo.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }

            base.Dispose(disposing);
        }

        static void StartServer()
        {

        }
    }
}