using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Cron.Extensions;
using static Cron.Syslog;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace Cron
{
    public class Cron
    {
        public DateTime StartTime { get; set; }

        public DateTime StopTime { get; set; }

		public Configuration? Configs { get; private set; }

        public Cron(Configuration? configs)
        {
			Configs = configs;
        }


        public void Start(string message = "Starting App.")
        {
            StartTime = DateTime.Now;
            Log(message);

            // Lets see if the process is running.
            if (Configs != null && File.Exists(Configs.PSF))
            {
                // The file exists, it may still be running.
                var status = File.ReadAllLines(Config.PSF);
                if (status[0] != "complete")
                {

                    // The status of the file seems to imply it is running. Exiting.
                    Log("Cronicology already running, exiting.", Level.Warning);
                    Environment.Exit(0);
                }
            }

            // Updating the process status file.
            Log("Writing the process status file at '" + Config.PSF + "'.", system: false);
            if (Configs != null)
				File.WriteAllText(Configs.PSF, "started");

            // Starting the log.
            if (Configs != null && File.Exists(Configs.Log))
            {
                // Log file exists. Checking size.
                var info = new FileInfo(Config.Log);
                if (info.Length > Config.LogMaxSize * 1024 * 1024)
                {
                    // Log file is greater than 10 MiB so it needs rotated.
                    Log("Log file greater than 10 MiB, rotating it.");
                    Log(DateTime.Now.ToString("s") + ": Rotating log file", system: false);
                    File.Move(Configs.Log, Config.LogPath + "/cronicology.old." + DateTime.Now.ToString("s"));
                    File.Create(Configs.Log);
                    Log(DateTime.Now.ToString("s") + ": Log file created.", system: false);

                    var oldLogs = Directory.GetFiles(Configs.LogPath, "cronicology.old.*").Select(p => Path.GetFileName(p)).ToArray();
                    if (oldLogs.Length > 5)
                    {
                        // We got too many logs, we will delete oldest to newest until we have five.
                        var orderedList = new SortedList<DateTime, string>();
                        foreach (var fileName in oldLogs)
                        {
                            var match = long.Parse(Regex.Match(fileName, @"cronicology\.old\.(.+)").Groups[1].Value);
                            var date = new DateTime(match);
                            orderedList.Add(date, fileName);
                        }

                        foreach (var entry in orderedList.Take(orderedList.Count - 5))
                        {
                            File.Delete(Configs.LogPath + entry.Value);
                        }
                    }
                }

            }
            else
            {
                // The log file doesn't exists. Creating it.
				if (Configs != null)
				{
	                File.Create(Configs.Log);
	                File.AppendAllText(Configs.Log, DateTime.Now.ToString("s") + ": Log file created.");
				}
            }
        }


        public void Fail(string message = "Ending Cronicology in a failed state.")
        {
            Log(message, Syslog.Level.Err);
			if (Configs != null)
            	File.WriteAllText(Configs.PSF, "failed");
            RunTime();
        }


        public void Complete(string message = "Cronicology complete.")
        {
            Log("Writing complete to PSF file.", system: false);
            Log(message);
            if (Configs != null)
				File.WriteAllText(Configs.PSF, "complete");
            RunTime();

        }

        public void RunTime()
        {
            StopTime = DateTime.Now;
            var runTime = StopTime - StartTime;
            Log("Total Run time: " + runTime.ToReadableString() + ".", system: false);
        }


        public void Log(string message, Syslog.Level level = Level.Info, string identity = "cronicology", bool system = true)
        {
            if (system)
                Syslog.Write(message, level, identity);

			if (Configs != null)
            	File.AppendAllText(Configs.Log, DateTime.Now.ToString("s") + "[" + level + "]: " + message + "\n");
        }

        public void LogInfo(string message)
        {
            Log(message, level: Level.Info, system: false);
        }

        public void Debug(string message)
        {
            if (Config.Debug)
                Log(message, Level.Debug, system: false);
        }
    }
}
