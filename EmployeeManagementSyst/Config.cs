using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementSyst
{
    public class Config
    {
        public IConfigurationRoot Builder { get; }

        public Config()
        {
            Builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Config>()
                .Build();

        }

        public string MasterConn => Builder["MasterConn"] ?? throw new Exception("MasterConn missing!");
        public string AppConn => Builder["AppConn"] ?? throw new Exception("AppConn missing!");

        // Email configuration values
        public string EmailSender => "akc.sreehari@gmail.com";
        public string EmailPassword => "dboh xwez gdco ufwd";
        // Maximum legal work hours for a single shift. Modify here as needed.
        public int LegalWorkHours => 16;
    }
}
