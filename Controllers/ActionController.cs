using MailService.Loggers;
using MailService.Models;
using MailService.Models.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MailService.Controllers
{
    [RoutePrefix("Afriland/api/v1")]
    public class ActionController : ApiController
    {
        [Route("GenerateAction")]
        [HttpPost]
        public HttpResponseMessage GenerateAction(AlertRequest request)
        {
            MailResponse mailResponse = null;
            try
            {
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();

                // log the mail format
                LogWriter.LogWrite("Before calling the web service: Informations details: \n" +JsonConvert.SerializeObject(request) + "\nTime: " + DateTime.Now);

                // validation

                if (request == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        message = "Email object is empty"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // check whether the loan number exists
                var DoesLoanExists = db.LoanApplications.Where(x => x.application_id == request.application_id).FirstOrDefault();
                if (DoesLoanExists == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.Ambiguous,
                        message = "Sorry, the loan number does not exists in the system."
                    };
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // set the occurence and the next of the alert and the action
                /* start alert */
                int nb_occurence_alert = 0;
                int nb_occurence_action = 0;
                if (request.end_date != null)
                {
                    DateTime start_date_val = (DateTime)request.start_date;
                    while (start_date_val < request.end_date)
                    {
                        switch (request.alert_frequence_periodicity)
                        {
                            case "Jour":
                                nb_occurence_alert++;
                                start_date_val = start_date_val.AddDays(Int32.Parse(request.alert_frequence));
                                break;
                            case "Semaine":
                                nb_occurence_alert++;
                                start_date_val = start_date_val.AddDays(Int32.Parse(request.alert_frequence) * 7);
                                break;
                            case "Mois":
                                nb_occurence_alert++;
                                start_date_val = start_date_val.AddMonths(Int32.Parse(request.alert_frequence));
                                break;
                            case "Annee":
                                nb_occurence_alert++;
                                start_date_val = start_date_val.AddYears(Int32.Parse(request.alert_frequence));
                                break;
                        }
                    }
                }

                /* end alert*/

                /* start action */
                if (request.end_date_action != null)
                {
                    DateTime due_date_action_val = (DateTime)request.due_date_action;
                    while (due_date_action_val < request.end_date_action)
                    {
                        switch (request.alert_action_periodicity)
                        {
                            case "Jour":
                                nb_occurence_action++;
                                due_date_action_val = due_date_action_val.AddDays(Int32.Parse(request.action_frequence));
                                break;
                            case "Semaine":
                                nb_occurence_action++;
                                due_date_action_val = due_date_action_val.AddDays(Int32.Parse(request.action_frequence) * 7);
                                break;
                            case "Mois":
                                nb_occurence_action++;
                                due_date_action_val = due_date_action_val.AddMonths(Int32.Parse(request.action_frequence));
                                break;
                            case "Annee":
                                nb_occurence_action++;
                                due_date_action_val = due_date_action_val.AddYears(Int32.Parse(request.action_frequence));
                                break;
                        }
                    }
                }
                /* end action */


                // remaining parameters
                request.nb_occurence = nb_occurence_alert;
                request.occurence_alert_duree = nb_occurence_alert;
                request.nb_occurence_action = nb_occurence_action;
                request.occurence_action_duree = nb_occurence_action;
                request.next_alert_date = request.start_date;
                request.next_action_date = request.due_date_action;

                // set user id
                //request.userId = Int32.Parse(ConfigurationManager.AppSettings["userId"].ToString());
                //request.decision_id = Int32.Parse(ConfigurationManager.AppSettings["decisionId"].ToString());
                //request.executed_by = Int32.Parse(ConfigurationManager.AppSettings["executedBy"].ToString());

                // save the object
                db.AlertRequests.Add(request);
                int save = db.SaveChanges();
                //int save = 1;
                if (save > 0)
                {
                    // log the mail format
                    LogWriter.LogWrite("After calling the web service: Response details: \nTime: " + DateTime.Now);


                    // send email
                    string path = ConfigurationManager.AppSettings["suivi_decision"].ToString();
                    string email_body = Mailer.SendToAccountOfficer("late", path, request, nb_occurence_action);

                    // get the account officer details
                    var acct_officer = db.UserProfiles.Where(x => x.userId == request.executed_by).FirstOrDefault();

                    // get the customer name
                    string customer_name = db.LoanApplications.Where(x => x.application_id == request.application_id).Select(c => c.customer_fullname).FirstOrDefault();

                    // get the decision deciption
                    string decision_desc = db.Decisions.Where(x => x.decision_id == request.decision_id).FirstOrDefault().decision_desc;

                    //string mail = Mailer.SendEmail(acct_officer.email, "SUIVI ENGAGEMENT POUR LE CLIENT " + customer_name.ToUpper(), email_body, acct_officer.responsable_email, null);
                    string mail = Mailer.SendEmail(acct_officer.email, decision_desc.ToUpper() + " POUR LE CLIENT " + customer_name.ToUpper(), email_body, acct_officer.responsable_email, null);

                    //string mail = Mailer.SendEmail(acct_officer.email, "SUIVI ENGAGEMENT POUR LE PRET NO " + request.application_id, email_body, acct_officer.responsable_email, null);

                    if (mail != string.Empty)
                    {
                        // log the mail format
                        LogWriter.LogWrite("Mail envoye avec success au gestionnaire : " + acct_officer.first_name + " " + acct_officer.last_name + " avec le mail: " + acct_officer.email +
                            " pour le dossier no. " + request.application_id + " le " + DateTime.Now);
                    }
                    else
                    {
                        // log the mail format
                        LogWriter.LogWrite("Echec du mail envoye au gestionnaire : " + acct_officer.first_name + " " + acct_officer.last_name + " avec le mail: " + acct_officer.email +
                            " pour le dossier no. " + request.application_id + " le " + DateTime.Now);
                    }

                    // log activity
                    //LogWriter.LogWrite("Web service generating action successfully... with the following details: \nDetails: " + JsonConvert.SerializeObject(request) + "\nTime: " + DateTime.Now);
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        message = "Action generated successfully"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }
                else
                {
                    // log activity
                    LogWriter.LogWrite("Error while calling web service... with the following details: \nAction Url: GenerateAction \nDetails: " + JsonConvert.SerializeObject(request) + "\nTime: " + DateTime.Now);
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.NotFound,
                        message = "Error while saving action into the database"
                    };

                    return Request.CreateResponse(HttpStatusCode.NotFound, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while sending the mail. Error message: " + ex.Message);
            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Error while sending the mail"
            };
            return Request.CreateResponse(HttpStatusCode.NotFound, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }
    }
}
