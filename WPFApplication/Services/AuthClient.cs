using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using WPFApplication.Models;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace WPFApplication.Services
{
    public class AuthClient
    {
        private readonly string _baseUrl;
        private string _username;
        private string _password;
        private readonly HttpClient _httpClient;
        private string Token;

        public AuthClient(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _httpClient = new HttpClient();
        }

        public AuthClient(string baseUrl)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _httpClient = new HttpClient();
        }

        public void AddAuthData(string username, string password)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public virtual async Task<RequestResult> PostRequestRegister()
        {
            object body = new
            {
                Username = _username,
                Password = _password
            };

            string url = $"{_baseUrl}/register";

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("grant_type", "password"));

            foreach (var property in body.GetType().GetProperties())
            {
                var value = property.GetValue(body)?.ToString();
                if (value != null)
                {
                    parameters.Add(new KeyValuePair<string, string>(property.Name.ToLower(), value));
                }
            }

            var content = new FormUrlEncodedContent(parameters);

            var response = await SendRequestAsync(url, content);

            return response;
        }

        public virtual async Task<RequestResult> PostRequestAuthenticate()
        {
            object body = new
            {
                Username = _username,
                Password = _password
            };

            string url = $"{_baseUrl}/Authenticate";

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("grant_type", "password"));

            foreach (var property in body.GetType().GetProperties())
            {
                var value = property.GetValue(body)?.ToString();
                if (value != null)
                {
                    parameters.Add(new KeyValuePair<string, string>(property.Name.ToLower(), value));
                }
            }

            var content = new FormUrlEncodedContent(parameters);

            var response = await SendRequestAsync(url, content);

            return response;
        }

        public virtual async Task<List<User>> GetUsersAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/getuser");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<User>>();

                if (users != null)
                {
                    return users;
                }
            }
            else
            {
                Console.WriteLine($"Ошибка получения пользователей. Статус код: {response.StatusCode}");
                return new List<User>();
            }

            return new List<User>();
        }

        public virtual async Task<RequestResult> SendRequestAsync(string url, FormUrlEncodedContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Token = await GetTokenAsync(response);
                    return new RequestResult
                    {
                        IsSuccess = true,
                        StatusCode = (int)response.StatusCode,
                        Message = "Успешно"
                    };
                }
                else
                {
                    Token = await GetTokenAsync(response);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);

                    return new RequestResult
                    {
                        IsSuccess = false,
                        Message = $"Ошибка от сервера: {(int)response.StatusCode} {errorData?.Error}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult
                {
                    IsSuccess = false,
                    Message = $"Сервер недоступен: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                return new RequestResult
                {
                    IsSuccess = false,
                    Message = $"Запрос был отменен или произошел таймаут: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RequestResult
                {
                    IsSuccess = false,
                    Message = $"Произошла непредвиденная ошибка: {ex.Message}"
                };
            }
            return null;
        }

        public virtual async Task<string> GetTokenAsync(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            return tokenResponse.access_token;
        }

        public virtual async Task<bool> ValidateTokenAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/validate-token");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
    
}
