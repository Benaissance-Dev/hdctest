using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hdcNoticeGeneratorConsole
{
    public class Util
    {
        public static bool IsWindows()
        {
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 128))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
