using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ais.Internal.Dcm.Web.Models
{
    public class EncodingTypeModel
    {
        [Required]
        public string TechnicalName { get; set; }
        [Required]
        public string FriendlyName { get; set; }
    }
}