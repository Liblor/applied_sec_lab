#!/usr/bin/python3

import requests
import json

def print_response(r):
    print(r)
    print(r.headers)
    print(r.text)

def test_response(response, expected_status_code):
    if response.status_code == expected_status_code:
        print("PASSED")
    else:
        print(f"FAILED (expected {expected_status_code})")
        print_response(response)

host = "aslcert01:80"
# host = "localhost:5001"

protocol = "http"
# protocol = "https"

headers =  {
    "accept": "*/*",
    "Content-Type": "application/json"
}

url_swagger = f"{protocol}://{host}/api/swagger/v1/swagger.json"
url_ciphersuite = f"{protocol}://{host}/api/ciphersuites"
url_issue = f"{protocol}://{host}/api/issue"
url_revoke = f"{protocol}://{host}/api/revoke"
url_crl = f"{protocol}://{host}/api/crl"
url_dl_priv = f"{protocol}://{host}/api/downloadprivatekey"

print("\n" + "-"*80)
response_swagger = requests.get(url_swagger, verify=False)
print("Test get swagger json: ", end="")
test_response(response_swagger, 200)

print("\n" + "-"*80)
response_ciphersuites = requests.get(url_ciphersuite, verify=False)
print("Test get swagger json: ", end="")
test_response(response_ciphersuites, 200)

print("\n" + "-"*80 + "\n")
payload_rsa = {
    "uid": "lb",
    "password": "D15Licz6",
    "certPassphrase": "someSecretPhrase",
    "requestedCipherSuite": {
        "Alg": "RSA",
        "HashAlg": "SHA512",
        "KeySize": 4096
    }
}

payload_ecdsa = {
    "uid": "lb",
    "password": "D15Licz6",
    "certPassphrase": "someSecretPhrase",
    "requestedCipherSuite": {
        "Alg": "ECDSA",
        "HashAlg": "SHA512",
        "KeySize": 521
    }
}

payload = json.dumps(payload_rsa)
response_issue_rsa = requests.post(url_issue, headers=headers, data=payload, verify=False)
print("Test issue RSA certificate: ", end="")
test_response(response_issue_rsa, 200)

payload = json.dumps(payload_ecdsa)
response_issue_ecdsa = requests.post(url_issue, headers=headers, data=payload, verify=False)
print("Test issue ECDSA certificate: ", end="")
test_response(response_issue_ecdsa, 200)

print("\n" + "-"*80 + "\n")
payload_ms = {
    "uid": "ms",
    "password": "MidbSvlJ",
    "serialNumber": 0
}

payload_lb = {
    "uid": "lb",
    "password": "D15Licz6",
    "serialNumber": 0
}

payload = json.dumps(payload_lb)
response_revoke = requests.post(url_revoke, headers=headers, data=payload, verify=False)
print("Test revoke certificate: ", end="")
test_response(response_revoke, 200)

payload = json.dumps(payload_ms)
response_revoke = requests.post(url_revoke, headers=headers, data=payload, verify=False)
print("Test revoke other user's certificate: ", end="")
test_response(response_revoke, 400)

print("\n" + "-"*80)
response_crl = requests.get(url_crl, verify=False)
print("Test get crl: ", end="")
test_response(response_crl, 200)

payload_lb = {
    "uid": "lb",
    "password": "D15Licz6"
}

payload = json.dumps(payload_lb)
response_dl_priv = requests.post(url_dl_priv, headers=headers, data=payload, verify=False)
print("Test private key download: ", end="")
test_response(response_dl_priv, 200)
print(response_dl_priv.text)
