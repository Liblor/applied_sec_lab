#!/usr/bin/python3

import requests
import json
import copy

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
url_change_pw = f"{protocol}://{host}/api/changepassword"

print("\n" + "-"*80 + "\n")
response_swagger = requests.get(url_swagger, verify=False)
print("Test get swagger json: ", end="")
test_response(response_swagger, 200)

print("\n" + "-"*80 + "\n")
response_ciphersuites = requests.get(url_ciphersuite, verify=False)
print("Test get cipher suites: ", end="")
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

payload_rsa_invalid_cipher_suite = copy.deepcopy(payload_rsa)
payload_rsa_invalid_cipher_suite["requestedCipherSuite"]["KeySize"] = 1024

payload_rsa_invalid_uname = copy.deepcopy(payload_rsa)
payload_rsa_invalid_uname["uid"] = "nonExistantUID"

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

payload = json.dumps(payload_rsa_invalid_cipher_suite)
response_rsa_invalid_cipher_suite = requests.post(url_issue, headers=headers, data=payload, verify=False)
print("Test issue RSA certificate with invalid cipher suite: ", end="")
test_response(response_rsa_invalid_cipher_suite, 400)

payload = json.dumps(payload_rsa_invalid_uname)
response_rsa_invalid_uname = requests.post(url_issue, headers=headers, data=payload, verify=False)
print("Test issue RSA certificate with invalid user: ", end="")
test_response(response_rsa_invalid_uname, 401)

payload = json.dumps(payload_ecdsa)
response_issue_ecdsa = requests.post(url_issue, headers=headers, data=payload, verify=False)
print("Test issue ECDSA certificate: ", end="")
test_response(response_issue_ecdsa, 200)

print("\n" + "-"*80 + "\n")
payload_valid = {
    "uid": "lb"
}

payload_invalid = {
    "uid": "nonExistantUID"
}

payload = json.dumps(payload_valid)
response_revoke = requests.post(url_revoke, headers=headers, data=payload, verify=False)
print("Test revoke certificate: ", end="")
test_response(response_revoke, 204)

payload = json.dumps(payload_invalid)
response_revoke = requests.post(url_revoke, headers=headers, data=payload, verify=False)
print("Test revoke certificate with invalid uid: ", end="")
test_response(response_revoke, 400)

print("\n" + "-"*80 + "\n")

response_crl = requests.get(url_crl, verify=False)
print("Test get crl: ", end="")
test_response(response_crl, 200)

print("\n" + "-"*80 + "\n")

payload_lb = {
    "uid": "lb",
    "password": "D15Licz6"
}

payload = json.dumps(payload_lb)
response_dl_priv = requests.post(url_dl_priv, headers=headers, data=payload, verify=False)
print("Test private key download: ", end="")
test_response(response_dl_priv, 200)

print("\n" + "-"*80 + "\n")

newPw = "NG3xk0zp"
oldPw = "D15Licz6"
weakPw = "strawberry"

payload_invalid = {
    "uid": "lb",
    "oldPassword": "invalidPW",
    "newPassword": newPw
}

payload_weak = {
    "uid": "lb",
    "oldPassword": oldPw,
    "newPassword": weakPw
}

payload_valid = {
    "uid": "lb",
    "oldPassword": oldPw,
    "newPassword": newPw
}

payload_restore = {
    "uid": "lb",
    "oldPassword": newPw,
    "newPassword": oldPw
}

payload = json.dumps(payload_invalid)
response_change_pw = requests.post(url_change_pw, headers=headers, data=payload, verify=False)
print("Test invalid password change: ", end="")
test_response(response_change_pw, 401)

payload = json.dumps(payload_weak)
response_change_pw_weak = requests.post(url_change_pw, headers=headers, data=payload, verify=False)
print("Test to change to a weak password: ", end="")
test_response(response_change_pw_weak, 400)

payload = json.dumps(payload_valid)
response_change_pw = requests.post(url_change_pw, headers=headers, data=payload, verify=False)
print("Test valid password change: ", end="")
test_response(response_change_pw, 204)

payload = json.dumps(payload_restore)
response_change_pw = requests.post(url_change_pw, headers=headers, data=payload, verify=False)
print("Restore old password: ", end="")
test_response(response_change_pw, 204)

print("\n" + "-"*80 + "\n")
