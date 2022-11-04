using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace MailService.Loggers
{
    public class ErrorLog
    {
        private static string log_path = string.Empty;
        //public LogWriter ( string logMessage )
        //{
        //    LogWrite( logMessage );
        //}
        public static void LogWrite(string logMessage)
        {
            string filePath = ConfigurationManager.AppSettings["LogPath"].ToString();
            string filename = "error_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";
            //log_path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            log_path = Path.GetDirectoryName(filePath);
            //var fullpath = Path.Combine( path, "myfile.txt" );
            try
            {
                //File.Create( log_path ).Dispose();
                //using ( StreamWriter w = File.AppendText( log_path + "\\" + "log_" + DateTime.Now + ".txt" ) ) {
                using (StreamWriter w = File.AppendText(log_path + "\\" + filename))
                {
                    Error(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void Error(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine(logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
            }
        }
    }
}