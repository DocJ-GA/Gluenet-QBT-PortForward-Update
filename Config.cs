using Cronix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomlyn;

namespace Gluenet_QBT_PortForward_Update
{
    internal class Config : Configurations, IConfigurations
    {
        protected string _qbtUrl, _glueUrl;

        public string QbtUrl
        {
            get => _qbtUrl;
            set
            {
                _qbtUrl = value;
                if (string.IsNullOrWhiteSpace(_qbtUrl))
                    QbtURI = null;
                QbtURI = new Uri(_qbtUrl);
            }
        }
        [IgnoreDataMember]
        public Uri? QbtURI { get; set; }

        public string GlueUrl
        {
            get => _glueUrl;
            set
            {
                _glueUrl = value;
                if (string.IsNullOrWhiteSpace(_glueUrl))
                    GlueURI = null;
                GlueURI = new Uri(_glueUrl);
            }
        }
        [IgnoreDataMember]
        public Uri? GlueURI { get; set; }
        public string QbtUsername { get; set; } = string.Empty;
        public string QbtPassword { get; set; } = string.Empty;
        public string GlueUsername { get; set; } = string.Empty;
        public string GluePassword { get; set; } = string.Empty;


        public Config() : base()
        {
            _qbtUrl = _glueUrl = "";
        }

        public void LoadSecrets()
        {
            var toml = Toml.ToModel(File.ReadAllText("config/secrets"));
            if (toml == null)
                throw new FileNotFoundException("The secrets file does not exist.");
            if (!toml.ContainsKey("qbt_username") || !toml.ContainsKey("qbt_password"))
                throw new ArgumentNullException("The qbt_username or qbt_password is not set.");
            QbtUsername = toml["qbt_username"] as string ?? string.Empty;
            QbtPassword = toml["qbt_password"] as string ?? string.Empty;
            if (toml.ContainsKey("glue_username"))
                GlueUsername = toml["glue_username"] as string ?? string.Empty;
            if (toml.ContainsKey("glue_password"))
                GluePassword = toml["glue_password"] as string ?? string.Empty;
        }

        public static new Config LoadConfig(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The configuration file '" + path + "' was not found.");
            var rawConfig = File.ReadAllText(path);
            var config = Toml.ToModel<Config>(rawConfig);
            return config;

        }
    }
}
