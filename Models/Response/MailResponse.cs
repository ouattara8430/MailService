using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models.Response
{
    public class MailResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string data { get; set; }
        public object record { get; set; }
    }
}