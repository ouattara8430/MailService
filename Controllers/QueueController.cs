using MailService.Loggers;
using MailService.Models.Queue;
using MailService.Models.Response;
using MailService.Repository;
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
    public class QueueController : ApiController
    {
        // fetch the list of services
        [Route("FetchServices")]
        [HttpGet]
        public HttpResponseMessage FetchServices()
        {
            QueueDBEntities db = new QueueDBEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                // fetch the list of candidates
                var data = RepoQueue.FetchServices();

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
        }

        // fetch the list of services linked to type of services
        [Route("FetchListServices")]
        [HttpGet]
        public HttpResponseMessage FetchTypeServices(int? service_id)
        {
            QueueDBEntities db = new QueueDBEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var data = RepoQueue.FetchListService(service_id);

                // Creating the Service object with the list of ServiceTypes
                TblServiceData service = new TblServiceData
                {
                    service_label = data[0].service_label,
                    service_id = data[0].service_id,
                    services = data
                };

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
        }

        // generate ticket number and assign to a staff (teller / cashier)

        // save operation record
        [Route("GenerateTicketNo")]
        [HttpGet]
        public HttpResponseMessage GetTicketNo()
        {
            QueueDBEntities db = new QueueDBEntities();
            MailResponse mailResponse = null;
            try
            {
                // fetch the list of candidates
                var res = RepoQueue.GetTicketNo();


                mailResponse = new MailResponse
                {
                    code = (res.Length > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res.Length > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    record = (res.Length > 0) ? res : new Object()
                };
                //LogWriter.LogWrite("Username: \n" + user.username + " successfully authenticated");
                return Request.CreateResponse(((res.Length > 0) ? HttpStatusCode.OK : HttpStatusCode.BadRequest), JObject.Parse(JsonConvert.SerializeObject(mailResponse)));

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

        // saving ticket in queue
        [Route("SaveTicket")]
        [HttpPost]
        public HttpResponseMessage InsertTicket(Ticket ticket)
        {
            QueueDBEntities db = new QueueDBEntities();
            MailResponse mailResponse = null;
            try
            {

                // get the available staff and assign the ticket in his queue
                int staff_id = RepoQueue.GetAvailableStaffId();
                staff_id = (staff_id == 0) ? 1 : staff_id;

                // assign ticket to staff
                var ticketAssign = new TicketAssignment
                {
                    staff_id = staff_id,
                    assigned_at = DateTime.Now,
                    status = 0,
                    ticket_id = ticket.ticket_id
                };

                // assign ticket
                db.TicketAssignments.Add(ticketAssign);

                // Add ticket
                ticket.decision_id = 0;
                ticket.created_at = DateTime.Now;
                ticket.seq_no = 0;
                ticket.ticket_status = "Open";
                db.Tickets.Add(ticket);

                int res = db.SaveChanges();

                mailResponse = new MailResponse
                {
                    code = (res > 0) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    message = (res > 0) ? "Data saved successfully..." : "Something went wrong while saving informations...",
                    //data = (res > 0) ? JsonConvert.SerializeObject(saisies) : ""
                    //record = (res > 0) ? ticket : new Object()
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
    }
}
