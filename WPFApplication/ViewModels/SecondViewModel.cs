using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPFApplication.Command;
using WPFApplication.Models;
using WPFApplication.Services;
using WPFApplication.View;
using WPFApplication.ViewModels.Base;

namespace WPFApplication.ViewModels
{
    public class SecondViewModel : ViewModel
    {
        private ICommand _clickGetData;
        public ICommand ClickGetData => _clickGetData ??= new AsyncRelayCommand(onClickGetData);

        private ICommand _clickCheck;
        public ICommand ClickCheck => _clickCheck ??= new AsyncRelayCommand(onClickCheck);

        private ICommand _clickGetNewToken;
        public ICommand ClickGetNewToken => _clickGetNewToken ??= new AsyncRelayCommand(onClickGetNewToken);

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        private readonly AuthClient client;
        public SecondViewModel(AuthClient _client)
        {
            client = _client;
        }

        public async Task onClickGetData()
        {
            var user = await client.GetUsersAsync();
            Users = new ObservableCollection<User>(user);
        }

        public async Task onClickCheck()
        {
            if ( await client.ValidateTokenAsync())
            {
                MessageBox.Show("Токен действителен");
            }
            else
            {
                MessageBox.Show("Токен не действителен");
            }
        }

        public async Task onClickGetNewToken()
        {
            var response = await client.PostRequestAuthenticate();

            if (response.IsSuccess)
            {
                MessageBox.Show("Новый токен получен");
            }
            else
            {
                MessageBox.Show(response.Message);
            }
        }
    }
}
