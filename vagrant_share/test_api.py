#!/usr/bin/python3

import requests
import json

host = "aslcert01:80"
# host = "localhost:5001"

protocol = "http"
# protocol = "https"

payload_rsa = {
    "uid": "ab",
    "password": "plain",
    "requestedCipherSuite": {
        "Alg": "RSA",
        "HashAlg": "SHA512",
        "KeySize": 4096
    }
}

payload_ecdsa = {
    "uid": "ab",
    "password": "plain",
    "requestedCipherSuite": {
        "Alg": "ECDSA",
        "HashAlg": "SHA512",
        "KeySize": 521
    }
}

payload = payload_ecdsa

headers =  {
    "accept": "*/*",
    "Content-Type": "application/json"
}

url_swagger = f"{protocol}://{host}/api/swagger/v1/swagger.json"
url_ciphersuite = f"{protocol}://{host}/api/ciphersuites"
url_issue = f"{protocol}://{host}/api/issue"
url_revoke = f"{protocol}://{host}/api/revoke"

payload = json.dumps(payload)
# response_swagger = requests.get(url_swagger, verify=False)
# print(response_swagger)
# print(response_swagger.text)

print("\n" + "-"*80 + "\n")

# response_ciphersuites = requests.get(url_ciphersuite, verify=False)
# print(response_ciphersuites)
# print(response_ciphersuites.text)

print("\n" + "-"*80 + "\n")

response_issue = requests.post(url_issue, headers=headers, data=payload, verify=False)
print(response_issue)
print(response_issue.headers)
print(response_issue.text)


print("\n" + "-"*80 + "\n")
payload = {
    "uid": "string",
    "password": "string",
    "serialNumber": 0
}

# payload = json.dumps(payload)
# response_revoke = requests.post(url_revoke, headers=headers, data=payload, verify=False)
# print(response_revoke)
# print(response_revoke.headers)
# print(response_revoke.text)
