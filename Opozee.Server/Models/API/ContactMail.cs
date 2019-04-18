using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Server.Models.API
{
    public class ContactMail
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public bool IsFromContact { get; set; }
    }
}