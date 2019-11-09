﻿using CoreCA.DataModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace CoreCA.Client
{

    public class CoreCAClient
    {
        // TODO: consider centralizing the endpoint URLs (currently hardcoded per-method)
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        private HttpContent Serialize(object obj) => new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, MediaTypeNames.Application.Json);
        private T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

        public CoreCAClient(HttpClient httpClient, ILogger<CoreCAClient> logger)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _httpClient= httpClient;
            _logger = logger;
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
            // TODO: consider sharing predefined ciphersuites
            cipherSuite = cipherSuite ?? new CipherSuite
            {
                Alg = "RSA",
                HashAlg = "SHA512",
                KeySize = 4096
            };

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
    }
}
