using Gluetun;
using qBittorrent;
using Cronix;

namespace Gluenet_QBT_PortForward_Update
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = Config.LoadConfig("config/config.toml");
            var cron = new Cron("glueQBT", config);
            cron.Start();          
            config.LoadSecrets();
            var qAPI = new qBittorrentClient(config.QbtURI!);
            var gAPI = new GluetunClient(config.GlueURI!);
            qAPI.Login(config.QbtUsername, config.QbtPassword);
            qAPI.GetPreferences();
            var port = gAPI.GetOpenVPNPort();
            var listen = qAPI.GetListenPort();
            if (listen != port)
                qAPI.SetPreferences(new Dictionary<string, object>() { { "listen_port", port } });
            cron.Complete();
        }
    }
}