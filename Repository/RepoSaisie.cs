using MailService.Loggers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MailService.Repository
{
    public class RepoSaisie
    {
        // connection string for accessing the database
        private static CommitmentDBTestEntities db = null;

        // historique
        public static IEnumerable<FetchParticipant_Result> FetchPartcipant(bool accountSaisie)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    IEnumerable<FetchParticipant_Result> historique_Results = db.Database.SqlQuery<FetchParticipant_Result>("FetchParticipant @accountSaisie", new SqlParameter("@accountSaisie", accountSaisie)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        // save participant
        public static int SaveParticipant(List<Process> processes, int initiated_by, int header_id)
        {
            try
            {
                db = new CommitmentDBTestEntities();

                var workflow = new Workflow();

                // sequence of process
                int sequence = 2;
                // save decision
                foreach (var process in processes)
                {
                    // check whether compliance does not exists  among the approbators of the process

                    process.created_at = DateTime.Now;
                    process.updated_at = DateTime.Now;
                    process.isCompleted = 0;
                    process.initiated_by = initiated_by;
                    process.seq_no = sequence;
                    process.isActive = true;

                    db.Processes.Add(process);

                    // increment the sequence
                    sequence++;
                }

                // save workflow
                workflow.initiated_by = initiated_by;
                workflow.created_at = DateTime.Now;
                workflow.updated_at = DateTime.Now;
                workflow.status = 0;
                workflow.saisie_operation_header_id = header_id;
                workflow.Processes = processes;

                db.Workflows.Add(workflow);

                // get the customer id

                int res = db.SaveChanges();
                if (res > 0)
                {

                    //LogActivity("Approbateurs", "Enregistrement des participants dans la chaine de validation du client " + fullname.ToUpper(), DateTime.Now, initiated_by);

                    //return true;
                    return workflow.case_id;
                }
                else
                {

                    // log activity
                    //ControleurRepo.LogActivity("Approbateurs", "Erreur durant l'enregistrement des participants dans la chaine de validation du client " + fullname.ToUpper(), DateTime.Now, initiated_by);


                    return 0;
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while saving the participant into the database... Error message: " + ex.Message);
                return 0;
            }
        }

        // fetch header id list
        public static List<int> FetchPendingWorkflow(int role_id)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<int> historique_Results = db.Database.SqlQuery<int>("FetchPendingWorkFlow @role_id", new SqlParameter("@role_id", role_id)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }
        // fetch header id list
        public static List<int> FetchRejectedWorkflow(int user_id)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    List<int> historique_Results = db.Database.SqlQuery<int>("FetchRejectedWorkflow @user_id", new SqlParameter("@user_id", user_id)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }
        // fetch header id list
        public static List<FetchRoleMenus_Result> FetchRoleMenu(int role_id)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<FetchRoleMenus_Result> historique_Results = db.Database.SqlQuery<FetchRoleMenus_Result>("FetchRoleMenus @role_id", new SqlParameter("@role_id", role_id)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        public static List<FetchSubMenus_Result> FetchSubMenus()
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<FetchSubMenus_Result> historique_Results = db.Database.SqlQuery<FetchSubMenus_Result>("FetchSubMenus").ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        public static List<FetchMenu_Result> FetchMenus()
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<FetchMenu_Result> historique_Results = db.Database.SqlQuery<FetchMenu_Result>("FetchMenu").ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        public static List<FetchSaisieType_Result> FetchSaisieTypes()
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<FetchSaisieType_Result> historique_Results = db.Database.SqlQuery<FetchSaisieType_Result>("FetchSaisieType").ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        public static FetchWorkflowStepStatus_Result FetchWorkflowStepStatus(int header_id)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    FetchWorkflowStepStatus_Result historique_Results = db.Database.SqlQuery<FetchWorkflowStepStatus_Result>("FetchWorkflowStepStatus @saisie_operation_header_id", new SqlParameter("@saisie_operation_header_id", header_id)).FirstOrDefault();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }


        // fetch header id list
        public static List<FetchReportByUser_Result> FetchReportByUsers(int user_id)
        {
            try
            {
                using (CommitmentDBTestEntities db = new CommitmentDBTestEntities())
                {
                    List<FetchReportByUser_Result> historique_Results = db.Database.SqlQuery<FetchReportByUser_Result>("FetchReportByUser @user_id", new SqlParameter("@user_id", user_id)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }
    }
}