using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using URL = System.Uri;

namespace ConnectADP
{
    public class ConnectADP
    {
        protected string _baseUrl;
        protected string _login;
        protected string _password;

        protected string _bzsession;
        protected string _principal;

        public enum MeetingRoles
        {
            Participant = 1,
            Presenter,
            Host,
            Denied
        };

        public enum BuiltInGoups
        {
            Administrator = 1,
            AdministratorsLimited,
            Authors,
            TrainingManagers,
            EventManagers,
            EventAdministrators,
            Learners,
            MeetingHosts
        }

        public ConnectADP(string baseUrl, string login, string password, string bzsession)
        {
            _baseUrl = baseUrl;
            _login = login;
            _password = password;
            _bzsession = bzsession;
        }

        public ConnectADP(string baseUrl, string bzsession)
        {
            _baseUrl = baseUrl;
            _bzsession = bzsession;
        }

        /// <summary>
        /// Log in to an Adobe Connect session
        /// </summary>
        protected void Login()
        {
            if (_bzsession != "")
                Logout();

            URL loginUrl = BreezeUrl("login",
                "login=" + _login + "&password=" + _password);

            var conn = WebRequest.Create(loginUrl);

            var resp = conn.GetResponse();
            var resultStream = resp.GetResponseStream();

            var encode = Encoding.GetEncoding("utf-8");
            var streamReader = new StreamReader(resultStream, encode);
            streamReader.ReadToEnd();

            var bzsessionstr = resp.Headers.Get("Set-Cookie");
            string[] tokens = bzsessionstr.Split('=');
            string sessionName = null;

            var token = tokens.GetEnumerator();
            token.MoveNext();

            if (tokens.Length > 0)
                sessionName = token.Current.ToString();

            if (sessionName != null && sessionName.Equals("JSESSIONID") || sessionName.Equals("BREEZESESSION"))
            {
                token.MoveNext();
                var bzsessionNext = token.Current.ToString();
                var semiIndex = bzsessionNext.IndexOf(';');

                _bzsession = bzsessionNext.Substring(0, semiIndex);
            }
            resp.Close();
            resultStream.Close();

            if (bzsessionstr == null)
                throw new Exception("Could not log in to Connect.");
        }

        /// <summary>
        /// Get Adobe Connect Principal ID
        /// </summary>
        /// <returns>principal-id</returns>
        public string GetPrincipalId()
        {
            if (_bzsession == "")
                Login();

            return Request("common-info", "").SelectSingleNode("//user/@user-id").Value;
        }

        /// <summary>
        /// Add a  user to an Adobe Connect Meeting
        /// </summary>
        /// <param name="acl">Meeting Identifier</param>
        /// <param name="email">User email</param>
        /// <param name="role">Meeting access permissions</param>
        public void AddUserToMeeting(string acl, string email, MeetingRoles role)
        {
            var principal_id = GetPrincipalIdByEmail(email);

            Request("permissions-update",
                    "principal-id=" + principal_id + "&acl-id=" + acl + "&permission-id=" + GetRole((int)role));
        }

        /// <summary>
        /// Remove user from an Adobe Conect Meeting, users will stay connected until session expires.
        /// </summary>
        /// <param name="acl"></param>
        /// <param name="email"></param>
        public void RemoveUserFromMeeting(string acl, string email)
        {
            var principal_id = GetPrincipalIdByEmail(email);

            Request("permissions-update",
                    "principal-id=" + principal_id + "&acl-id=" + acl + "&permission-id=remove");
        }

        /// <summary>
        /// Return Principal ID for user with email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>principal-id</returns>
        public string GetPrincipalIdByEmail(string email)
        {
            return Request("principal-list", "filter-email=" + email).SelectSingleNode("results/principal-list/principal/@principal-id").Value;
        }

        private string GetRole(int roleId)
        {
            var role = "";
            switch (roleId)
            {
                case 1:
                    role = "view";
                    break;
                case 2:
                    role = "mini-host";
                    break;
                case 3:
                    role = "host";
                    break;
                case 4:
                    break;
            }
            return role;
        }

        private string GetGroup(int groupId)
        {
            var group = "";
            switch (groupId)
            {
                case 1:
                    group = "admins";
                    break;
                case 2:
                    group = "admins-limited";
                    break;
                case 3:
                    group = "authors";
                    break;
                case 4:
                    group = "course-admins";
                    break;
                case 5:
                    group = "event-admins";
                    break;
                case 6:
                    group = "event-super-admins";
                    break;
                case 7:
                    group = "learners";
                    break;
                case 8:
                    group = "live-admins";
                    break;
            }
            return group;
        }

        private string GetGroupPrincipalId(BuiltInGoups group)
        {
            return Request("principal-list", "filter-group=" +
                GetGroup((int)group)).SelectSingleNode("/results/principal-list/principal/@principal-id").Value;
        }


        public void AddUserToBuiltInGroup(string email, BuiltInGoups group)
        {
            var user_id = GetPrincipalIdByEmail(email);
            var group_id = GetGroupPrincipalId(group);

            Request("group-membership-update",
                    "group-id=" + group_id +
                    "&principal_id=" + user_id);
        }

        /// <summary>
        /// Create new user on Adobe Connect server.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void CreateUser(string firstName, string lastName, string email, string password)
        {
            if (_bzsession == "")
                Login();

            var queryString = "first-name=" + firstName +
                                "&last-name=" + lastName +
                                "&login=" + email +
                                "&password=" + password +
                                "&type=user" +
                                "&send-email=true" +
                                "&has-children=0" +
                                "&email=" + email;

            var result = Request("principal-update", queryString);
        }

