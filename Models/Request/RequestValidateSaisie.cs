using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models.Request
{
    public class RequestValidateSaisie
    {
        public int case_id{get; set;}
        public int userId { get; set; }
        public int role_id { get; set; }
        public int decision_id { get; set; }
        public string comment { get; set; }
    }
}