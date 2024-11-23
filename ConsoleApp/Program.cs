using System;
using System.Security.Cryptography;
using System.Text;


using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Generic;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var baseUrl = "https://localhost:7100/connect";
            var username = "artem";
            var password = "08022007artem";

            Console.Read();

            var authClient = new AuthClient(baseUrl, username, password);

            //Console.Read();

            var response = await authClient.PostRequestRegister(); // Используем await

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Успех PostRequestRegister");
            }
            else
            {
                Console.WriteLine(response.RequestMessage + "\n-----");

                response = await authClient.PostRequestAuthenticate();
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Успех PostRequestAuthenticate");
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                }
            }

            Console.WriteLine(response.RequestMessage + "\n-----");


            var users = await authClient.GetUsersAsync();

            foreach (var user in users)
            {
                Console.WriteLine(user.Login, user.Password);
            }

            Console.ReadLine();
        }
    }

    public class AuthClient
    {
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly HttpClient _httpClient;
        private string Token;

        public AuthClient(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> PostRequestRegister()
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

            var response = await _httpClient.PostAsync(url, content);

            Token = await GetTokenAsync(response);
            return response;
        }

        public async Task<HttpResponseMessage> PostRequestAuthenticate()
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

            var response = await _httpClient.PostAsync(url, content);

            Token = await GetTokenAsync(response);

            return response;
        }

        public async Task<List<User>> GetUsersAsync()
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
                Console.WriteLine($"Failed to get users. Status code: {response.StatusCode}");
                return new List<User>();
            }

            return new List<User>();
        }

        public async Task<string> GetTokenAsync(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            return tokenResponse.access_token;
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }

}

