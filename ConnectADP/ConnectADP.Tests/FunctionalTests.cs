using System;
using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConnectADP;

namespace ConnectADP.Tests
{
    [TestClass]
    public class FunctionalTests
    {
        ConnectADP adapter;

        [TestInitialize]
        public void Setup()
        {
            adapter = new ConnectADP(@"",
               "", "", ""); // provide a valid domain, user name and passwrd

        }

        [TestMethod]
        public void LogoutFromAdobeConnet()
        {
            adapter.Logout();
        }

        [TestMethod]
        public void CreateNewUser()
        {
            adapter.CreateUser("Romaine", "Carter", "xpwiz@hotmail.com", "foooze");
        }


        [TestMethod]
        public void GetMyMeetings()
        {
            adapter.GetMyMeetings();
            
            adapter.Logout();
        }

        [TestMethod]
        public void GetMyTrainings()
        {
            adapter.GetMyTrainings();
            adapter.Logout();
        }

        [TestMethod]
        public void GetUrlFromScoID()
        {
            adapter.ScoUrl("1092615392");
        }

        [TestMethod]
        public void GetScoInformation()
        {
            adapter.ScoInfo("1092615417");
        }

        [TestMethod]
        public void CreateMeeting()
        {
            adapter.CreateMeeting("new meeting", "2006-10-01T09:00", "2006-10-01T17:00", "foopath");
        }

        [TestMethod]
        public void GetAllMeetings()
        {
            adapter.GetAllMeetings();
        }

        [TestMethod]
        public void GetPrincipalIdByEmail()
        {
            adapter.GetPrincipalIdByEmail("xpwiz@hotmail.com");
        }

        [TestMethod]
        public void AddUserToMeeting()
        {
            var acl = adapter.GetAllMeetings()[2].Attributes["sco-id"].Value;
            adapter.AddUserToMeeting(acl, "xpwiz@hotmail.com", ConnectADP.MeetingRoles.Host);
        }

        [TestMethod]
        public void RemoveUserFromMeeting()
        {
            var acl = adapter.GetAllMeetings()[2].Attributes["sco-id"].Value;
            adapter.RemoveUserFromMeeting(acl, "xpwiz@hotmail.com");
        }

        [TestMethod]
        public void DeleteUser()
        {
            adapter.DeleteUser("xpwiz@hotmail.com");
        }

        [TestMethod]
        public void UpdateUser()
        {
            var principal_id = adapter.GetPrincipalIdByEmail("xpwiz@hotmail.com");
            adapter.UpdateUser(principal_id, lastName:"foo");
        }

        [TestMethod]
        public void UpdateUserPassword()
        {
            var principal_id = adapter.GetPrincipalIdByEmail("xpwiz@hotmail.com");
            adapter.UpdateUserPassword(principal_id, "foooze","romaine", "romaine");
        }

        [TestMethod]
        public void AddUserToBuiltInGroup()
        {
            adapter.AddUserToBuiltInGroup("xpwiz@hotmail.com", ConnectADP.BuiltInGoups.MeetingHosts);
        }

        [TestMethod]
        public void GetMyPrincipalId()
        {
            adapter.GetPrincipalId();
        }
    }
}
