using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cker.Models;

namespace Cker.Authentication
{
    public static class Authenticator
    {
        /// <summary>
        /// Access the current user that is logged in and authenticated.
        /// </summary>
        public static User CurrentUser { get; private set; }

        // Store all valid users
        private static List<User> validUsers;

        static Authenticator()
        {
            InitializeValidUsers();
        }

        /// <summary>
        /// Authenticates the specified user name and password.
        /// If successful, that user is logged in and set as the CurrentUser.
        /// Returns true if login successful; false if credentials are invalid.
        /// </summary>
        public static bool Login(string username, string password)
        {
            // Find user and store as the current logged in user.
            // Valid as long as not null.
            CurrentUser = FindUser(username, password);
            return CurrentUser != null;
        }

        /// <summary>
        /// Looks for the user with specified name and password.
        /// Returns the found user; null if not found.
        /// </summary>
        public static User FindUser(string username, string password)
        {
            return validUsers.FirstOrDefault(user => user.Name.Equals(username) && user.Password.Equals(password));
        }

        /// <summary>
        /// Looks for the user with specified name.
        /// Returns the first found user; null if not found.
        /// </summary>
        public static User FindUser(string username)
        {
            return validUsers.FirstOrDefault(user => user.Name.Equals(username));
        }

        public static void Logout() 
        {
            CurrentUser = null;
        }
        
        private static void InitializeValidUsers()
        {
            // Place for implementation of code for retrieval of user data, ideally from a database.
            // Currently, the app utilizes a local collection.
            validUsers = new List<User>()
						 {
						     new User() { Name="admin", Password="fullaccess", Type = UserType.Administrator },
						     new User() { Name="operator", Password="gimpedaccess", Type = UserType.Operator },
						 };
        }
    }
}
