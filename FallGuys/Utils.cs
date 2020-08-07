using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace FallGuys
{
    public static class Utils
    {
        public static string GetRemoteConfigJson()
        {

            var wc = new WebClient();
            return wc.DownloadString(
                "https://raw.githubusercontent.com/KiKoS0/Fall-guys-log-dumper/master/FallGuys/config.json");
        }
    }
}
