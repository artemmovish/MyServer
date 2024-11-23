using Xunit;
using System.Windows;
using WPFApplication.ViewModels;
using WPFApplication.Services;
using WPFApplication.Models;
using Moq;
namespace MyApp.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void CliclMode_ChangesTitleAndTextInButton()
        {
            var viewModel = new MainViewModel();

            viewModel.CliclMode.Execute(null);

            Assert.Equal("Регистрация", viewModel.Title);
            Assert.Equal("Аутентификация", viewModel.TextInButton);
        }

        [Fact]
        public void CheckCorrect_InvalidLogin_ShowsCorrectLoginVisibility()
        {
            var viewModel = new MainViewModel();
            viewModel.Login = "Lo";

            var result = viewModel.CheckCorrect();

            Assert.False(result);
            Assert.Equal(Visibility.Visible, viewModel.CorrectLogin);
        }

        [Fact]
        public void CheckCorrect_ValidLoginAndPassword_HidesVisibility()
        {
            var viewModel = new MainViewModel();
            viewModel.Login = "ValidLogin";
            viewModel.Password = "Valid123";

            var result = viewModel.CheckCorrect();

            Assert.True(result);
            Assert.Equal(Visibility.Hidden, viewModel.CorrectLogin);
            Assert.Equal(Visibility.Hidden, viewModel.CorrectPassword);
        }

        [Fact]
        [STAThread]
        public async Task ClickIn_AuthenticationSuccess_ClosesWindow()
        {
            var mockAuthClient = new Mock<AuthClient>("https://localhost:7100/connect");
            mockAuthClient.Setup(client => client.PostRequestAuthenticate())
                          .ReturnsAsync(new RequestResult { IsSuccess = true });

            var viewModel = new MainViewModel
            {
                authClient = mockAuthClient.Object,
                CloseAction = () => { }
            };
            viewModel.Login = "ValidLogin";
            viewModel.Password = "Valid123";

            await viewModel.onClicIn();

            Assert.Equal("", viewModel.ErrorText);
            mockAuthClient.Verify(client => client.PostRequestAuthenticate(), Times.Once);
        }

    }
}

