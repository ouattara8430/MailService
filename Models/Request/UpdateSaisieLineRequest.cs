using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models.Request
{
    public class UpdateSaisieLineRequest
    {
        public int id { get; set; }
        public string account_saisie { get; set; }
    }
}