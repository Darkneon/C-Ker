using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Cker.Authentication;
using Cker.Models;
using Cker.Presenters;
using SoftArcs.WPFSmartLibrary.MVVMCommands;
using SoftArcs.WPFSmartLibrary.MVVMCore;
using SoftArcs.WPFSmartLibrary.SmartUserControls;

namespace CkerGUI
{
    public class LoginView : ViewModelBase
    {
        private LoginPresenter loginPresenter;

        // Action to perform when login is done.
        private Action loginCompleteAction;

        // The user name field entered by the user.
        public string UserNameField
        {
            get { return GetValue(() => UserNameField); }
            set
            {
                SetValue(() => UserNameField, value);

                // This displays the corresponding user image when the correct username is typed into the field.
                // It is not necessarily the most secure feature in the world.
                this.UserImageSource = loginPresenter.GetUserImagePath(UserNameField);
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

        // Handler for when the user enters data.
        public ICommand SubmitCommand { get; private set; }

        public LoginView()
        {
            if (ViewModelHelper.IsInDesignModeStatic == false)
            {
                InitializeCommands();
            }
            loginPresenter = new LoginPresenter();
            loginCompleteAction = delegate { };
        }

        /// <summary>
        /// Adds an action to perform when login is done.
        /// </summary>
        /// <param name="action"></param>
        public void AddLoginCompleteAction(Action action)
        {
            loginCompleteAction += action;
        }

        private void InitializeCommands()
        {
            this.SubmitCommand = new ActionCommand(this.ExecuteSubmit, this.CanExecuteSubmit);
        }

        private void ExecuteSubmit(object commandParameter)
        {
            var accessControlSystem = commandParameter as SmartLoginOverlay;

            if (accessControlSystem != null)
            {
                if (loginPresenter.Authenticate(this.UserNameField, this.PasswordField))
                {
                    accessControlSystem.Unlock();
                    loginCompleteAction();
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
    }
}

