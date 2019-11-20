using CoreCA.DataModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace CoreCA.Client
{

    public class CoreCAClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        private HttpContent Serialize(object obj) => new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, MediaTypeNames.Application.Json);
        private T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

        public CoreCAClient(HttpClient httpClient, ILogger<CoreCAClient> logger)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<byte[]> GetCRL()
        {
            var response = await _httpClient.GetAsync("/api/CRL");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> DownloadPrivateKey(string uid, string userPassword)
        {
            var request = new DownloadPrivateKeyRequest
            {
                Uid = uid,
                Password = userPassword
            };

            var response = await _httpClient.PostAsync("/api/DownloadPrivateKey", Serialize(request));
            if (response.IsSuccessStatusCode)
                return Deserialize<UserCertificate>(await response.Content.ReadAsStringAsync()).Pkcs12Archive;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                _logger.LogWarning("New certificate request failed (unauthorized).");

            return null;
        }


        public async Task<string> RequestNewCertificate(string uid, string userPassword, string certPassphrase, CipherSuite cipherSuite = null)
        {
            if (cipherSuite == null)
            {
                var cipherSuitesResponse = await _httpClient.GetAsync("/api/CipherSuites");
                if (!cipherSuitesResponse.IsSuccessStatusCode)
                    return null;

                cipherSuite = JsonConvert.DeserializeObject<CipherSuite[]>(await cipherSuitesResponse.Content.ReadAsStringAsync()).FirstOrDefault();
                if (cipherSuite == null)
                    return null;
            }

            var request = new CertRequest
            {
                Uid = uid,
                Password = userPassword,
                CertPassphrase = certPassphrase,
                RequestedCipherSuite = cipherSuite
            };

            var response = await _httpClient.PostAsync("/api/Issue", Serialize(request));
            if (response.IsSuccessStatusCode)
                return Deserialize<UserCertificate>(await response.Content.ReadAsStringAsync()).Pkcs12Archive;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                _logger.LogWarning("New certificate request failed (unauthorized).");
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                _logger.LogWarning("New certificate request failed (bad request).");

            return null;
        }

        public async Task<bool> RevokeCertificate(string uid)
        {
            var request = new RevokeRequest { Uid = uid };

            var response = await _httpClient.PostAsync("/api/Revoke", Serialize(request));
            // No response content applicable - simply return whether the status code indicates successful revocation.
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePassword(string uid, string oldPassword, string newPassword)
        {
            var request = new PasswordChangeRequest
            {
                Uid = uid,
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            var response = await _httpClient.PostAsync("/api/ChangePassword", Serialize(request));

            return response.IsSuccessStatusCode;
        }

        public async Task<HealthCheckResult> CheckHealth()
        {
            var response = await _httpClient.GetAsync("/health");

            if (!response.IsSuccessStatusCode)
                return HealthCheckResult.Unhealthy("CertServer reported Unhealthy");

            string content = await response.Content.ReadAsStringAsync();

            if (content == nameof(HealthCheckResult.Degraded))
                return HealthCheckResult.Degraded("CertServer reported Degraded");

            return HealthCheckResult.Healthy();
        }
    }
}
