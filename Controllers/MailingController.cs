using MailService.Loggers;
using MailService.Models;
using MailService.Models.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MailService.Controllers
{
    [RoutePrefix("Afriland/api/v1")]
    public class MailingController : ApiController
    {
        [Route("SendMail")]
        [HttpPost]
        public HttpResponseMessage SendMail(Mail mail)
        {
            MailResponse mailResponse = null;
            try
            {
                if (mail == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        message = "Email object is empty"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // get the details
                string sent = Mailer.SendEmail(mail.to, mail.subject, mail.emailBody, mail.copy, mail.attachment);
                if (sent.Contains("email sent from"))
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        message = "Email sent successfully"
                    };
                    LogWriter.LogWrite("Mail sending went through successfully with the following details: \n" + JsonConvert.SerializeObject(mail));
                    return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
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
