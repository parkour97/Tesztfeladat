using Frontend.Models;
using static System.Net.WebRequestMethods;

namespace Frontend.Control
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<SystemUsageView>> GetHistoryAsync(
            int deviceId,
            DateTime from,
            DateTime to)
        {
            var url =
                $"api/systemusage" +
                $"?deviceId={deviceId}" +
                $"&from={Uri.EscapeDataString(from.ToString("O"))}" +
                $"&to={Uri.EscapeDataString(to.ToString("O"))}";

            var result =
                await _http.GetFromJsonAsync<List<SystemUsageView>>(url);

            return result ?? new List<SystemUsageView>();
        }

        public async Task<List<DeviceView>> GetDevicesAsync()
        {
            var url = $"api/devices";

            var result =
                await _http.GetFromJsonAsync<List<DeviceView>>(url);

            return result ?? new List<DeviceView>();
        }

        public async Task<List<DeviceParamView>> GetDeviceParamsAsync()
        {
            var url = $"api/deviceparams";

            var result =
                await _http.GetFromJsonAsync<List<DeviceParamView>>(url);

            return result ?? new List<DeviceParamView>();
        }

        public async Task<bool> SetDeviceParameterAsync(int deviceParameterId, int value, string username)
        {
            var request = new DeviceParamSend(deviceParameterId, value, username);

            var response = await _http.PostAsJsonAsync("api/devicevalue", request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var result = await _http.PostAsJsonAsync("api/auth/register",
                new { Username = username, Email = email, Password = password });

            return result.IsSuccessStatusCode;
        }

        public async Task<string?> LoginAsync(string usernameOrEmail, string password)
        {
            var result = await _http.PostAsJsonAsync("api/auth/login",
                new { UsernameOrEmail = usernameOrEmail, Password = password });

            if (!result.IsSuccessStatusCode) return null;

            var obj = await result.Content.ReadFromJsonAsync<LoginResponse>();
            return obj?.Token;
        }

        private record LoginResponse(string Token);
    }
}
