#!/usr/bin/python3

# Quick and dirty script to get a CA admin certificate through the CertServer logic
# (as opposed to manually building and signing it)
# Intended to run directly on the cert server box since outside clients require TLS certs

import base64
import json
import os
import requests
import sys

protocol = "http"
host = "localhost:5000"
url_issue = f"{protocol}://{host}/api/issue"

headers = {
    "accept": "*/*",
    "Content-Type": "application/json"
}

issue_payload = {
    "uid": "ms",
    "password": "MidbSvlJ",
    "certPassphrase": "correct horse battery staple",
    "requestedCipherSuite": {
        "Alg": "ECDSA",
        "HashAlg": "SHA512",
        "KeySize": 521
    }
}

if (__name__ == "__main__"):
    payload = json.dumps(issue_payload)
    response = requests.post(url_issue, headers=headers, data=payload, verify=False)

    if not response.ok:
        raise RuntimeError('Certificate request failed.')

    with open(sys.argv[1], "wb") as out_file:
        response_json = json.loads(response.content.decode("utf-8"))
        pfx_bytes = base64.b64decode(response_json["pkcs12Archive"])
        out_file.write(pfx_bytes)

    del response
