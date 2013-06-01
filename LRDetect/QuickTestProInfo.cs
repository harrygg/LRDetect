using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class QuickTestProInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "99F9FE8FCA23AC3488DB13B6F1837C99"; }
        }

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\QuickTest Professional\"; }
        }

        protected override string[] ExecutableFiles
        {
            get { return new string[] { @"bin\QTPro.exe" }; }
        }
    }
}
