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
    
    public partial class submenu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public submenu()
        {
            this.role_submenu = new HashSet<role_submenu>();
        }
    
        public int idsubmenu { get; set; }
        public string path { get; set; }
        public string title { get; set; }
        public string iconType { get; set; }
        public string icon { get; set; }
        public int identity { get; set; }
        public Nullable<bool> rupture { get; set; }
        public int idmenu { get; set; }
        public int rang { get; set; }
        public Nullable<bool> iTrue { get; set; }
        public Nullable<bool> ismenu { get; set; }
    
        public virtual menu menu { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<role_submenu> role_submenu { get; set; }
    }
}
