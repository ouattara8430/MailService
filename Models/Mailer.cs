using MailService.Loggers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace MailService.Models
{
    public class Mailer
    {
        // send email with attachment
        public static string SendEmail(string to, string subject, string emailBody)
        {
            try
            {
                //to = ConfigurationManager.AppSettings["to"].ToString(); //To address    
                //to = to; //To address    
                string from = ConfigurationManager.AppSettings["from"].ToString(); //From address
                string smtp = ConfigurationManager.AppSettings["smtp"].ToString(); //smtp
                int port = Int32.Parse(ConfigurationManager.AppSettings["port"].ToString()); //port
                MailMessage message = new MailMessage(from, to);

                //string mailbody = "<b>This is a test from OUATTARA host.</b>";
                string mailbody = emailBody;
                //mailbody = emailBody;
                message.Subject = subject;
                // html true
                message.IsBodyHtml = true;
                message.Body = mailbody;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                SmtpClient client = new SmtpClient(smtp, port); //Afriland SMTP    
                System.Net.NetworkCredential basicCredential1 = new
                System.Net.NetworkCredential(from, ConfigurationManager.AppSettings["password"].ToString());
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicCredential1;
                client.Send(message);
                return "email sent from " + from + " to " + to;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to send email. Error : " + ex);
                return "Error while sending email";
            }
        }

        // send email with attachment
        public static string SendEmail(string to, string subject, string emailBody, string cc, string attachmentPath)
        {
            try
            {
                //to = ConfigurationManager.AppSettings["to"].ToString(); //To address    
                //to = to; //To address    
                //attachmentPath = ConfigurationManager.AppSettings["attachment"].ToString();
                string from = ConfigurationManager.AppSettings["from"].ToString(); //From address
                string smtp = ConfigurationManager.AppSettings["smtp"].ToString(); //smtp
                //cc = ConfigurationManager.AppSettings["copy"].ToString(); //smtp
                int port = Int32.Parse(ConfigurationManager.AppSettings["port"].ToString()); //port
                MailMessage message = new MailMessage(from, to);

                //string mailbody = "<b>This is a test from OUATTARA host.</b>";
                string mailbody = emailBody;
                //mailbody = emailBody;
                message.Subject = subject;
                // html true
                message.CC.Add(cc);
                message.IsBodyHtml = true;
                message.Body = mailbody;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                if(attachmentPath != null)
                {
                    message.Attachments.Add(new Attachment(attachmentPath));
                }
                SmtpClient client = new SmtpClient(smtp, port); //Gmail smtp    
                System.Net.NetworkCredential basicCredential1 = new
                System.Net.NetworkCredential(from, ConfigurationManager.AppSettings["password"].ToString());
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicCredential1;
                client.Send(message);
                return "email sent from " + from + " to " + to;
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return "Error while sending email";
            }
        }

        public static string SendEmail()
        {
            string to = ConfigurationManager.AppSettings["to"].ToString(); //To address    
            string from = ConfigurationManager.AppSettings["from"].ToString(); //From address
            string smtp = ConfigurationManager.AppSettings["smtp"].ToString(); //smtp
            int port = Int32.Parse(ConfigurationManager.AppSettings["port"].ToString()); //port
            MailMessage message = new MailMessage(from, to);

            string mailbody = "This is a test from OUATTARA host.";
            message.Subject = "test afriland email";
            // html true
            message.IsBodyHtml = true;
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(smtp, port); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential(from, ConfigurationManager.AppSettings["password"].ToString());
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
                return "email sent from " + from + " to " + to;
            }

            catch (Exception ex)
            {
                throw ex;
                return "Erreur " + ex.Message;
            }
        }

        public static string SendToAccountOfficer(string status, string file, AlertRequest request, int occurence_total)
        {
            try
            {
                // fetch the loan details
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                var loan = db.LoanApplications.Where(x => x.application_id == request.application_id).FirstOrDefault();

                // get the account officer details
                var acct_officer = db.UserProfiles.Where(x => x.userId == request.executed_by).FirstOrDefault();

                // log the mail format
                LogWriter.LogWrite("Mail details => Numero dossier: " + loan.application_id + "\nNumero compte: " + loan.NUMERO_COMPTE + "\nCle rib: " + loan.CLE_COMPTE +
                    "\nGestionnaire: " + acct_officer.first_name + " " + acct_officer.last_name + "\nEmail: " + acct_officer.email +
                    "\nNom du responsable gestionnaire: " + acct_officer.responsable_fullname + "\nEmail du responsable gestionnaire: " + acct_officer.responsable_email);


                string email_body = string.Empty;

                // format amount
                string amount = FormatAmount(Decimal.Parse(loan.IMPAYES_CREDIT.ToString()));
                amount = FormatAmount(Decimal.Parse(loan.MONTANT_DEBLOQUE.ToString()));

                email_body = File.ReadAllText(file);
                email_body = email_body.Replace("{{customer_name}}", loan.customer_fullname); //Intitule du compte
                email_body = email_body.Replace("{{account_no}}", loan.NUMERO_COMPTE); //Numéro de compte
                email_body = email_body.Replace("{{rib_key}}", loan.CLE_COMPTE); // cle rib
                email_body = email_body.Replace("{{loan_number}}", loan.application_id); //No Prêt
                //email_body = email_body.Replace("{{amount_due_date}}", loan.IMPAYES_CREDIT.ToString()); //Montant pret
                email_body = email_body.Replace("{{amount_due_date}}", amount); //Montant pret
                email_body = email_body.Replace("{{financing_type}}", loan.TYPE_ENGAGEMENT); //Objet financement
                email_body = email_body.Replace("{{actions}}", request.decisions); //Action 
                email_body = email_body.Replace("{{due_date}}", request.due_date_action.Value.ToString("dd/MM/yyyy")); //Échéance 
                email_body = email_body.Replace("{{repetitive}}", (request.end_date_action != null) ? "OUI" : "NON"); //Échéance 
                email_body = email_body.Replace("{{due_no}}", (request.end_date_action != null) ? (request.nb_occurence_action).ToString() + "/" + occurence_total.ToString() : "Action non repetitive"); //Occurence 
                email_body = email_body.Replace("{{action_end_date}}", (request.end_date_action != null) ? request.end_date_action.ToString() : "Action non repetitive"); //Occurence 
                                                                                                                                                                          //email_body = email_body.Replace("{{task_no}}", (item.DUREE_DE_CREDIT - item.NBRE_ECHEANCE).ToString() + "/" + item.DUREE_DE_CREDIT);

                return email_body;
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return string.Empty;
            }
        }

        private static string FormatAmount(decimal amount)
        {
            try
            {
                var f = new NumberFormatInfo { NumberGroupSeparator = " " };

                var s = amount.ToString("n", f); // 2 000 000.00
                return s;
            } catch(Exception ex)
            {
                ErrorLog.LogWrite("Error while formating amount.... \nError message: " + ex.Message);
                return null;
            }
        }
    }
}