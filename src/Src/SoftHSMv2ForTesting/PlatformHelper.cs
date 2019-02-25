using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    internal static class PlatformHelper
    {
        public static void CheckPlatform()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if(!isWindows)
            {
                throw new PlatformNotSupportedException("SoftHSMv2ForTesting support Windows only, for more info or contribute see https://github.com/harrison314/SoftHSMv2ForTesting");
            }
        }
    }
}
