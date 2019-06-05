using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Server.Models
{
    public class FollowerVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Following { get; set; }
        public bool IsFollowing { get; set; }
    }
}