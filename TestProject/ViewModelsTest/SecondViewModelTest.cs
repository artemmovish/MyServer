using Xunit;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WPFApplication.ViewModels;
using WPFApplication.Services;
using WPFApplication.Models;
using System.Windows;

namespace TestProject.ViewModelsTest
{
    public class SecondViewModelTest
    {

        public SecondViewModelTest()
        {

        }

        [Fact]
        public async Task onClickGetData_FillsUsersCollection()
        {
            var mockAuthClient = new Mock<AuthClient>("https://localhost:7100/connect");
            mockAuthClient.Setup(client => client.GetUsersAsync())
                          .ReturnsAsync(new List<User>
                          {
                  new User { Id = 1, Login = "John Doe" },
                  new User { Id = 2, Login = "Jane Smith" }
                          });
            var viewModel = new SecondViewModel(mockAuthClient.Object);

            await viewModel.onClickGetData();

            Assert.Equal(2, viewModel.Users.Count);
            Assert.Equal("John Doe", viewModel.Users[0].Login);
            Assert.Equal("Jane Smith", viewModel.Users[1].Login);
        }

        [Fact]
        public async Task onClickCheck_TokenIsValid_ShowsValidMessage()
        {
            var mockAuthClient = new Mock<AuthClient>("https://localhost:7100/connect");

            mockAuthClient.Setup(client => client.ValidateTokenAsync()).ReturnsAsync(true);

            var viewModel = new SecondViewModel(mockAuthClient.Object);

            await viewModel.onClickCheck();
        }

        [Fact]
        public async Task onClickCheck_TokenIsInvalid_ShowsInvalidMessage()
        {
            var mockAuthClient = new Mock<AuthClient>("https://localhost:7100/connect");

            mockAuthClient.Setup(client => client.ValidateTokenAsync()).ReturnsAsync(false);

            var viewModel = new SecondViewModel(mockAuthClient.Object);

            await viewModel.onClickCheck();
        }

        [Fact]
        public async Task onClickGetNewToken_SuccessfulResponse_ShowsSuccessMessage()
        {
            var mockAuthClient = new Mock<AuthClient>("https://localhost:7100/connect");

            mockAuthClient.Setup(client => client.PostRequestAuthenticate())
              .ReturnsAsync(new RequestResult { IsSuccess = true });

            var viewModel = new SecondViewModel(mockAuthClient.Object);

            await viewModel.onClickGetNewToken();
        }
    }
}
