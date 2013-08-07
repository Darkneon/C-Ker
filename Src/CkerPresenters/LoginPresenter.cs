using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cker.Authentication;
using Cker.Models;

namespace Cker.Presenters
{
    /// <summary>
    /// Presenter allows the login view to get information needed to display.
    /// </summary>
    public class LoginPresenter
    {
        // Paths to the user type display images.
        public const string ADMIN_IMAGE_PATH = @"\Images\admin.png";
        public const string OPERATOR_IMAGE_PATH = @"\Images\operator.png";

        /// <summary>
        /// Authenticates the specified user with name and password.
        /// Returns true if valid; false otherwise.
        /// </summary>
        public bool Authenticate(string name, string password)
        {
            bool isValid = Authenticator.Login(name, password);
            return isValid;
        }

        /// <summary>
        /// Get the associated image path of the specified user name.
        /// </summary>
        public string GetUserImagePath(string userName)
        {
            User user = Authenticator.FindUser(userName);

            if (user != null)
            {
                return user.Type == UserType.Administrator ? ADMIN_IMAGE_PATH : OPERATOR_IMAGE_PATH;
            }

            return String.Empty;
        }
    }
}
