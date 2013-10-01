using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ais.Internal.Dcm.Web.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public bool EditNotAllowed { get; set; }
    }
}