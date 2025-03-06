using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models.Identity
{
    public class EmailSettings
    {
        public int Port { get; set; }
        public string SmtpServer { get; set; }
        public string Email { get; set; }
        public string DisplayedName { get; set; }
        public string Password { get; set; }
    }
}
