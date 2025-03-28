using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models
{
    public class VoteModel
    {
        public int vote_id { get; set; }
        public int candidate_id { get; set; }
        public int userid { get; set; }
        public string matricule { get; set; }
        public string vote_status { get; set; }
        public string username { get; set; }
    }
}