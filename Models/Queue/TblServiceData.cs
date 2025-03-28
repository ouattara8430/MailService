using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models.Queue
{
    public class TblServiceData
    {
        public int service_id { get; set; }
        public string service_label { get; set; }
        public List<FetchListService_Result> services { get; set; }
    }
}