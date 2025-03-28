using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models
{
    public class Staff
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string agence { get; set; }
        public string email { get; set; }
        public string created_at { get; set; }
    }
}