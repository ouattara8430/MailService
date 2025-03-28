using MailService.Loggers;
using MailService.Models;
using MailService.Models.Request;
using MailService.Models.Response;
using MailService.Repository;
using MailService.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using static MailService.Models.CustomerOperation;

namespace MailService.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("Afriland/api/v1")]
    public class SaisieController : ApiController
    {

        // Active directory
        [Route("LoginSaisie")]
        [HttpPost]
        public HttpResponseMessage Login(UserData user)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
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

                    //bool IsUserAuthenticated = RepoUser.IsValidActiveDirectoryUser(user.username, user.password);
                    var data = RepoUser.UserInfo(user);

                    //if (IsUserAuthenticated)
                    if (data != null)
                    {
                        //var userProfile = db.UserProfiles.Where(x => x.username == user.username).FirstOrDefault();
                        mailResponse = new MailResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            message = "User successfully authenticated",
                            data = JsonConvert.SerializeObject(data, Formatting.Indented,
                                                                    new JsonSerializerSettings
                                                                    {
                                                                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                                                                    }
                                                                )
                        };
                        LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                        return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));
                    }
                    mailResponse = new MailResponse
                    {
                        code = (int)HttpStatusCode.Forbidden,
                        message = "Invalid username or password",
                        data = "Aucune donnee recuperee..."
                    };
                    return Request.CreateResponse(HttpStatusCode.Forbidden, JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));
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


        //Rechercher un compte client avec le nom, numéro de cc
        [Route("FetchBankCustomers")]
        [HttpGet]
        public HttpResponseMessage FetchCustomerBasicInfo(string full_name)
        {
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoABS.FetchABSCustomerFullName(full_name);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.OK,
                    message = "Data fetched successfully...",
                    data = JsonConvert.SerializeObject(data),
                    record = data
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


        //Rechercher un compte client avec le nom, numéro de cc
        [Route("FetchCustomerSaisieAccount")]
        [HttpGet]
        public HttpResponseMessage FetchCustomerSaisieAccount(string customer_no)
        {
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoABS.FetchABSCustomerSaisieAccount(customer_no);
                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "No data found...",
                    data = (data != null) ? JsonConvert.SerializeObject(data) : null,
                    record = data
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data != null) ? HttpStatusCode.OK : HttpStatusCode.Accepted), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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

        //Service pour enregistrer les saisies
        [Route("InsertSaisie")]
        [HttpPost]
        public HttpResponseMessage InsertSaisie(Saisie saisie)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                db.Saisies.Add(saisie);
                int res = db.SaveChanges();


                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? saisie : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0)  ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        //Service pour lister les saisies


        //Service pour enregistrer les saisies
        [Route("FetchSaisies")]
        [HttpGet]
        public HttpResponseMessage FetchSaisies()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = db.Saisies.ToList();


                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "Something went wrong while fetching informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the encryption does not match... Error message: " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            //mailResponse = new MailResponse
            //{
            //    code = (int)HttpStatusCode.NotFound,
            //    message = "Sorry, the encryption does not match..."
            //};
            //return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        // save operation record
        [Route("InsertOperationOld")]
        [HttpPost]
        public HttpResponseMessage InsertOperationOld(CustomerOperation operation)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            int header_id = 0;
            SaisieOperation record = null;
            try
            {
                SaisieOperationHeader header = new SaisieOperationHeader
                {
                    account_no = operation.account_no,
                    account_name = operation.client,
                    amount = operation.montant,
                    date_saisie = DateTime.Today,
                    statut = 0,
                    saved_by = operation.userId
                };
                db.SaisieOperationHeaders.Add(header);

                foreach(var item in operation.record)
                {
                    record = new SaisieOperation
                    {
                         account_name = item.account_name,
                         account_no = item.account_no,
                         amount_credit = item.amount_credit,
                         amount_debit = item.amount_debit,
                         branch_code = item.branch_code,
                         operation_description = item.operation_description,
                        saisie_type_id = item.saisie_type_id,
                        saisie_operation_header_id = header.saisie_operation_header_id
                    };
                    header_id = header.saisie_operation_header_id;
                    db.SaisieOperations.Add(record);
                }
                var res = db.SaveChanges();
                header_id = header.saisie_operation_header_id;

                if (res > 0)
                {
                    //int id = db.SaisieOperations.Where(x => x.saisie_operation_id == header_id).FirstOrDefault().saisie_operation_id;
                    ServiceSaisie.GenerateWorkflow(header_id, operation.userId);
                }

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    //record = (res.Id > 0) ? operation : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                        ErrorLog.LogWrite("Error message: " + ve.PropertyName);
                    }
                }
                throw;
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

        // save operation record
        [Route("InsertOperationOriginal")]
        [HttpPost]
        public HttpResponseMessage InsertOperation(CustomerOperation operation)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            int header_id = 0;
            SaisieOperation record = null;
            try
            {
                SaisieOperationHeader header = new SaisieOperationHeader
                {
                    account_no = operation.account_no,
                    account_name = operation.client,
                    amount = operation.montant,
                    date_saisie = DateTime.Today,
                    statut = 0,
                    saved_by = operation.userId
                };
                db.SaisieOperationHeaders.Add(header);

                foreach(var item in operation.record)
                {
                    record = new SaisieOperation
                    {
                         account_name = item.account_name,
                         account_no = item.account_no,
                         amount_credit = item.amount_credit,
                         amount_debit = item.amount_debit,
                         branch_code = item.branch_code,
                         operation_description = item.operation_description,
                        saisie_type_id = item.saisie_type_id,
                        saisie_operation_header_id = header.saisie_operation_header_id
                    };
                    header_id = header.saisie_operation_header_id;
                    db.SaisieOperations.Add(record);
                }
                var res = db.SaveChanges();
                header_id = header.saisie_operation_header_id;

                if (res > 0)
                {
                    //int id = db.SaisieOperations.Where(x => x.saisie_operation_id == header_id).FirstOrDefault().saisie_operation_id;
                    ServiceSaisie.GenerateWorkflow(header_id, operation.userId);
                }

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    //record = (res.Id > 0) ? operation : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                        ErrorLog.LogWrite("Error message: " + ve.PropertyName);
                    }
                }
                throw;
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

        //Service pour enregistrer les saisies
        [Route("FetchOperations")]
        [HttpGet]
        public HttpResponseMessage FetchOperations(int? role_id)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = db.SaisieOperationHeaders.ToList();
                List<CustomerOperation> operations = new List<CustomerOperation>();

                List<int> headers_id = RepoSaisie.FetchPendingWorkflow(role_id ?? 0);
                //List<int?> ids = headers_id.Select(c => c.saisie_operation_header_id).ToList();

                List<SaisieOperationHeader> saisies = (role_id == 0) ? data : data.Where(x => headers_id.Contains(x.saisie_operation_header_id)).ToList();


                foreach (var item in saisies)
                {
                    var saisieOperations = db.SaisieOperations.Where(x => x.saisie_operation_header_id == item.saisie_operation_header_id).ToList();
                    List<CustomerOperation.Record> records = new List<CustomerOperation.Record>();
                    foreach(var it in saisieOperations)
                    {
                        records.Add(new CustomerOperation.Record
                        {
                            saisie_operation_id = it.saisie_operation_id,
                            account_name = it.account_name,
                            account_no = it.account_no,
                            amount_credit = it.amount_credit,
                            amount_debit = it.amount_debit,
                            branch_code = it.branch_code,
                            operation_description = it.operation_description,
                            saisie_type_id = it.saisie_type_id ?? 0,
                        });
                    }

                    // get info
                    var workflow = RepoSaisie.FetchWorkflowStepStatus(item.saisie_operation_header_id);

                    //var workflow = db.Workflows.Where(x => x.saisie_operation_header_id == ).FirstOrDefault();
                    int case_id = workflow.case_id;
                    int initiated_by = workflow.initiated_by ?? 0;
                    string step = string.Empty, status = string.Empty, libelle_saisie = string.Empty;
                    int saisie_type_id = records[0].saisie_type_id;
                    libelle_saisie = db.SaisieTypes.Where(x => x.saisie_type_id == saisie_type_id).FirstOrDefault().saisie_label;

                    CustomerOperation operation = new CustomerOperation
                    {
                        case_id = workflow.case_id,
                        userId = workflow.initiated_by ?? 0,
                        account_no = item.account_no,
                        client = item.account_name,
                        montant = item.amount,
                        statut = workflow.status,
                        etape = workflow.step,
                        libelle_saisie = libelle_saisie,
                        date_saisie = item.date_saisie.Value.ToString("dd/MM/yyyy"),
                        record = records,
                        document_name = item.document_name
                    };
                    operations.Add(operation);
                }


                // format response
                


                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "Something went wrong while fetching informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? operations : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the encryption does not match... Error message: " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            //mailResponse = new MailResponse
            //{
            //    code = (int)HttpStatusCode.NotFound,
            //    message = "Sorry, the encryption does not match..."
            //};
            //return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }


        //Service pour enregistrer les saisies
        [Route("FetchRejectedOperations")]
        [HttpGet]
        public HttpResponseMessage FetchRejectedOperations(int userId)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = db.SaisieOperationHeaders.ToList();
                List<CustomerOperation> operations = new List<CustomerOperation>();

                List<int> headers_id = RepoSaisie.FetchRejectedWorkflow(userId);
                //List<int?> ids = headers_id.Select(c => c.saisie_operation_header_id).ToList();

                List<SaisieOperationHeader> saisies = (userId == 0) ? data : data.Where(x => headers_id.Contains(x.saisie_operation_header_id)).ToList();


                foreach (var item in saisies)
                {
                    var saisieOperations = db.SaisieOperations.Where(x => x.saisie_operation_header_id == item.saisie_operation_header_id).ToList();
                    List<CustomerOperation.Record> records = new List<CustomerOperation.Record>();
                    foreach (var it in saisieOperations)
                    {
                        records.Add(new CustomerOperation.Record
                        {
                            saisie_operation_id = it.saisie_operation_id,
                            account_name = it.account_name,
                            account_no = it.account_no,
                            amount_credit = it.amount_credit,
                            amount_debit = it.amount_debit,
                            branch_code = it.branch_code,
                            operation_description = it.operation_description,
                            saisie_type_id = it.saisie_type_id ?? 0
                        });
                    }

                    // get info
                    var workflow = RepoSaisie.FetchWorkflowStepStatus(item.saisie_operation_header_id);

                    //var workflow = db.Workflows.Where(x => x.saisie_operation_header_id == ).FirstOrDefault();
                    int case_id = workflow.case_id;
                    int initiated_by = workflow.initiated_by ?? 0;
                    string step = string.Empty, status = string.Empty, libelle_saisie = string.Empty;
                    int saisie_type_id = records[0].saisie_type_id;
                    libelle_saisie = db.SaisieTypes.Where(x => x.saisie_type_id == saisie_type_id).FirstOrDefault().saisie_label;

                    CustomerOperation operation = new CustomerOperation
                    {
                        header_id = workflow.saisie_operation_header_id ?? 0,
                        case_id = workflow.case_id,
                        userId = workflow.initiated_by ?? 0,
                        account_no = item.account_no,
                        client = item.account_name,
                        montant = item.amount,
                        statut = workflow.status,
                        etape = workflow.step,
                        libelle_saisie = libelle_saisie,
                        date_saisie = item.date_saisie.Value.ToString("dd/MM/yyyy"),
                        record = records,
                        document_name = item.document_name
                    };
                    operations.Add(operation);
                }


                // format response



                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "Something went wrong while fetching informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? operations : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the encryption does not match... Error message: " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            //mailResponse = new MailResponse
            //{
            //    code = (int)HttpStatusCode.NotFound,
            //    message = "Sorry, the encryption does not match..."
            //};
            //return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        //Service pour enregistrer les saisies
        [Route("FetchTypeSaisies")]
        [HttpGet]
        public HttpResponseMessage FetchTypeSaisies()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoSaisie.FetchSaisieTypes();

                mailResponse = new MailResponse
                {
                    code = (data.Count > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data.Count > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data.Count > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the encryption does not match... Error message: " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            //mailResponse = new MailResponse
            //{
            //    code = (int)HttpStatusCode.NotFound,
            //    message = "Sorry, the encryption does not match..."
            //};
            //return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        //Service pour enregistrer les saisies
        [Route("ValidateSaisie")]
        [HttpPost]
        public HttpResponseMessage ValidateSaisie(RequestValidateSaisie saisie)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {

                // get the current user process and associate the decision
                bool status = ServiceSaisie.Validate(saisie);

                mailResponse = new MailResponse
                {
                    code = (status) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (status) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (status) ? saisie : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((status) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        //Service pour enregistrer les saisies
        [Route("RejectSaisie")]
        [HttpPost]
        public HttpResponseMessage RejectSaisie(RequestValidateSaisie saisie)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {

                // get the current user process and associate the decision
                bool status = ServiceSaisie.Reject(saisie);

                mailResponse = new MailResponse
                {
                    code = (status) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (status) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (status) ? saisie : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((status) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        // update operation line
        [Route("UpdateSaisieLine")]
        [HttpPost]
        public HttpResponseMessage UpdateSaisieLine(List<UpdateSaisieLineRequest> updateSaisies)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                foreach(var account_saisie in updateSaisies)
                {
                    var data = db.SaisieOperations.Where(x => x.saisie_operation_id == account_saisie.id).FirstOrDefault();
                    data.account_no = account_saisie.account_saisie;
                    db.Entry(data).State = EntityState.Modified;
                }

                int res = db.SaveChanges();


                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? updateSaisies : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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

        // add menu
        [Route("AddMenu")]
        [HttpPost]
        public HttpResponseMessage AddMenu(menu data)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {

                // get the current user process and associate the decision
                db.menus.Add(data);
                int res = db.SaveChanges();

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        // add submenu
        [Route("AddSubMenu")]
        [HttpPost]
        public HttpResponseMessage AddSubMenu(submenu data)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {

                db.submenus.Add(data);
                int res = db.SaveChanges();

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        // add role_submenu
        [Route("AddRoleSubMenu")]
        [HttpPost]
        public HttpResponseMessage AddRoleSubMenu(List<role_submenu> data)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // check whether the idroles already has menu associated
                var idroles = data.Select(c => c.idroles).ToList();
                var roles_data = db.role_submenu.Where(x => idroles.Contains(x.idroles)).ToList();
                if(roles_data.Count > 0)
                {
                    db.role_submenu.RemoveRange(roles_data);
                }

                db.role_submenu.AddRange(data);
                int res = db.SaveChanges();

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        // fetch menus
        [Route("FetchMenus")]
        [HttpGet]
        public HttpResponseMessage FetchMenus()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                var data = RepoSaisie.FetchMenus();

                mailResponse = new MailResponse
                {
                    code = (data.Count > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data.Count > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data.Count > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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

        // fetch sub menus
        [Route("FetchSubMenus")]
        [HttpGet]
        public HttpResponseMessage FetchSubMenus()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                var data = RepoSaisie.FetchSubMenus();

                mailResponse = new MailResponse
                {
                    code = (data.Count > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data.Count > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data.Count > 0) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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


        // fetch sub menus
        [Route("FetchRoleMenu")]
        [HttpGet]
        public HttpResponseMessage FetchRoleMenu(int role_id)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                var data = RepoSaisie.FetchRoleMenu(role_id);



                mailResponse = new MailResponse
                {
                    code = (data.Count > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Created,
                    message = (data.Count > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data.Count > 0) ? data : new List<FetchRoleMenus_Result>()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.Created), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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


        // fetch menus
        [Route("FetchRoles")]
        [HttpGet]
        public HttpResponseMessage FetchRoles()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                var data = db.Roles.Select(x => new { x.role_id, x.role_description }).ToList();

                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data != null) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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

        // FetchReportByUsers
        [Route("FetchReportByUsers")]
        [HttpGet]
        public HttpResponseMessage FetchReportByUsers(int user_id)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                var data = RepoSaisie.FetchReportByUsers(user_id);
                List<CustomerOperation> operations = new List<CustomerOperation>();

                foreach (var item in data)
                {
                    var saisieOperations = db.SaisieOperations.Where(x => x.saisie_operation_header_id == item.saisie_operation_header_id).ToList();
                    List<CustomerOperation.Record> records = new List<CustomerOperation.Record>();
                    foreach (var it in saisieOperations)
                    {
                        records.Add(new CustomerOperation.Record
                        {
                            saisie_operation_id = it.saisie_operation_id,
                            account_name = it.account_name,
                            account_no = it.account_no,
                            amount_credit = it.amount_credit,
                            amount_debit = it.amount_debit,
                            branch_code = it.branch_code,
                            operation_description = it.operation_description,
                            saisie_type_id = it.saisie_type_id ?? 0
                        });
                    }

                    // get info
                    var workflow = RepoSaisie.FetchWorkflowStepStatus(item.saisie_operation_header_id);

                    //var workflow = db.Workflows.Where(x => x.saisie_operation_header_id == ).FirstOrDefault();
                    int case_id = workflow.case_id;
                    int initiated_by = workflow.initiated_by ?? 0;
                    string step = string.Empty, status = string.Empty, libelle_saisie = string.Empty;
                    int saisie_type_id = records[0].saisie_type_id;
                    libelle_saisie = db.SaisieTypes.Where(x => x.saisie_type_id == saisie_type_id).FirstOrDefault().saisie_label;

                    CustomerOperation operation = new CustomerOperation
                    {
                        case_id = workflow.case_id,
                        userId = workflow.initiated_by ?? 0,
                        account_no = item.account_no,
                        client = item.account_name,
                        montant = item.amount,
                        statut = workflow.status,
                        etape = workflow.step,
                        libelle_saisie = libelle_saisie,
                        date_saisie = item.initiated_at.Value.ToString("dd/MM/yyyy"),
                        record = records,
                    };
                    operations.Add(operation);
                }



                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "Something went wrong while fetching informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? operations : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((data.Count > 0) ? HttpStatusCode.OK : HttpStatusCode.Created), JObject.Parse(JsonConvert.SerializeObject(mailResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        })));

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


        // save operation record
        [Route("UpdateOperation")]
        [HttpPost]
        public HttpResponseMessage UpdateOperation(CustomerOperation operation)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            int header_id = 0;
            try
            {
                // get header info
                var saisie_header_info = db.SaisieOperationHeaders.Where(x => x.saisie_operation_header_id == operation.header_id).FirstOrDefault();
                saisie_header_info.account_no = operation.account_no;
                saisie_header_info.account_name = operation.client;
                saisie_header_info.amount = operation.montant;
                saisie_header_info.saved_by = operation.userId;
                db.Entry(saisie_header_info).State = EntityState.Modified;


                foreach (var item in operation.record)
                {
                    var record = db.SaisieOperations.Where(x => x.saisie_operation_id == item.saisie_operation_id).FirstOrDefault();
                    record.account_name = item.account_name;
                    record.account_no = item.account_no;
                    record.amount_credit = item.amount_credit;
                    record.amount_debit = item.amount_debit;
                    record.branch_code = item.branch_code;
                    record.operation_description = item.operation_description;
                    record.saisie_type_id = item.saisie_type_id;
                    db.Entry(record).State = EntityState.Modified;
                }
                var res = db.SaveChanges();

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data updated successfully..." : "Something went wrong while updating informations...",
                };
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                        ErrorLog.LogWrite("Error message: " + ve.PropertyName);
                    }
                }
                throw;
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

        // upload file
        [Route("fileupload")]
        [HttpPost]
        public HttpResponseMessage UploadFiles()
        {
            MailResponse mailResponse = null;
            if (!Request.Content.IsMimeMultipartContent())
            {
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the file is corrupted..."
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }

            var provider = new MultipartFormDataStreamProvider(Path.Combine(HttpContext.Current.Server.MapPath("~/Uploadeds")));
            var fileName = string.Empty;

            try
            {
                // Read the form data and files
                var task = Request.Content.ReadAsMultipartAsync(provider);
                task.Wait();

                foreach (var file in provider.FileData)
                {
                    fileName = Path.GetFileName(file.LocalFileName);
                    // Optionally, you can also rename the file before saving
                    // File.Move(file.LocalFileName, Path.Combine(..., newFileName));
                }
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.OK,
                    message = "Data updated successfully...",
                };
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            catch (Exception ex)
            {

            }
            mailResponse = new MailResponse
            {
                code = (int)HttpStatusCode.NotFound,
                message = "Sorry, the encryption does not match..."
            };
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

        }


        [Route("InsertOperation")]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertOperationFile()
        {
            MailResponse mailResponse;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                }


                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                CustomerOperation operation = null;
                FileUpload uploadedFiles = new FileUpload();

                foreach (var content in provider.Contents)
                {
                    // Check if the content is a file
                    if (content.Headers.ContentDisposition.FileName != null)
                    {
                        var fileName = content.Headers.ContentDisposition.FileName.Trim('"');
                        var fileContent = await content.ReadAsByteArrayAsync();

                        // Save the file to the server
                        string path = HttpContext.Current.Server.MapPath("~/Uploads/"); // Adjust path as necessary

                        uploadedFiles = new FileUpload
                        {
                            FileName = fileName,
                            FileContent = fileContent
                        };

                        string fullPath = Path.Combine(path, uploadedFiles.FileName);
                        Directory.CreateDirectory(path); // Ensure the directory exists

                        // Write the file to the server
                        File.WriteAllBytes(fullPath, fileContent);

                    }
                    else
                    {

                        // Assume the content is form data
                        var jsonData = await content.ReadAsStringAsync();
                        operation = JsonConvert.DeserializeObject<CustomerOperation>(jsonData);
                    }
                }

                if (operation != null)
                {
                    string path = "http://172.19.56.80/MailServiceLive/Uploads/" + uploadedFiles.FileName;
                    //operation.Files[0].FileName = path + uploadedFiles[0].FileName; // Associate the uploaded files with the operation

                    using (var db = new CommitmentDBTestEntities())
                    {
                        int headerId;

                        try
                        {
                            // Create operation header
                            var header = new SaisieOperationHeader
                            {
                                account_no = operation.account_no,
                                account_name = operation.client,
                                amount = operation.montant,
                                date_saisie = DateTime.Today,
                                statut = 0,
                                saved_by = operation.userId,
                                document_name = path
                            };
                            db.SaisieOperationHeaders.Add(header);

                            // Add each operation record
                            foreach (var item in operation.record)
                            {
                                var record = new SaisieOperation
                                {
                                    saisie_operation_id = 0,
                                    account_name = item.account_name,
                                    account_no = item.account_no,
                                    amount_credit = item.amount_credit,
                                    amount_debit = item.amount_debit,
                                    branch_code = item.branch_code,
                                    operation_description = item.operation_description,
                                    saisie_type_id = item.saisie_type_id,
                                    saisie_operation_header_id = header.saisie_operation_header_id
                                };
                                db.SaisieOperations.Add(record);
                            }

                            // Save changes
                            var res = db.SaveChanges();
                            headerId = header.saisie_operation_header_id;

                            if (res > 0)
                            {
                                ServiceSaisie.GenerateWorkflow(headerId, operation.userId);
                            }

                            mailResponse = new MailResponse
                            {
                                code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                                message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                                //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                                //record = (res.Id > 0) ? operation : new Object()
                            };
                            //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                            return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                    ErrorLog.LogWrite("Error message: " + ve.PropertyName);
                                }
                            }
                            throw;
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                        }
                    }
                }

            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                        ErrorLog.LogWrite("Error message: " + ve.PropertyName);
                    }
                }
                throw;
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

        //Service pour enregistrer les saisies des clients non trouvés
        [Route("SaveOtherCustomerSaisie")]
        [HttpPost]
        public HttpResponseMessage SaveOtherCustomerSaisie(OtherCustomerSaisie saisie)
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                db.OtherCustomerSaisies.Add(saisie);
                int res = db.SaveChanges();


                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res > 0) ? saisie : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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


        [Route("FetchOtherCustomerSaisie")]
        [HttpGet]
        public HttpResponseMessage FetchOtherCustomerSaisie()
        {
            CommitmentDBTestEntities db = new CommitmentDBTestEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = db.OtherCustomerSaisies.ToList();


                mailResponse = new MailResponse
                {
                    code = (data != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (data != null) ? "Data fetched successfully..." : "Something went wrong while fetching informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (data != null) ? data : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching data. Error message: " + ex.Message);
                mailResponse = new MailResponse
                {
                    code = (int)HttpStatusCode.NotFound,
                    message = "Sorry, the encryption does not match... Error message: " + ex.Message
                };
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
            }
            //mailResponse = new MailResponse
            //{
            //    code = (int)HttpStatusCode.NotFound,
            //    message = "Sorry, the encryption does not match..."
            //};
            //return Request.CreateResponse(HttpStatusCode.NotAcceptable, JObject.Parse(JsonConvert.SerializeObject(mailResponse)));
        }

        //Service pour valider une saisie
        //Service pour rejeter une saisie
        //Service pour ajouter compte de saisie
        //Service des users pour se connecter
        //Services d’habilitation des users 
        //	Services workflow
    }
}
