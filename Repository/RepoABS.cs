using MailService.Loggers;
using MailService.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace MailService.Repository
{
    public class RepoABS
    {

        private static string ConString = ConfigurationManager.ConnectionStrings["ConnexionOracle"].ToString();

        // Initialize connection with Oracle DB
        public static OracleConnection GetConnection()
        {

            try
            {
                string myConnectionString = ConString;
                DataSet ds = new DataSet();
                OracleConnection myConnection = new OracleConnection(myConnectionString);
                myConnection.Open();
                return myConnection;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // fetch customer basic information
        public static CustomerInfo FetchCustomerBasinInfo(string customer_no)
        {
            try
            {
                string myConnectionString = ConString;
                OracleConnection oracon = new OracleConnection(myConnectionString);

                // queries
                //string query = "select CLIENT_INTITULE, CLIENT_ADRESSE, CLIENT_TELEPHONE1, CLIENT_EMAILCLIE from bq_ad_client where CLIENT_CODE = '" + customer_no + "'";
                //string query = "SELECT CLIENT_INTITULE, CLIENT_ADRESSE, CLIENT_TELEPHONE1, CLIENT_EMAILCLIE, pers.PERSONNE_NIMPIECEID \"CNI\" from bq_ad_client client inner join bq_ad_cltprphysq phy on client.client_id = phy.client_id inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID inner join bq_ad_parametre pa on pers.PROFESSION_ID=pa.ID where CLIENT.CLIENT_CODE = '" + customer_no + "'";
                string query = "SELECT CLIENT_INTITULE, CLIENT_ADRESSE, CLIENT_TELEPHONE1, CLIENT_EMAILCLIE, case client.CLIENT_TYPE when 'P'  then (SELECT pa.PARAMETRE_LIBELLE from bq_ad_client client1 inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID inner join bq_ad_parametre pa on PERS.TYPEPIECE_ID = pa.ID WHERE client1.CLIENT_ID = client.CLIENT_ID)when 'M'  then 'REGISTRE DE COMMERCE' END  \"NATURE_PIECE\",case client.CLIENT_TYPE when 'M'  then (SELECT PHY.CLTPRMORAL_REGISTCOM from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID =  client.CLIENT_ID)when 'P'  then (SELECT PERS.PERSONNE_NIMPIECEID from bq_ad_client client1 inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID inner join bq_ad_parametre pa on pers.PROFESSION_ID=pa.ID WHERE client1.CLIENT_ID = client.CLIENT_ID)END \"PIECE\" from bq_ad_client client where CLIENT.CLIENT_CODE = '" + customer_no + "'";


                DataSet ds = new DataSet();
                // fetch decouvert encours, douteux et litigeux
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query;
                cmd.Connection = oracon;
                oracon.Open();

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.ReturnProviderSpecificTypes = true;

                OracleDataReader dr = cmd.ExecuteReader();
                CustomerInfo info = null;

                if (dr.HasRows)
                {
                    // initialize
                    dr.Read();

                    info = new CustomerInfo
                    {
                        fullname = dr["CLIENT_INTITULE"].ToString(),
                        address = dr["CLIENT_ADRESSE"].ToString(),
                        phone_no = dr["CLIENT_TELEPHONE1"].ToString(),
                        email = dr["CLIENT_EMAILCLIE"].ToString(),
                        nature_piece = dr["NATURE_PIECE"].ToString(),
                        no_piece = dr["PIECE"].ToString(),

                    };
                }


                //return data;
                return info;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // fetch customer full name
        public static List<CustomerInfo> FetchABSCustomerFullName(string full_name)
        {
            try
            {
                //string myConnectionString = configuration.GetSection("ConnectionStrings").GetSection("OracleConString").Value;
                string myConnectionString = ConString;
                OracleConnection oracon = new OracleConnection(myConnectionString);


                LogWriter.LogWrite("Fetching all the customer data from ABS...");

                //
                //string query_customer_info = "select CLIENT_INTITULE, CLIENT_CODE from bq_ad_client where CLIENT_INTITULE like '%" + full_name.ToUpper() + "%'";
                //string query_customer_info = "select case cl.CLIENT_TYPE when 'M' then (SELECT PHY.CHIFFRE_AFFAIRE_HT from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then 0 END \"CAPITAL_SOCIAL\",  case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_REGISTCOM from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"REGISTRE_COMMERCE\", case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_RAISONSOCI from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"RAISON_SOCIALE\", case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_SIEGE from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"SIEGE_SOCIAL\",Y.CODE CODE_AGENCE, case cl.CLIENT_TYPE when 'P'  then (SELECT PA.PARAMETRE_LIBELLE from bq_ad_client client1                inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id                inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID                inner join bq_ad_parametre pa on pers.typepiece_id=pa.ID                WHERE client1.CLIENT_ID = Y.CLIENT                                )when 'M'  then '' END  \"NATURE_PIECE\", case cl.CLIENT_TYPE when 'P'  then (SELECT pers.PERSONNE_NIMPIECEID from bq_ad_client client1                inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id                inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID                inner join bq_ad_parametre pa on pers.PROFESSION_ID=pa.ID                WHERE client1.CLIENT_ID = Y.CLIENT                                )when 'M'  then '' END  \"NUMERO_PIECE\",   Y.AGENCE,   Y.NUM_COMPTE,   Y.CLE,   Y.INTITULE,   Y.NUM_CLIENT,   Y.TYPE_CLIENT,   Y.CHAPITRE_COMPTABLE,   Y.LIBELLE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"ACTIVITES_PERS_PHYSIQUE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       decode(        PERS.PERSONNE_SEXE, 0, 'HOMME', 'FEMME'      )     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"GENRE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       pa.parametre_code || '-' || pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id       inner join bq_ad_parametre pa on PHY.FORMJURIDI_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"FORME_JURIDIQUE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CHIFFRE_AFFAIRE_HT     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then 0 END \"CHIFFRE_AFFAIRE_HT\",   action.PARAMETRE_LIBELLE \"CATEGORIE\",   cl.CLIENT_TELEPHONE1,   cl.CLIENT_TELEPHONE2,   cl.CLIENT_TELEPHONE3,   cl.CLIENT_EMAILCLIE \"EMAIL\",   Y.GESTIONAIR_CODE,   Y.GESTIONNAIRE \"GESTIONNAIRE\",   CLIENT_ADRESSE,   CLIENT_ADRESSE2,   Y.DTE_LAST_DEB \"DATE DERN DEBIT\",   Y.DTE_LAST_CRE \"DATE DERN CREDIT\",   Y.DTE_OUV_COMPTE \"DATE_OUVERTURE_COMPTE\",   Y.SOLDE \"SOLDE\",   Y.SOLDE_JOUR,   case Y.ETAT when 0 then 'En cours de validation' when 1 then 'Validé' when 2 then 'En cours de fermeture' when 3 then 'Fermé' end ETA_COMPTE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATENAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUNAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMPERE || ' ' || PHYS.CLTPRPHY_PRENOMPERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_PERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMMERE || ' ' || PHYS.CLTPRPHY_PRENOMMERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_MERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_EMPLOYEUR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"EMPLOYEUR\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_NIMPIECEID     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NUMERO_PIECE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_DELIVRANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEEXPIR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_EXPIRATION\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_DELIVRANCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_RAISONSOCI     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"RAISON_SOCIALE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_SIEGE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"SIEGE_SOCIAL\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_REGISTCOM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_DATERCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_LIEURCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"LIEU_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_NUMCONTRIB     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"NUMERO_CONTRIBUABLE\" from   (    SELECT       A.AGENCE_CODE CODE,       A.AGENCE_NOM AGENCE,       C.COMPTE_ID COMPTE,       C.COMPTE_NUMCOMPTE NUM_COMPTE,       C.COMPTE_CLECOMPTE CLE,       C.COMPTE_INTITULE INTITULE,       CLI.CLIENT_ID CLIENT,       cli.CLIENT_CODE NUM_CLIENT,       C.COMPTE_ETATCOMPTE ETAT,       cli.CLIENT_TYPE TYPE_CLIENT,       H.CHAPITRE_CODE CHAPITRE_COMPTABLE,       h.CHAPITRE_LIBELLE LIBELLE,       C.COMPTE_DTELASTCRE DTE_LAST_CRE,       C.COMPTE_DTEOU DTE_OUV_COMPTE,       C.COMPTE_DTELASTDEB DTE_LAST_DEB,       C.COMPTE_SLDCO SOLDE,       C.COMPTE_SLDJR SOLDE_JOUR,       I.GESTIONAIR_CODE,       I.GESTIONAIR_NOM GESTIONNAIRE     FROM       BQ_AD_COMPTE C       INNER JOIN BQ_AD_AGENCE A ON C.COMPTE_AGENCEID = A.AGENCE_ID       INNER JOIN BQ_AD_CLIENT cli on C.COMPTE_CLIENTID = CLI.CLIENT_ID       INNER JOIN BQ_AD_CHAPITRE H ON C.COMPTE_CHAPID = H.CHAPITRE_ID       AND h.chapitre_nature = 0       INNER JOIN BQ_AD_GESTIONAIR I ON C.COMPTE_GESTID = I.GESTIONAIR_ID     WHERE       C.COMPTE_INTITULE like '%" + full_name + "%'  AND cli.CLIENT_IDPROFILE<>'3942'   ) Y   inner join bq_ad_client cl on Y.CLIENT = CL.CLIENT_ID   left join bq_ad_parametre action on cl.SECTEURACT_ID = action.id   and action.typeparam_code = '109'";
                string query_customer_info = "select Y.TYPE_COMPTE, case cl.CLIENT_TYPE when 'M' then (SELECT PHY.CHIFFRE_AFFAIRE_HT from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then 0 END \"CAPITAL_SOCIAL\",  case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_NUMENREG from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"REGISTRE_COMMERCE\", case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_RAISONSOCI from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"RAISON_SOCIALE\", case cl.CLIENT_TYPE when 'M' then ( SELECT PHY.CLTPRMORAL_SIEGE from bq_ad_client client1 inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id WHERE client1.CLIENT_ID = Y.CLIENT ) when 'P' then '' END \"SIEGE_SOCIAL\",Y.CODE CODE_AGENCE, case cl.CLIENT_TYPE when 'P'  then (SELECT PA.PARAMETRE_LIBELLE from bq_ad_client client1                inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id                inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID                inner join bq_ad_parametre pa on pers.typepiece_id=pa.ID                WHERE client1.CLIENT_ID = Y.CLIENT                                )when 'M'  then '' END  \"NATURE_PIECE\", case cl.CLIENT_TYPE when 'P'  then (SELECT pers.PERSONNE_NIMPIECEID from bq_ad_client client1                inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id                inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE=pers.PERSONNE_ID                inner join bq_ad_parametre pa on pers.PROFESSION_ID=pa.ID                WHERE client1.CLIENT_ID = Y.CLIENT                                )when 'M'  then '' END  \"NUMERO_PIECE\",   Y.AGENCE,   Y.NUM_COMPTE,   Y.CLE,   Y.INTITULE,   Y.NUM_CLIENT,   Y.TYPE_CLIENT,   Y.CHAPITRE_COMPTABLE,   Y.LIBELLE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"ACTIVITES_PERS_PHYSIQUE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       decode(        PERS.PERSONNE_SEXE, 0, 'HOMME', 'FEMME'      )     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"GENRE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       pa.parametre_code || '-' || pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id       inner join bq_ad_parametre pa on PHY.FORMJURIDI_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"FORME_JURIDIQUE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CHIFFRE_AFFAIRE_HT     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then 0 END \"CHIFFRE_AFFAIRE_HT\",   action.PARAMETRE_LIBELLE \"CATEGORIE\",   cl.CLIENT_TELEPHONE1,   cl.CLIENT_TELEPHONE2,   cl.CLIENT_TELEPHONE3,   cl.CLIENT_EMAILCLIE \"EMAIL\",   Y.GESTIONAIR_CODE,   Y.GESTIONNAIRE \"GESTIONNAIRE\",   CLIENT_ADRESSE,   CLIENT_ADRESSE2,   Y.DTE_LAST_DEB \"DATE DERN DEBIT\",   Y.DTE_LAST_CRE \"DATE DERN CREDIT\",   Y.DTE_OUV_COMPTE \"DATE_OUVERTURE_COMPTE\",   Y.SOLDE \"SOLDE\",   Y.SOLDE_JOUR,   case Y.ETAT when 0 then 'En cours de validation' when 1 then 'Validé' when 2 then 'En cours de fermeture' when 3 then 'Fermé' end ETA_COMPTE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATENAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUNAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMPERE || ' ' || PHYS.CLTPRPHY_PRENOMPERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_PERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMMERE || ' ' || PHYS.CLTPRPHY_PRENOMMERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_MERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_EMPLOYEUR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"EMPLOYEUR\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_NIMPIECEID     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NUMERO_PIECE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_DELIVRANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEEXPIR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_EXPIRATION\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_DELIVRANCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_RAISONSOCI     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"RAISON_SOCIALE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_SIEGE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"SIEGE_SOCIAL\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_NUMENREG     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_DATERCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_LIEURCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"LIEU_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_NUMCONTRIB     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"NUMERO_CONTRIBUABLE\" from   (    SELECT       A.AGENCE_CODE CODE,       A.AGENCE_NOM AGENCE,       C.COMPTE_ID COMPTE,       C.COMPTE_NUMCOMPTE NUM_COMPTE,       C.COMPTE_CLECOMPTE CLE,       C.COMPTE_INTITULE INTITULE,       CLI.CLIENT_ID CLIENT,       cli.CLIENT_CODE NUM_CLIENT,       C.COMPTE_ETATCOMPTE ETAT,       cli.CLIENT_TYPE TYPE_CLIENT,       H.CHAPITRE_CODE CHAPITRE_COMPTABLE,       h.CHAPITRE_LIBELLE LIBELLE,       C.COMPTE_DTELASTCRE DTE_LAST_CRE,       C.COMPTE_DTEOU DTE_OUV_COMPTE,       C.COMPTE_DTELASTDEB DTE_LAST_DEB,       C.COMPTE_SLDCO SOLDE,       C.COMPTE_SLDJR SOLDE_JOUR,       I.GESTIONAIR_CODE,       I.GESTIONAIR_NOM GESTIONNAIRE,TYPECPTE.TYPECOMPTE_CODE ||' - '|| TYPECPTE.TYPECOMPTE_LIBELLE \"TYPE_COMPTE\"     FROM       BQ_AD_COMPTE C       INNER JOIN BQ_AD_AGENCE A ON C.COMPTE_AGENCEID = A.AGENCE_ID       INNER JOIN BQ_AD_CLIENT cli on C.COMPTE_CLIENTID = CLI.CLIENT_ID       INNER JOIN BQ_AD_CHAPITRE H ON C.COMPTE_CHAPID = H.CHAPITRE_ID        INNER JOIN BQ_AD_GESTIONAIR I ON C.COMPTE_GESTID = I.GESTIONAIR_ID INNER JOIN BQ_AD_TYPECOMPTE TYPECPTE ON C.COMPTE_TYPECPTID = TYPECPTE.TYPECOMPTE_ID     WHERE       C.COMPTE_INTITULE like '%" + full_name + "%'  AND cli.CLIENT_IDPROFILE<>'3942'   ) Y   inner join bq_ad_client cl on Y.CLIENT = CL.CLIENT_ID   left join bq_ad_parametre action on cl.SECTEURACT_ID = action.id   and action.typeparam_code = '109'";

                // variables
                List<CustomerInfo> customers = new List<CustomerInfo>();


                // fetch decouvert encours, douteux et litigeux
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query_customer_info;
                cmd.Connection = oracon;
                oracon.Open();

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.ReturnProviderSpecificTypes = true;


                OracleDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {

                    while (dr.Read())
                    {
                        customers.Add(new CustomerInfo
                        {
                            fullname = dr["INTITULE"].ToString(),
                            customer_no = dr["NUM_CLIENT"].ToString(),
                            numero_contribuable = dr["NUMERO_CONTRIBUABLE"].ToString(),
                            secteur_activite = dr["ACTIVITES_PERS_PHYSIQUE"].ToString(),
                            solde = dr["SOLDE"].ToString(),
                            account_no = dr["NUM_COMPTE"].ToString(),
                            branch_name = dr["AGENCE"].ToString(),
                            code_agence = dr["CODE_AGENCE"].ToString(),
                            cle_rib = dr["CLE"].ToString(),
                            phone_no = dr["CLIENT_TELEPHONE1"].ToString(),
                            address = dr["CLIENT_ADRESSE"].ToString(),
                            email = dr["EMAIL"].ToString(),
                            nature_piece = dr["NATURE_PIECE"].ToString(), // SIEGE_SOCIAL, RAISON_SOCIALE, REGISTRE_COMMERCE, CAPITAL_SOCIAL, LIBELLE, ETAT, GESTIONNAIRE, TYPE_COMPTE
                            no_piece = dr["NUMERO_PIECE"].ToString(),
                            siege_social = dr["SIEGE_SOCIAL"].ToString(),
                            raison_sociale = dr["RAISON_SOCIALE"].ToString(),
                            registre_commerce = dr["REGISTRE_COMMERCE"].ToString(),
                            capital_social = dr["CAPITAL_SOCIAL"].ToString(),
                            chapitre_comptable = dr["LIBELLE"].ToString(),
                            etat = dr["ETA_COMPTE"].ToString(),
                            gestionnaire = dr["GESTIONNAIRE"].ToString(),
                            type_compte = dr["TYPE_COMPTE"].ToString(),
                            chapitre_code = dr["CHAPITRE_COMPTABLE"].ToString(),
                        });
                    }
                    LogWriter.LogWrite("Search of the following name: " + full_name + " \nNo. of instance: " + customers.Count);
                }
                else
                {
                    return null;
                }

                return customers;
            }
            catch (Exception ex)
            {
                //ErrorLog.LogWrite("Error while fetching customer informations... Error message: " + ex.Message);
                return null;
            }
        }

        // get the account saisie of the customer
        public static List<CustomerInfo> FetchABSCustomerSaisieAccount(string customer_no)
        {
            try
            {
                //string myConnectionString = configuration.GetSection("ConnectionStrings").GetSection("OracleConString").Value;
                string myConnectionString = ConString;
                OracleConnection oracon = new OracleConnection(myConnectionString);


                LogWriter.LogWrite("Fetching all the customer data from ABS...");

                //
                //string query_customer_info = "select CLIENT_INTITULE, CLIENT_CODE from bq_ad_client where CLIENT_INTITULE like '%" + full_name.ToUpper() + "%'";
                string query_customer_info = "select   Y.CODE CODE_AGENCE,   Y.AGENCE,   Y.NUM_COMPTE,   Y.CLE,   Y.INTITULE,   Y.NUM_CLIENT,   Y.TYPE_CLIENT,   Y.CHAPITRE_COMPTABLE,   Y.LIBELLE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"ACTIVITES_PERS_PHYSIQUE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       decode(        PERS.PERSONNE_SEXE, 0, 'HOMME', 'FEMME'      )     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then '' END \"GENRE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       pa.parametre_code || '-' || pa.PARAMETRE_LIBELLE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id       inner join bq_ad_parametre pa on PHY.FORMJURIDI_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"FORME_JURIDIQUE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CHIFFRE_AFFAIRE_HT     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then 0 END \"CHIFFRE_AFFAIRE_HT\",   action.PARAMETRE_LIBELLE \"CATEGORIE\",   cl.CLIENT_TELEPHONE1,   cl.CLIENT_TELEPHONE2,   cl.CLIENT_TELEPHONE3,   cl.CLIENT_EMAILCLIE \"EMAIL\",   Y.GESTIONAIR_CODE,   Y.GESTIONNAIRE \"GESTIONNAIRE\",   CLIENT_ADRESSE,   CLIENT_ADRESSE2,   Y.DTE_LAST_DEB \"DATE DERN DEBIT\",   Y.DTE_LAST_CRE \"DATE DERN CREDIT\",   Y.DTE_OUV_COMPTE \"DATE_OUVERTURE_COMPTE\",   Y.SOLDE \"SOLDE\",   Y.SOLDE_JOUR,   case Y.ETAT when 0 then 'En cours de validation' when 1 then 'Validé' when 2 then 'En cours de fermeture' when 3 then 'Fermé' end ETA_COMPTE,   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATENAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUNAISS     from       bq_ad_client client1       inner join bq_ad_cltprphysq phy on client1.client_id = phy.client_id       inner join bq_ad_personne pers on phy.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_NAISSANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMPERE || ' ' || PHYS.CLTPRPHY_PRENOMPERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_PERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_NOMMERE || ' ' || PHYS.CLTPRPHY_PRENOMMERE     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NOM_MERE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PHYS.CLTPRPHY_EMPLOYEUR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"EMPLOYEUR\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_NIMPIECEID     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"NUMERO_PIECE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_DELIVRANCE\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_DATEEXPIR     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_EXPIRATION\",   case cl.CLIENT_TYPE when 'P' then (    SELECT       PERS.PERSONNE_LIEUDELIV     from       bq_ad_client client1       inner join bq_ad_cltprphysq phys on client1.client_id = phys.client_id       inner join bq_ad_personne pers on PHYS.CLTPRPHY_IDPERSONNE = pers.PERSONNE_ID       inner join bq_ad_parametre pa on pers.PROFESSION_ID = pa.ID     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'M' then TO_DATE('', 'DD/MM/YYYY') END \"LIEU_DELIVRANCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_RAISONSOCI     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"RAISON_SOCIALE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_SIEGE     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"SIEGE_SOCIAL\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_REGISTCOM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_DATERCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then TO_DATE('', 'DD/MM/YYYY') END \"DATE_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_LIEURCCM     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"LIEU_REGISTRE_COMMERCE\",   case cl.CLIENT_TYPE when 'M' then (    SELECT       PHY.CLTPRMORAL_NUMCONTRIB     from       bq_ad_client client1       inner join bq_ad_cltprmoral phy on client1.client_id = phy.client_id     WHERE       client1.CLIENT_ID = Y.CLIENT  ) when 'P' then '' END \"NUMERO_CONTRIBUABLE\" from   (    SELECT       A.AGENCE_CODE CODE,       A.AGENCE_NOM AGENCE,       C.COMPTE_ID COMPTE,       C.COMPTE_NUMCOMPTE NUM_COMPTE,       C.COMPTE_CLECOMPTE CLE,       C.COMPTE_INTITULE INTITULE,       CLI.CLIENT_ID CLIENT,       cli.CLIENT_CODE NUM_CLIENT,       C.COMPTE_ETATCOMPTE ETAT,       cli.CLIENT_TYPE TYPE_CLIENT,       H.CHAPITRE_CODE CHAPITRE_COMPTABLE,       h.CHAPITRE_LIBELLE LIBELLE,       C.COMPTE_DTELASTCRE DTE_LAST_CRE,       C.COMPTE_DTEOU DTE_OUV_COMPTE,       C.COMPTE_DTELASTDEB DTE_LAST_DEB,       C.COMPTE_SLDCO SOLDE,       C.COMPTE_SLDJR SOLDE_JOUR,       I.GESTIONAIR_CODE,       I.GESTIONAIR_NOM GESTIONNAIRE     FROM       BQ_AD_COMPTE C       INNER JOIN BQ_AD_AGENCE A ON C.COMPTE_AGENCEID = A.AGENCE_ID       INNER JOIN BQ_AD_CLIENT cli on C.COMPTE_CLIENTID = CLI.CLIENT_ID       INNER JOIN BQ_AD_CHAPITRE H ON C.COMPTE_CHAPID = H.CHAPITRE_ID         INNER JOIN BQ_AD_GESTIONAIR I ON C.COMPTE_GESTID = I.GESTIONAIR_ID     WHERE  CLI.CLIENT_CODE = '" + customer_no + "'  AND H.CHAPITRE_CODE = '251100015'  ) Y   inner join bq_ad_client cl on Y.CLIENT = CL.CLIENT_ID   left join bq_ad_parametre action on cl.SECTEURACT_ID = action.id   and action.typeparam_code = '109'";

                // variables
                List<CustomerInfo> customers = new List<CustomerInfo>();


                // fetch decouvert encours, douteux et litigeux
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query_customer_info;
                cmd.Connection = oracon;
                oracon.Open();

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                da.ReturnProviderSpecificTypes = true;


                OracleDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {

                    while (dr.Read())
                    {
                        customers.Add(new CustomerInfo
                        {
                            fullname = dr["INTITULE"].ToString(),
                            customer_no = dr["NUM_CLIENT"].ToString(),
                            numero_contribuable = dr["NUMERO_CONTRIBUABLE"].ToString(),
                            secteur_activite = dr["ACTIVITES_PERS_PHYSIQUE"].ToString(),
                            solde = dr["SOLDE"].ToString(),
                            compte_saisie = dr["NUM_COMPTE"].ToString(),
                            branch_name = dr["AGENCE"].ToString(),
                            code_agence = dr["CODE_AGENCE"].ToString(),
                            cle_rib = dr["CLE"].ToString(),
                        });
                    }
                    LogWriter.LogWrite("Search of the following customer no: " + customer_no + " \nNo. of instance: " + customers.Count);
                }
                else
                {
                    return null;
                }

                return customers;
            }
            catch (Exception ex)
            {
                //ErrorLog.LogWrite("Error while fetching customer informations... Error message: " + ex.Message);
                return new List<CustomerInfo>();
            }
        }
    }
}