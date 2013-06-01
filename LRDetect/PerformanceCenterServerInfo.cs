using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class PerformanceCenterServerInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "DBC0B694AF01C15448B57B6BF896DE16"; }
        }

        //protected override string productCode = "";

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; }
        }

        protected override string[] ExecutableFiles
        {
            get
            {
                return new string[] { @"bin\HostSetup.exe", @"bin\KAdminUI.exe", @"bin\HP.PC.PCS.Configurator.exe" };
            }
        }

        //protected override string agentServiceCaption
        //{
        //    get { return "HP LoadRunner Launcher Service"; }
        //}

        //protected override string agentServiceName
        //{
        //    get { return "alagentservice"; }
        //}

        //protected override string agentProcessName
        //{
        //    get { return "alagentproc"; }
        //}

        //protected override string agentProcessCaption
        //{
        //    get { return "HP LoadRunner Launcher Process"; }
        //}
        public override string[] LatestPatchNames { get { return new string[] { "", "HP Performance Center 11.00 Server Patch 4v2", "" }; } }

        public override string[] LatestPatchURLs
        {
            get
            {
                return new string[] { 
                    "", 
                    "", 
                    "" 
                };
            }
        }
    }
}
