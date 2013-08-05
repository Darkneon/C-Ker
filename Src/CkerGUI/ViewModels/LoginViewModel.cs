using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Cker.Models;
using SoftArcs.WPFSmartLibrary.MVVMCommands;
using SoftArcs.WPFSmartLibrary.MVVMCore;
using SoftArcs.WPFSmartLibrary.SmartUserControls;

namespace CkerGUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        // Fields
        #region Fields

        List<User> userList;

        private readonly string userImagesPath = @"\Images";

        #endregion

        // Constructors
        #region Constructors

        public LoginViewModel()
        {
            if (ViewModelHelper.IsInDesignModeStatic == false)
            {
                this.initializeAllCommands();

                // This displays the corresponding user image when the correct username is typed into the field.
                // It is not necessarily the most secure feature in the world.
                this.getAllUser();
            }
        }

        #endregion

        // Public Properties
        #region Public Properties

        public string UserName
        {
            get { return GetValue(() => UserName); }
            set
            {
                SetValue(() => UserName, value);

                this.UserImageSource = this.getUserImagePath();
            }
        }

        public string Password
        {
            get { return GetValue(() => Password); }
            set { SetValue(() => Password, value); }
        }

        public string UserImageSource
        {
            get { return GetValue(() => UserImageSource); }
            set { SetValue(() => UserImageSource, value); }
        }

        #endregion

        // Submit Command Handler
        #region Submit Command Handler

        public ICommand SubmitCommand { get; private set; }

        private void ExecuteSubmit(object commandParameter)
        {
            var accessControlSystem = commandParameter as SmartLoginOverlay;

            if (accessControlSystem != null)
            {
                if (this.validateUser(this.UserName, this.Password) == true)
                {
                    accessControlSystem.Unlock();

                    // Store the current user so we can check the type later.
                    var currentUser = findUser(this.UserName, this.Password);
                    Debug.Assert(currentUser != null, "Logged in a null user.");

                    Application.Current.Properties["CurrentUser"] = currentUser;
                }
                else
                {
                    accessControlSystem.ShowWrongCredentialsMessage();
                }
            }
        }

        private bool CanExecuteSubmit(object commandParameter)
        {
            return !string.IsNullOrEmpty(this.Password);
        }

        #endregion

        // Private Methods
        #region Private Methods

        private void initializeAllCommands()
        {
            this.SubmitCommand = new ActionCommand(this.ExecuteSubmit, this.CanExecuteSubmit);
        }

        private void getAllUser()
        {
            // Place for implementation of code for retrieval of user data, ideally from a database.
            // Currently, the app utilizes a local collection.
            this.userList = new List<User>()
								 {
									new User() { Type = UserType.Administrator, UserName="admin", Password="fullaccess",
											     ImageSourcePath = Path.Combine( userImagesPath, "admin.png") },
									new User() { Type = UserType.Operator, UserName="operator", Password="gimpedaccess",
												 ImageSourcePath = Path.Combine( userImagesPath, "operator.png") },
								 };
        }

        private bool validateUser(string username, string password)
        {
            // Place for implementation of code for credentials validation.
            User validatedUser = findUser(username, password);
            return validatedUser != null;
        }

        private User findUser(string username, string password)
        {
            return this.userList.FirstOrDefault(user => user.UserName.Equals(username) &&
                                                        user.Password.Equals(password));
        }

        private string getUserImagePath()
        {
            User currentUser = this.userList.FirstOrDefault(user => user.UserName.Equals(this.UserName));

            if (currentUser != null)
            {
                return currentUser.ImageSourcePath;
            }

            return String.Empty;
        }

        #endregion
    }
}

