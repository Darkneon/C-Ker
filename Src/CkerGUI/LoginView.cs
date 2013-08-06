using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Cker.Authentication;
using Cker.Models;
using SoftArcs.WPFSmartLibrary.MVVMCommands;
using SoftArcs.WPFSmartLibrary.MVVMCore;
using SoftArcs.WPFSmartLibrary.SmartUserControls;

namespace CkerGUI
{
    public class LoginView : ViewModelBase
    {
        // Paths to the user type display images.
        private readonly string administratorImagePath = @"\Images\admin.png";
        private readonly string operatorImagePath = @"\Images\operator.png";

        // Handler for when the user enters data.
        public ICommand SubmitCommand { get; private set; }

        // The user name field entered by the user.
        public string UserNameField
        {
            get { return GetValue(() => UserNameField); }
            set
            {
                SetValue(() => UserNameField, value);

                // This displays the corresponding user image when the correct username is typed into the field.
                // It is not necessarily the most secure feature in the world.
                this.UserImageSource = this.GetUserImagePath( UserNameField );
            }
        }

        // The password field entered by the user.
        public string PasswordField
        {
            get { return GetValue(() => PasswordField); }
            set { SetValue(() => PasswordField, value); }
        }

        // The path to the display image.
        public string UserImageSource
        {
            get { return GetValue(() => UserImageSource); }
            set { SetValue(() => UserImageSource, value); }
        }

        public LoginView()
        {
            if (ViewModelHelper.IsInDesignModeStatic == false)
            {
                this.InitializeAllCommands();
            }
        }

        private void ExecuteSubmit(object commandParameter)
        {
            var accessControlSystem = commandParameter as SmartLoginOverlay;

            if (accessControlSystem != null)
            {
                if (Authenticator.Login(this.UserNameField, this.PasswordField) == true)
                {
                    accessControlSystem.Unlock();
                }
                else
                {
                    accessControlSystem.ShowWrongCredentialsMessage();
                }
            }
        }

        private bool CanExecuteSubmit(object commandParameter)
        {
            return !string.IsNullOrEmpty(this.PasswordField);
        }

        private void InitializeAllCommands()
        {
            this.SubmitCommand = new ActionCommand(this.ExecuteSubmit, this.CanExecuteSubmit);
        }

        private string GetUserImagePath(string userName)
        {
            User user = Authenticator.FindUser(userName);

            if (user != null)
            {
                return user.Type == UserType.Administrator ? administratorImagePath : operatorImagePath;
            }

            return String.Empty;
        }
    }
}

