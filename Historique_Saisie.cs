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
    
    public partial class Historique_Saisie
    {
        public int historique_saisie_id { get; set; }
        public Nullable<int> saisie_id { get; set; }
        public string amount_historique_saisie { get; set; }
        public Nullable<System.DateTime> date_historique_saisie { get; set; }
        public string executed_by { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> updated_at { get; set; }
    
        public virtual Saisie Saisie { get; set; }
    }
}
