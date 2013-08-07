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
        public void PresenterTest_Authentication()
        {
            // At first, no user is logged in yet.
            Assert.IsNull(Authenticator.CurrentUser, "User logged in without logging in.");

            // If a login is successful, then the user is logged in.
            bool isLoggedIn = Authenticator.Login("admin", "fullaccess");
            Assert.IsTrue(isLoggedIn, "Login failed when using corrent info.");
            Assert.IsNotNull(Authenticator.CurrentUser, "User not logged in after login.");
            Assert.AreEqual(Authenticator.CurrentUser.Name, "admin", "Current user info. does not match login info.");
        }

    }
}
