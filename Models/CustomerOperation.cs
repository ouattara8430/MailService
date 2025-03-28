using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models
{
    public class CustomerOperation
    {
        public int header_id { get; set; }
        public int userId { get; set; }
        public int initiated_by { get; set; }
        public string account_no { get; set; }
        public string client { get; set; }
        public string montant { get; set; }
        public string date_saisie { get; set; }
        public string statut { get; set; }
        public string libelle_saisie { get; set; }
        public string etape { get; set; }
        public int case_id { get; set; }
        public string document_name { get; set; }
        public List<Record> record { get; set; }

        // New property to hold file data
        public FileUpload Files { get; set; }

        public class Record
        {
            public int saisie_operation_id { get; set; }
            public string branch_code { get; set; }
            public string account_no { get; set; }
            public string account_name { get; set; }
            public string operation_description { get; set; }
            public string amount_debit { get; set; }
            public string amount_credit { get; set; }
            public int saisie_type_id { get; set; }
            public DateTime created_at { get; set; }
            //public string created_at { get; set; }
        }

        // New class to represent file uploads
        public class FileUpload
        {
            public string FileName { get; set; }
            public byte[] FileContent { get; set; }
        }
    }
}