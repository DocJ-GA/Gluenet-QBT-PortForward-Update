using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cron
{
    public class Configurations
    {

        public string LogPath { get; set; } = "/var/log/cron/";

        public float LogMaxSize { get; set; } = 10;

        public int LogMaxOld { get; set; } = 5;

        public string PSF { get; set; }

        public string Log
        {
            get
            {
                if (!LogPath.EndsWith("/"))
                    LogPath += "/";
                return LogPath + "/current.log";
            }
        }

        public Configurations()
        {
        }
    }
}
