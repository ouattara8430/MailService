//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailService
{
    using System;
    using System.Collections.Generic;
    
    public partial class TicketHistory
    {
        public int ticket_history_id { get; set; }
        public Nullable<int> ticket_id { get; set; }
        public Nullable<int> decision_id { get; set; }
        public Nullable<System.DateTime> changed_date { get; set; }
    
        public virtual Ticket Ticket { get; set; }
    }
}
