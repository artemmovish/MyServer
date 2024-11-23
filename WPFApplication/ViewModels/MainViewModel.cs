using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFApplication.ViewModels.Base;
using WPFApplication.Command;
using System.Windows.Controls;
using System.Windows.Input;
using WPFApplication.Services;
using System.Text.RegularExpressions;
using System.Net.Http;
using WPFApplication.View;
namespace WPFApplication.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public Action CloseAction { get; set; }

        private string _title = "Аутентификация";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _errorText = "";
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged();
            }
        }

        // Можно было просто присвоить ""
        public Visibility _correctLogin = Visibility.Hidden;
        public Visibility CorrectLogin
        {
            get { return _correctLogin; }
            set
            {
                _correctLogin = value; 
                OnPropertyChanged();
            }
        }
        private string _login = "Логин";
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public Visibility _correctPassword = Visibility.Hidden;
        public Visibility CorrectPassword
        {
            get { return _correctPassword; }
            set 
            {
                _correctPassword = value;
                OnPropertyChanged();
            }
        }
        private string _password = "Пароль";
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _textInButton = "Регистрация";
        public string TextInButton
        {
            get { return _textInButton; }
            set
            {
                _textInButton = value;
                OnPropertyChanged();
            }
        }

        private ICommand _cliclMode;
        public ICommand CliclMode => _cliclMode ??= new RelayCommand(onCliclMode);

        private ICommand _clickIn;
        public ICommand ClickIn => _clickIn ??= new AsyncRelayCommand(onClicIn);

        public AuthClient authClient;
        public MainViewModel()
        {
            CorrectLogin = Visibility.Hidden;
            CorrectPassword = Visibility.Hidden;
            authClient = new AuthClient("https://localhost:7100/connect");
        }

        private void onCliclMode()
        {
            if (Title == "Аутентификация")
            {
                Title = "Регистрация";
                TextInButton = "Аутентификация";
            }
            else
            {
                Title = "Аутентификация";
                TextInButton = "Регистрация";
            }
        }

        private async Task auth()
        {
            var response = await authClient.PostRequestAuthenticate();

            if (response.IsSuccess)
            {
                SynchronizationContext.Current.Post(_ =>
                {
                    SecondViewModel dataContext = new SecondViewModel(authClient);
                    SecondWindow window = new SecondWindow();
                    window.DataContext = dataContext;
                    window.Show();
                    CloseAction();
                }, null);

            }
            else
            {
                ErrorText = response.Message;            
            }
        }

        public async Task onClicIn()
        {
            if (CheckCorrect())
            {
                authClient.AddAuthData(Login, Password);

                if (Title == "Аутентификация")
                {
                    await auth();
                }
                else if (Title == "Регистрация")
                {
                    var response = await authClient.PostRequestRegister();

                    if (response.IsSuccess)
                    {
                        onCliclMode();
                        await auth();
                    }
                    else
                    {
                        ErrorText = response.Message;
                    }
                }
            }          
        }

        public bool CheckCorrect()
        {
            bool correct = true;
            if (Login.Length < 3 || Login == "Логин")
            {
                correct = false;
                CorrectLogin = Visibility.Visible;
            }
            else CorrectLogin = Visibility.Hidden;

            if (string.IsNullOrEmpty(Password) || Password.Length < 6 || !Regex.IsMatch(Password, @"^(?=.*[A-Za-z])"))
            {
                correct = false;
                CorrectPassword = Visibility.Visible;
            }
            else CorrectPassword = Visibility.Hidden;

            return correct;
        }
    }
}
