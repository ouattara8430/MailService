using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailService.Models
{
    public class CustomerInfo
    {
        // SIEGE_SOCIAL, RAISON_SOCIALE, REGISTRE_COMMERCE, CAPITAL_SOCIAL, LIBELLE, ETAT, GESTIONNAIRE
        public string customer_no { get; set; }
        public string fullname { get; set; }
        public string account_no { get; set; }
        public string branch_name { get; set; }
        public string phone_no { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string nature_piece { get; set; }
        public string no_piece { get; set; }
        
        // saisie & ATD
        public string numero_contribuable { get; set; }
        public string secteur_activite { get; set; }
        public string solde { get; set; }
        public string code_agence { get; set; }
        public string cle_rib { get; set; }
        public string compte_saisie { get; set; }

        //
        public string siege_social { get; set; }
        public string raison_sociale { get; set; }
        public string registre_commerce { get; set; }
        public string capital_social { get; set; }
        public string chapitre_comptable { get; set; }
        public string etat { get; set; }
        public string gestionnaire { get; set; }
        public string type_compte { get; set; }
        public string chapitre_code { get; set; }
    }
}