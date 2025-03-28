using MailService.Loggers;
using MailService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Web;

namespace MailService.Repository
{
    public class RepoUser
    {

        private static string activeDirectoryServerDomain = ConfigurationManager.AppSettings["LDAPDomain"].ToString();

        public static bool IsValidActiveDirectoryUser(string username, string password)
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://" + activeDirectoryServerDomain, username + "@" + activeDirectoryServerDomain, password, AuthenticationTypes.Secure);
                DirectorySearcher ds = new DirectorySearcher(de);
                SearchResult result = null;
                result = ds.FindOne();

                LogWriter.LogWrite("AD connectivity: " + result);

                return true;
            }
            catch (LdapException ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.GetBaseException().Message);
                return false;
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.GetBaseException().Message);
                return false;
            }
        }

        // get the formula to check the user information
        public static string FormatUserInfo(string username)
        {
            return username + DateTime.Today.ToString("ddMMyyyy") + username.Substring(1, 3).ToUpper();
        }

        // check user info
        public static UserProfile UserInfo(UserData user)
        {
            try
            {
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                return db.UserProfiles.Where(x => x.username == user.username && x.password == user.password).FirstOrDefault();

            } catch(Exception ex)
            {
                return null;
            }
        }

        // 
        public static bool CheckUserAlreadyVoted(string username)
        {
            try
            {
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();

                var user = db.UserVoteTblCandidates.Where(x => x.username == username).FirstOrDefault();
                if(user != null)
                {
                    return true;
                }

                //LogWriter.LogWrite("AD connectivity: " + result);

                return false;
            }
            catch (LdapException ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.GetBaseException().Message);
                return false;
            }
            catch (Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.GetBaseException().Message);
                return false;
            }
        }

        // get the list of candidates
        public static List<TblCandidate> FetchCandidates()
        {
            try
            {
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                var candidates = db.TblCandidates.ToList();
                return candidates;

            } catch(Exception ex)
            {
                ErrorLog.LogWrite("Error while fetching the list of candidates... Error message: " + ex.Message);
                return null;
            }
        }

        // save votes
        public static int SaveVotes(List<VoteModel> votes)
        {
            try
            {
                // save votes
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();


                foreach(var item in votes)
                {
                    var vote = new UserVoteTblCandidate
                    {
                        username = item.username,
                        candidate_id = item.candidate_id,
                        userid = item.userid,
                        matricule = item.matricule,
                        vote_status = item.vote_status,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };

                    db.UserVoteTblCandidates.Add(vote);
                }

                int res = db.SaveChanges();

                return res;
            } catch(Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return 0;
            }
        }

        // delete data
        public static bool DeleteVotes()
        {
            try
            {
                // save votes
                CommitmentDBTestEntities db = new CommitmentDBTestEntities();
                var votes = db.UserVoteTblCandidates.ToList();

                db.UserVoteTblCandidates.RemoveRange(votes);
                int res = db.SaveChanges();

                if(res > 0)
                {
                    return true;
                }
                return false;

            } catch(Exception ex)
            {
                ErrorLog.LogWrite("Error message: " + ex.Message);
                return false;
            }
        }
    }
}