        public void UpdateUser(string principal_id,
                                string firstName = "",
                                string lastName = "",
                                string login = "",
                                string type = "",
                                string email = "")
        {
            var query = "principal-id=" + principal_id;

            if (firstName != "")
                query = "&first-name=" + firstName;
            if (lastName != "")
                query = query + "&last-name=" + lastName;
            if (login != "")
                query = query + "&login=" + login;
            if (type != "")
                query = query + "&type=" + type;
            if (email != "")
                query = query + "&email=" + email;

            Request("principal-update", query);
        }

        public void UpdateUserPassword(string user_id, string oldpassword, string password, string password_verify)
        {
            var query = "user-id=" + user_id +
                        "&password-old=" + oldpassword +
                        "&password=" + password +
                        "&password-verify=" + password_verify +
                        "&session=" + _bzsession;

            Request("user-update-pwd", query);
        }


        private bool IsAdmin
        {
            get
            {
                if (_bzsession == "")
                    Login();

                var queryString = "principal-id=" + GetPrincipalId() +
                                  "&filter-is-member=true";

                Request("principal-list", queryString);

                return true;
            }
        }

        /// <summary>
        /// Create new meeting on Adobe Connect Server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="urlPath">path to the meeting room</param>
        public void CreateMeeting(string name, string start, string end, string urlPath)
        {
            var sco_id = Request("sco-shortcuts", "").
                SelectSingleNode("results/shortcuts/sco[@type='my-meetings']/@sco-id").Value;


            var query = "type=meeting" +
                        "&name=" + name +
                        "&folder-id=" + sco_id +
                        "&date-begin=" + start +
                        "&date-end=" + end +
                        "&url-path=" + urlPath;

            sco_id = Request("sco-update", query).SelectSingleNode("results/sco/@sco-id").Value;

            //DO NOT DELETE***************************
            Request("permissions-update",
                    "acl-id=" + sco_id +
                    "&principal-id=public-access&permission-id=view-hidden");

        }

        /// <summary>
        /// Return all meetings in my login context.
        /// </summary>
        /// <returns>list of meetings</returns>
        public XmlNodeList GetMyMeetings()
        {
            var meetingDoc = Request("report-my-meetings", null);

            return meetingDoc.SelectNodes("results/my-meetings/meeting");
        }

        /// <summary>
        /// Return all meetings on server.
        /// </summary>
        /// <returns>List of meetings</returns>
        public XmlNodeList GetAllMeetings()
        {
            var sco_id = Request("sco-shortcuts", "").SelectSingleNode("results/shortcuts/sco[@type='user-meetings']/@sco-id").Value;

            return Request("sco-expanded-contents", "sco-id=" + sco_id + "&filter-type=meeting").SelectNodes("results/expanded-scos/sco");
        }

        /// <summary>
        /// Return all courses in login context
        /// </summary>
        /// <returns>List of courses</returns>
        public XmlNodeList GetMyTrainings()
        {
            var trainingDoc = Request("report-my-training", null);
            return trainingDoc.SelectNodes(@"results/meeting");
        }

        /// <summary>
        /// Log out user
        /// </summary>
        public void Logout()
        {
            Request("logout", null);
            _bzsession = string.Empty;
        }

        /// <summary>
        /// Build and run Adobe Connect server requests.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected XmlElement Request(String action, String queryString)
        {
            if (_bzsession == "")
                Login();

            URL url = BreezeUrl(action, queryString);

            var conn = WebRequest.Create(url);
            conn.Headers.Set(@"Cookie", "BREEZESESSION=" + _bzsession);

            var resp = conn.GetResponse();
            var resultStream = resp.GetResponseStream();

            var doc = new XmlDocument();
            if (resultStream != null) doc.Load(resultStream);

            doc.DocumentElement.InnerXml = doc.InnerXml.Replace("<status code=\"ok\"/>", "");

            GetStatus(doc.DocumentElement); // check response status.

            return doc.DocumentElement;
        }

        /// <summary>
        /// Return information about shared content objects
        /// </summary>
        /// <param name="scoId"></param>
        /// <returns>sco information</returns>
        public XmlElement ScoInfo(string scoId)
        {
            return Request("sco-info", "sco-id=" + scoId);
        }

        /// <summary>
        /// Returns url of shared content object
        /// </summary>
        /// <param name="scoId"></param>
        /// <returns>url</returns>
        public string ScoUrl(String scoId)
        {
            var path = Request("sco-info", "sco-id=" + scoId).SelectSingleNode("//url-path").InnerText;
            var url = Request("sco-shortcuts", null).SelectSingleNode("//domain-name").InnerText;

            return url + "/" + path.Substring(1) + "?session=" + _bzsession;
        }

        /// <summary>
        /// Return Session id
        /// </summary>
        /// <returns>session id</returns>
        public string GetBreezesession()
        {
            if (_bzsession == "")
                Login();

            return _bzsession;
        }

        /// <summary>
        /// Build request url
        /// </summary>
        /// <param name="action"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected URL BreezeUrl(String action, String queryString)
        {
            return new URL(_baseUrl + "/api/xml?" + "action=" + action
                        + (queryString != null ? ('&' + queryString) : ""));
        }

        /// <summary>
        /// Delete user from Adobe Connect server.
        /// </summary>
        /// <param name="email"></param>
        public void DeleteUser(string email)
        {
            var principal_id = GetPrincipalIdByEmail(email);
            Request("principals-delete", "principal-id=" + principal_id + "&session=" + _bzsession);
        }

        private void GetStatus(XmlElement response)
        {
            var code = response.SelectSingleNode("results/status/@code").Value;
            if (code != "ok")
            {
                var subcode = response.SelectSingleNode("results/status/invalid/@subcode").Value;
                throw new Exception("request: " + code + " -> reason: " + subcode);
            }
        }
    }
}