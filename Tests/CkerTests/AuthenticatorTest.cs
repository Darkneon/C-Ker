using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Cker;
using Cker.Models;
using Cker.Presenters;
using Cker.Authentication;

namespace CKerTests
{
    public class AuthenticatorTest
    {
        /// <summary>
        /// Tests that login function works.
        /// </summary>

        [Test]
        public void FindUser_InvalidUsername_ReturnsNull()
        {
            string username = "InvalidUsername";

            User actual = Authenticator.FindUser(username);

            Assert.IsNull(actual);
        }

        [Test]
        public void FindUser_ValidUsername_ReturnsInstanciatedUserObject()
        {
            string username = "admin";

            User expected = new User();
            expected.Name = username;            

            User actual = Authenticator.FindUser(username);

            Assert.AreEqual(expected.ToString(), actual.ToString());

        }

        [Test]
        public void FindUser_InvalidUsernameAndPassword_ReturnsNull() 
        {
            string username = "InvalidUsername";
            string password = "InvalidPassword";

            User actual = Authenticator.FindUser(username, password);

            Assert.IsNull(actual);
        }

        [Test]
        public void FindUser_ValidUsernameAndPassword_ReturnsInstanciatedUserObject()
        {
            string username = "admin";
            string password = "fullaccess";

            User expected = new User();
            expected.Name = username;
            expected.Password = password;

            User actual = Authenticator.FindUser(username, password);

            Assert.AreEqual(expected.ToString(), actual.ToString());

        }
        
        [Test]
        public void Logout_AUserLoggedIn_ReturnsNull() 
        {
            Authenticator.Login("admin", "fullaccess");
            Authenticator.Logout();
            Assert.IsNull(Authenticator.CurrentUser);
        }

        [Test]
        public void Login_ValidUser_ReturnsMatchingUsernameAndPassword()
        {            
            Authenticator.Login("admin", "fullaccess");            

            string expected = "admin";
            string actual = Authenticator.CurrentUser.Name;
            Assert.AreEqual(expected, actual);

            expected = "fullaccess";
            actual = Authenticator.CurrentUser.Password;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Login_InvalidUser_ReturnsCurrentUserNull()
        {
            Authenticator.Login("adminfake", "fullaccessfake");
            Assert.IsNull(Authenticator.CurrentUser);          
        }   
    }
}
