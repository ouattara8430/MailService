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
    
    public partial class Saisie
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Saisie()
        {
            this.Historique_Saisie = new HashSet<Historique_Saisie>();
        }
    
        public int saisie_id { get; set; }
        public Nullable<int> saisie_type_id { get; set; }
        public string amount_saisie { get; set; }
        public string account_no_saisie { get; set; }
        public Nullable<int> status_saisie { get; set; }
        public string amount_found { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> updated_at { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Historique_Saisie> Historique_Saisie { get; set; }
        public virtual SaisieType SaisieType { get; set; }
    }
}
