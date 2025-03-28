using MailService.Loggers;
using MailService.Models.Queue;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MailService.Repository
{
    public class RepoQueue
    {
        // connection string for accessing the database
        private static QueueDBEntities db = null;
        // fetch header id list
        public static List<FetchListService_Result> FetchListService(int? service_id)
        {
            try
            {
                using (QueueDBEntities db = new QueueDBEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    List<FetchListService_Result> historique_Results = db.Database.SqlQuery<FetchListService_Result>("FetchListService @service_id", new SqlParameter("@service_id", service_id)).ToList();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }

        public static string GetTicketNo()
        {
            try
            {
                using (QueueDBEntities db = new QueueDBEntities())
                {
                    // Define the output parameter for the stored procedure
                    var ticketNumberParam = new SqlParameter("@TicketNumber", SqlDbType.VarChar, 50)
                    {
                        Direction = ParameterDirection.Output
                    };

                    // Execute the stored procedure and fetch the result
                    var result = db.Database.SqlQuery<TblGenerateTicketNo>(
                        "EXEC [dbo].[GenerateTicketNo] @TicketNumber OUT", ticketNumberParam).FirstOrDefault();

                    // Retrieve the output ticket number
                    return ticketNumberParam.Value.ToString();  // Access the output parameter directly
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return null;
            }
        }



        public static int GetAvailableStaffId()
        {
            try
            {
                using (QueueDBEntities db = new QueueDBEntities())
                {
                    // db.Database.SqlQuery<FetchTransferredActions_Result>("FetchTransferredActions @userId", new SqlParameter("userId", userId)).ToList();
                    var historique_Results = db.Database.SqlQuery<int>("FetchAvailableStaff").FirstOrDefault();
                    return historique_Results;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return 0;
            }
        }

        public static List<FetchServices_Result> FetchServices()
        {
            try
            {
                using (QueueDBEntities db = new QueueDBEntities())
                {
                    List<FetchServices_Result> historique_Results = db.Database.SqlQuery<FetchServices_Result>("FetchServices").ToList();
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