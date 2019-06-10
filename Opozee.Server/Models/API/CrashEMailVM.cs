using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Server.Models.API
{
    public class CrashEMailVM
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Info { get; set; }
        public string Exception { get; set; }
    }
}