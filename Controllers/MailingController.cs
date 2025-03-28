using MailService.Loggers;
using MailService.Models;
using MailService.Models.Response;
using MailService.Repository;
using MailService.Service;
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

        // Active directory
        [Route("ActiveDirectory")]
        [HttpPost]
        public HttpResponseMessage AuthenticateUserAD(UserData user)
        {
            MailResponse mailResponse = null;
            try
            {
                if (user == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        message = "User data is empty"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // format the data


                // check whether the user is sending the right information
                //string formula = EncryptionService.ComputeSha256Hash(RepoUser.FormatUserInfo(user.username));
                string formula = EncryptionService.ComputeSha256Hash(user.password);

                if (formula.Equals(user.passphrase))
                {

                    bool IsUserAuthenticated = RepoUser.IsValidActiveDirectoryUser(user.username, user.password);

                    if (IsUserAuthenticated)
                    {
                        mailResponse = new MailResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            message = "User successfully authenticated"
                        };
                        LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                        return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                    }
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.Forbidden,
                        message = "Invalid username or password"
                    };
                    return Request.CreateResponse(HttpStatusCode.Forbidden, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while authenticating. Error message: " + ex.Message);
            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Sorry, the encryption does not match..."
            };
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        // Verification AD
        [Route("CheckingAuthentication")]
        [HttpPost]
        public HttpResponseMessage AuthenticateUserAD(UserDataAD user)
        {
            MailResponse mailResponse = null;
            try
            {
                if (user == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        message = "User data is empty"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // format the data


                // check whether the user is sending the right information
                //string formula = EncryptionService.ComputeSha256Hash(RepoUser.FormatUserInfo(user.username));
                string formula = EncryptionService.ComputeSha256Hash(user.password);

                if (formula.Equals(user.passphrase))
                {

                    // check whether the user already voted
                    var HasVoted = RepoUser.CheckUserAlreadyVoted(user.username);

                    if (HasVoted)
                    {
                        mailResponse = new MailResponse
                        {
                            code = (int)HttpStatusCode.Accepted,
                            message = "User has already voted !"
                        };
                        return Request.CreateResponse(HttpStatusCode.Forbidden, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                    }

                    bool IsUserAuthenticated = RepoUser.IsValidActiveDirectoryUser(user.username, user.password);

                    if (IsUserAuthenticated)
                    {
                        CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                        var data = db.UserProfiles.Where(x => x.username == user.username).FirstOrDefault();

                        Staff staff = new Staff
                        {
                            userId = data.userId,
                            first_name = data.first_name,
                            last_name = data.last_name,
                            username = data.username
                        };

                        mailResponse = new MailResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            message = "User successfully authenticated",
                            data = JsonConvert.SerializeObject(staff)
                        };
                        LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                        return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                    }
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.Forbidden,
                        message = "Invalid username or password"
                    };
                    return Request.CreateResponse(HttpStatusCode.Forbidden, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while authenticating. Error message: " + ex.Message);
            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Sorry, the encryption does not match..."
            };
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }


        // Verification AD
        [Route("ListCandidates")]
        [HttpGet]
        public HttpResponseMessage FetchListCandidate()
        {
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoUser.FetchCandidates();
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.OK,
                    message = "Candidates data fetched successfully...",
                    data = JsonConvert.SerializeObject(data)
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Sorry, the encryption does not match..."
            };
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }


        // Verification AD
        [Route("SaveVote")]
        [HttpPost]
        public HttpResponseMessage InsertVotes([FromBody]List<VoteModel> votes)
        {
            MailResponse mailResponse = null;
            try
            {
                if (votes == null)
                {
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        message = "Email object is empty"
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                }

                // save data
                int save = RepoUser.SaveVotes(votes);
                if(save > 0)
                {

                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        message = "Data saved successfully... Instance: " + votes.Count()
                    };
                    LogWriter.LogWrite("Mail sending went through successfully with the following details: \n" + JsonConvert.SerializeObject(mailResponse));
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

        [Route("DeleteAllVotes")]
        [HttpDelete]
        public HttpResponseMessage Delete()
        {
            MailResponse mailResponse = null;
            try
            {
                // save data
                bool save = RepoUser.DeleteVotes();
                if (save)
                {

                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        data = string.Empty,
                        message = "Data deleted successfully... "
                    };
                    LogWriter.LogWrite("Mail sending went through successfully with the following details: \n" + JsonConvert.SerializeObject(mailResponse));
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

        // Verification AD
        [Route("GetCustomerBasicInfo")]
        [HttpGet]
        public HttpResponseMessage FetchCustomerBasicInfo(string customer_no)
        {
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoABS.FetchCustomerBasinInfo(customer_no);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.OK,
                    message = "Data fetched successfully...",
                    data = JsonConvert.SerializeObject(data)
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Sorry, the encryption does not match..."
            };
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        
    }
}
