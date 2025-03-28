using MailService.Loggers;
using MailService.Models;
using MailService.Models.Request;
using MailService.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MailService.Service
{
    public class ServiceSaisie
    {
        // generate workflow
        public static string GenerateWorkflow(int header_id, int userID)
        {
            try
            {
                int caseID = 0;
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();

                // save the participant into the db
                var participants = RepoSaisie.FetchPartcipant(true);

                // process
                List<Process> processes = new List<Process>();


                // save decision
                foreach (var participant in participants)
                {
                    Process process = new Process
                    {
                        role_id = participant.role_id,
                        stepId = participant.step_id,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        isCompleted = 0,
                        initiated_by = userID
                    };
                    processes.Add(process);
                }

                // calling the method for saving the participant
                caseID = RepoSaisie.SaveParticipant(processes, userID, header_id);

                // fresh process initiated
                // get the current user process and associate the decision
                //var currentProcess = db.Processes.Where( x => x.initiated_by == userID && x.customerId == decision.customerID && x.isActive == true ).FirstOrDefault();
                var currentProcess = new Process();
                currentProcess.decision_id = 1;
                currentProcess.role_id = 1002;
                currentProcess.stepId = 1;
                currentProcess.updated_at = DateTime.Now;
                currentProcess.created_at = DateTime.Now;
                currentProcess.isCompleted = 1;
                currentProcess.next_user_status = 1;
                //currentProcess.seq_no = 2;
                currentProcess.seq_no = 1;
                currentProcess.isActive = true;
                currentProcess.case_id = caseID;
                currentProcess.userId = userID;
                currentProcess.initiated_by = userID;
                db.Processes.Add(currentProcess);

                int res = db.SaveChanges();
                if (res > 0)
                {

                    // set the path
                    //string path = ConfigurationManager.AppSettings["pending"].ToString();

                    //
                    string mail = string.Empty;
                    //
                    string email_body = string.Empty;

                    // fetch the full name of the initiator
                    string fullname = db.UserProfiles.Where(x => x.userId == userID).Select(c => c.first_name + " " + c.last_name).FirstOrDefault();


                    /* start sendind notification to the next approbator */
                    int final_case_id = caseID;
                    var nextApprobator = db.Processes.Where(x => x.isCompleted == 0 && (x.decision_id == 2 || x.decision_id == null) && x.isActive == true && x.case_id == final_case_id).OrderBy(x => x.seq_no).FirstOrDefault();

                    // set the status of next_user_status to 1
                    nextApprobator.next_user_status = 1;
                    db.Entry(nextApprobator).State = EntityState.Modified;
                    db.SaveChanges();

                    // 
                    string email_subject = string.Empty;

                    if (nextApprobator.userId == null)
                    {
                        // send mail to all participants with the same role
                        int next_role_id = db.Roles.Where(x => x.role_id == nextApprobator.role_id && x.isActive == true).FirstOrDefault().role_id;

                        var user_emails = db.UserProfiles.Where(x => x.role_id == next_role_id && x.isActive == true).Select(c => c.email).ToList();

                        string emails = string.Join(",", user_emails);



                        // format email body
                        //email_body = AccountService.FormatPendingNotification(path, "collegue", customer.intitule_compte, nextApprobator.Workflow.account_no, nextApprobator.updated_at ?? DateTime.Now, nextApprobator.Step.step_label, nextApprobator.case_id.ToString());
                        email_body = "Cher collegue, avis en attente a votre niveau....";

                        //email_subject = (decision.motif_id == null) ? "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE APRES REJET DU DOSSIER DU CLIENT " : "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE DU CLIENT ";
                        email_subject = "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE APRES REJET DU DOSSIER DU CLIENT ";

                        // send email
                        //mail = Mailer.SendEmailWithCopy( emails, "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE ", "Hello Team, this is to inform you that you have a pending request from " + fullname, null );
                        mail = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", email_subject, email_body);

                    }
                    else
                    {

                        // fetch the next approbator email
                        string email = db.Processes.Where(x => x.isCompleted == 0).OrderBy(x => x.seq_no).FirstOrDefault().UserProfile.email;


                        // format email body
                        //email_body = AccountService.FormatPendingNotification(path, nextApprobator.UserProfile1.first_name + " " + nextApprobator.UserProfile1.last_name, customer.intitule_compte, nextApprobator.Workflow.account_no, nextApprobator.updated_at ?? DateTime.Now, nextApprobator.Step.step_label, nextApprobator.case_id.ToString());
                        email_body = "Cher collegue, avis en attente a votre niveau....";

                        email_subject = "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE APRES REJET DU DOSSIER DU CLIENT ";

                        // send email
                        //mail = Mailer.SendEmailWithCopy( emails, "DEMANDE DE CONTROLE D'OUVERTURE DE COMPTE EN ATTENTE ", "Hello Team, this is to inform you that you have a pending request from " + fullname, null );
                        mail = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", email_subject, email_body);

                    }
                    if (mail.Contains("email sent"))
                    {
                        // update the next approbator status
                        //var next_approbator = db.Processes.Where( x => x.isCompleted == 0 ).OrderBy( x => x.role_id ).First();
                        //var next_approbator = db.Processes.Where( x => x.isCompleted == 0 && ( x.decision_id == 2 || x.decision_id == null ) ).OrderBy( x => x.role_id ).First();
                        var next_approbator = db.Processes.Where(x => x.isCompleted == 1 && (x.decision_id == 1 || x.decision_id == 4)).OrderByDescending(x => x.role_id).FirstOrDefault();
                        next_approbator.isCompleted = 1;
                        next_approbator.next_user_status = 0;
                        db.Entry(next_approbator).State = EntityState.Modified;
                    }

                    /* end */

                    //db.Entry( update_agent_status ).State = EntityState.Modified;
                    int update = db.SaveChanges();
                    // log activity

                    return "ok";
                }
                else
                {

                    // log activity


                    return "";
                }

            }
            catch (Exception ex)
            {
                return "";
            }

        }

        // evaluate saisie
        public static bool Validate(RequestValidateSaisie saisie)
        {
            try
            {
                string mail_agent_ouverture = string.Empty;
                string email_body = string.Empty;
                string mail = string.Empty;

                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                var currentProcess = db.Processes.Where(x => x.case_id == saisie.case_id && x.role_id == saisie.role_id).FirstOrDefault();
                currentProcess.decision_id = saisie.decision_id;
                if(saisie.decision_id == 1)
                    currentProcess.approve_comment = saisie.comment;
                else
                    currentProcess.reject_comment = saisie.comment;

                currentProcess.updated_at = DateTime.Now;
                currentProcess.userId = saisie.userId;
                currentProcess.isCompleted = 1;
                currentProcess.next_user_status = 1;

                db.Entry(currentProcess).State = EntityState.Modified;
                int res = db.SaveChanges();

                if (res > 0)
                {

                    // fetch customer informations

                    // fetch the next approbator email
                    /* start sendind notification to the next approbator */
                    var nextApprobator = db.Processes.Where(x => x.isCompleted == 0 && (x.decision_id == 2 || x.decision_id == null) && x.isActive == true && x.case_id == saisie.case_id).OrderBy(x => x.seq_no).FirstOrDefault();


                    // fetch the full name of the initiator
                    var initiator = db.UserProfiles.Where(x => x.userId == currentProcess.initiated_by && x.isActive == true).FirstOrDefault();

                    // check whether the process is completed
                    if (nextApprobator == null)
                    {
                        // update the case id
                        var workflow = db.Workflows.Where(x => x.case_id == currentProcess.case_id).FirstOrDefault();
                        workflow.status = 1;
                        db.Entry(workflow).State = EntityState.Modified;
                        db.SaveChanges();

                        // generate a report about the process
                        // fetch the case historique
                        string table_row = string.Empty;
                        string path = string.Empty;
                        int i = 0;
                        // fetch historique
                        var data = db.HistoriqueProcesses.Where(x => x.case_id == workflow.case_id).ToList();

                        // fetch customer name
                        int pv_case_id = workflow.case_id;
                        int saisie_operation_header_id = db.Workflows.Where(x => x.case_id == pv_case_id).FirstOrDefault().saisie_operation_header_id ?? 0;
                        string customer_name = (from c in db.SaisieOperationHeaders
                                                join w in db.Workflows
                                                on c.saisie_operation_header_id equals w.saisie_operation_header_id
                                                where w.case_id == pv_case_id
                                                select c.account_name
                            ).Distinct().FirstOrDefault();


                        email_body = (saisie.decision_id == 1) ? "Cher collegue, votre dossier a ete valide" : "Cher collegue, votre dossier a ete rejete";

                        //mail_agent_ouverture = Mailer.SendEmailWithCopy(initiator.email, "APPROBATION ET VALIDATION COMPLETE DU DOSSIER D'OUVERTURE DE COMPTE DU CLIENT " + customer.first_name.ToUpper() + " " + customer.last_name.ToUpper() + " par " + currentProcess.UserProfile1.first_name + " " + currentProcess.UserProfile1.last_name, email_body, null);
                        mail_agent_ouverture = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", (saisie.decision_id == 1) ? "APPROBATION " : "REJET " + " DU DOSSIER " + customer_name + " par " + currentProcess.UserProfile1.first_name + " " + currentProcess.UserProfile1.last_name, email_body);



                        // get the motif label
                        //var label_agent_ouverture = db.Motifs.Where( x => x.motif_id == decision.motif_id ).Select( c => c.motif_label ).FirstOrDefault();
                        var label_agent_ouverture = saisie.comment;
                        // log activity
                        //ControleurRepo.LogActivity("Decision de l'approbateur" + currentProcess.UserProfile1.first_name + " " + currentProcess.UserProfile1.last_name, "Decision: " + label_agent_ouverture + " pour le client " + customer.first_name + " " + customer.last_name, DateTime.Now, (int)Session["userId"]);

                        // generate historique

                        //return Json(new { flag = true, message = "Decision de compte enregistré aevc succès", pv = pv_doc }, JsonRequestBehavior.AllowGet);
                        return true;
                    }


                    // notify the initiator about the process approbation
                    email_body = "Cher collegue, votre dossier a ete valide.";
                    mail = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", (saisie.decision_id == 1) ? "APPROBATION " : "REJET " + " DU DOSSIER ", email_body);

                    // set the status of next_user_status to 1
                    nextApprobator.next_user_status = 1;
                    db.Entry(nextApprobator).State = EntityState.Modified;
                    db.SaveChanges();

                    // pending notification
                    if (nextApprobator.userId == null)
                    {
                        // format the email body
                        email_body = "Ceci est un test";


                        // send mail to all participants with the same role
                        int next_role_id = db.Roles.Where(x => x.role_id == nextApprobator.role_id && x.isActive == true).FirstOrDefault().role_id;
                        if (next_role_id == 0)
                        {
                            return false;
                        }
                        var user_emails = db.UserProfiles.Where(x => x.role_id == next_role_id && x.isActive == true).Select(c => c.email).ToList();

                        string emails = string.Join(",", user_emails);


                        mail_agent_ouverture = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", "DEMANDE DE CONTROLE", email_body);
                    }
                    else
                    {

                        // format the email body
                        email_body = "Ceci est un test";

                        // fetch the next approbator email
                        string email = db.Processes.Where(x => x.isCompleted == 0 && x.case_id == saisie.case_id).OrderBy(x => x.seq_no).FirstOrDefault().UserProfile.email;
                        // send email
                        // send notification to the next approbator
                        string mail_next = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", "DEMANDE DE CONTROLE", email_body);

                    }

                    // get the motif label
                    //var label = db.Motifs.Where( x => x.motif_id == decision.motif_id ).Select( c => c.motif_label ).FirstOrDefault();

                    var label = saisie.comment;
                    // log activity
                    //ControleurRepo.LogActivity("Decision de l'approbateur" + currentProcess.UserProfile.first_name + " " + currentProcess.UserProfile.last_name, "Decision: " + label + " pour le client " + customer.first_name + " " + customer.last_name, DateTime.Now, (int)Session["userId"]);

                    return true;
                }

                return false;
            } catch(Exception ex)
            {
                return false;
            }
        }
        
        // evaluate saisie
        public static bool Reject(RequestValidateSaisie saisie)
        {
            try
            {
                string mail_agent_ouverture = string.Empty;
                string email_body = string.Empty;
                string mail = string.Empty;

                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                var currentProcess = db.Processes.Where(x => x.case_id == saisie.case_id && x.role_id == saisie.role_id).FirstOrDefault();
                currentProcess.decision_id = saisie.decision_id;
                if(saisie.decision_id == 1)
                    currentProcess.approve_comment = saisie.comment;
                else
                    currentProcess.reject_comment = saisie.comment;

                currentProcess.updated_at = DateTime.Now;
                currentProcess.userId = saisie.userId;
                currentProcess.isCompleted = 0;
                currentProcess.next_user_status = 0;

                db.Entry(currentProcess).State = EntityState.Modified;
                int res = db.SaveChanges();

                if (res > 0)
                {

                    // fetch customer informations

                    // fetch the next approbator email
                    /* start sendind notification to the next approbator */
                    var updateInitiatorProcess = db.Processes.Where(x => x.userId == currentProcess.initiated_by && x.isActive == true && x.case_id == saisie.case_id).FirstOrDefault();
                    updateInitiatorProcess.isCompleted = 0;
                    updateInitiatorProcess.decision_id = 0;
                    updateInitiatorProcess.next_user_status = 1;
                    //updateInitiatorProcess.decision_id = 2;
                    db.Entry(updateInitiatorProcess).State = EntityState.Modified;

                    // update the worlflow status
                    var workflow = db.Workflows.Where(x => x.case_id == updateInitiatorProcess.case_id).FirstOrDefault();
                    workflow.status = 2;
                    db.Entry(workflow).State = EntityState.Modified;

                    db.SaveChanges();
                    // fetch the full name of the initiator
                    var initiator = db.UserProfiles.Where(x => x.userId == currentProcess.initiated_by && x.isActive == true).FirstOrDefault();

                    

                    // get the motif label
                    //var label = db.Motifs.Where( x => x.motif_id == decision.motif_id ).Select( c => c.motif_label ).FirstOrDefault();

                    var label = saisie.comment;
                    // notify the initiator about the process approbation
                    email_body = "Cher collegue, votre dossier a ete rejete.";
                    mail = Mailer.SendEmail("ismael.ouattara@afrilandfirstbankgroup.com", (saisie.decision_id == 1) ? "APPROBATION " : "REJET " + " DU DOSSIER ", email_body);

                    return true;
                }

                return false;
            } catch(Exception ex)
            {
                return false;
            }
        }

    }
}