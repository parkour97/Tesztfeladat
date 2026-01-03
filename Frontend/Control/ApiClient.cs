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

        public async Task<List<SystemUsage>> GetHistoryAsync(
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
                await _http.GetFromJsonAsync<List<SystemUsage>>(url);

            return result ?? new List<SystemUsage>();
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            var url = $"api/devices";

            var result =
                await _http.GetFromJsonAsync<List<Device>>(url);

            return result ?? new List<Device>();
        }

        public async Task<List<DeviceParam>> GetDeviceParamsAsync()
        {
            var url = $"api/deviceparams";

            var result =
                await _http.GetFromJsonAsync<List<DeviceParam>>(url);

            return result ?? new List<DeviceParam>();
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
