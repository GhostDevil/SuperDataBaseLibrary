using System;
using System.Linq;
using System.Xml.Linq;

namespace SuperDataBase
{
    /// <summary>
    /// 注意，程序的配置文件和机器配置是互斥的，也就是这段代码只能在其中一个文件中存在，不能同时存在，也不能都存在。动态的处理配置
    /// </summary>
    public  class OracleDriverWithoutClient
    {
        private static OracleDriverWithoutClient _instance = null;

        /// <summary>
        /// 初始化oracle在配置文件中驱动
        /// </summary>
        private OracleDriverWithoutClient(string productName)
        {
            if (!CheckFramework4MachineConfig() && !CheckAppconfig(productName))
            {
                //在机器配置和程序运行配置文件都没有oracle托管驱动，那么添加
                AddOracleManagedDriver(productName);
            }
            else if (CheckFramework4MachineConfig() && CheckAppconfig(productName))
            {
                //在机器配置中存在，在程序运行配置文件同时存在，那么从程序运行配置中删除
                RemoveOracleManagedDriver(productName);
            }
        }
        public static OracleDriverWithoutClient CreateInstance(string productName)
        {
            if (_instance == null)
            {
                _instance = new OracleDriverWithoutClient(productName);
            }
            return _instance;
        }

        /// <summary>
        /// 检测机器配置未见是否已经安装了oralce托管驱动
        /// </summary>
        /// <returns></returns>

        bool CheckFramework4MachineConfig()
        {
            var machinePath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\machine.config";
            var ele = XElement.Load(machinePath);
            var addNodes = ele.Element("system.data").Element("DbProviderFactories").Elements("add").ToList();

            if (addNodes.Any(o => o.Attribute("name").Value == "ODP.NET, Managed Driver"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测程序配置文件中是否已经存在了oracle托管驱动
        /// </summary>
        /// <returns></returns>
        bool CheckAppconfig(string productName)
        {
            var configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\{0}.exe.config", productName);
            var appConfigElement = XElement.Load(configPath);

            var systemDataEle = appConfigElement.Element("system.data");

            if (systemDataEle == null) return false;
            var dbProviderFactoriesEle = systemDataEle.Element("DbProviderFactories");
            if (dbProviderFactoriesEle == null) return false;
            if (!dbProviderFactoriesEle.Elements("add").Where(o => o.Attribute("name").Value == "ODP.NET, Managed Driver").Any())
                return false;
            return true;

        }

        /// <summary>
        /// 向程序驱动中添加oracle托管驱动配置
        /// </summary>
        void AddOracleManagedDriver(string productName)
        {
            var configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\{0}.exe.config", productName);
            var appConfigElement = XElement.Load(configPath);

            var systemDataEle = appConfigElement.Element("system.data");

            if (systemDataEle == null)
            {
                appConfigElement.Add(new XElement("system.data"));
                systemDataEle = appConfigElement.Element("system.data");
            }
            var dbProviderFactoriesEle = systemDataEle.Element("DbProviderFactories");
            if (dbProviderFactoriesEle == null)
            {
                systemDataEle.AddFirst(new XElement("DbProviderFactories"));
                dbProviderFactoriesEle = systemDataEle.Element("DbProviderFactories");
            }
            if (!dbProviderFactoriesEle.Elements("add").Where(o => o.Attribute("name").Value == "ODP.NET, Managed Driver").Any())
            {
                dbProviderFactoriesEle.AddFirst(new XElement("add", new object[] {
                    new XAttribute("name", "ODP.NET, Managed Driver")
                    , new XAttribute("invariant", "Oracle.ManagedDataAccess.Client")
                    ,new XAttribute("description","Oracle Data Provider for .NET, Managed Driver"),
                new XAttribute("type","Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Version=4.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342")}));
            }
            appConfigElement.Save(configPath, SaveOptions.None);
        }

        /// <summary>
        /// 从程序配置文件中删除oracle托管驱动配置
        /// </summary>
        void RemoveOracleManagedDriver(string productName)
        {
            var configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\{0}.exe.config", productName);
            var appConfigElement = XElement.Load(configPath);

            var systemDataEle = appConfigElement.Element("system.data");

            if (systemDataEle == null) return;
            var dbProviderFactoriesEle = systemDataEle.Element("DbProviderFactories");
            if (dbProviderFactoriesEle == null) return;

            if (dbProviderFactoriesEle.Elements("add").Where(o => o.Attribute("name").Value == "ODP.NET, Managed Driver").Any())
            {
                dbProviderFactoriesEle.Elements("add").Where(o => o.Attribute("name").Value == "ODP.NET, Managed Driver").Remove();
            }
            appConfigElement.Save(configPath, SaveOptions.None);
        }
    }
}
