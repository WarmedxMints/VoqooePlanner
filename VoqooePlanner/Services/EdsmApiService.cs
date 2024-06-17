using System.Net.Http;
using System.Net.Http.Json;

namespace VoqooePlanner.Services
{
    public struct EdsmBodyResponse
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public sealed class EdsmApiService(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;

        public async Task<string?> GetSystemUrlAsync(long address)
        {
            var content = await httpClient.GetFromJsonAsync<EdsmBodyResponse>($"bodies?systemId64={address}");

            if(content.Url is null)
                return null;
            return content.Url.Replace("&", "^&");
        }
    }
}
