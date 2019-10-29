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

            _httpClient = httpClient;
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
            switch (response.StatusCode)
            {
                // TODO: reevaluate the pattern of (a) inferring wrong arguments and (b) throwing exceptions
                case HttpStatusCode.Unauthorized:
                    throw new ArgumentException(nameof(userPassword));

                case HttpStatusCode.OK:
                    return Deserialize<UserCertificate>(await response.Content.ReadAsStringAsync()).Pkcs12Archive;

                default:
                    return null;
            }
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

            switch(response.StatusCode)
            {
                // TODO: reevaluate the pattern of (a) inferring wrong arguments and (b) throwing exceptions
                case HttpStatusCode.BadRequest:
                    throw new ArgumentException(nameof(cipherSuite));

                case HttpStatusCode.Unauthorized:
                    throw new ArgumentException(nameof(userPassword));

                case HttpStatusCode.OK:
                    return Deserialize<UserCertificate>(await response.Content.ReadAsStringAsync()).Pkcs12Archive;

                default:
                    return null;
            }
        }

        public async Task<bool> RevokeCertificate(string uid)
        {
            var request = new RevokeRequest { Uid = uid };

            var response = await _httpClient.PostAsync("/api/Revoke", Serialize(request));
            // No response content applicable - simply return whether the status code indicates successful revocation.
            return response.IsSuccessStatusCode;
        }
    }
}
