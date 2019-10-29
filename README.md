# applied_sec_lab
Applied Security Laboratory - AS19

## Build status

### Web server
[![Build Status](https://travis-ci.com/Liblor/applied_sec_lab.svg?token=v2htoQjxNh7zAtUbzeQt&branch=master)](https://travis-ci.com/Liblor/applied_sec_lab)

### Core CA
#### Ansible Certificate Generation
Rough overview over the certificate generation by ansible:
- **Root CA keys and certificates** are generated in `/vagrant/key_store/iMovies_Root_CA_key.pem`. This folder is considered to be offline, because we will remove this shared folder, together with all vagrant users, for the final production environment as the last step of ansible. The Root CA certificate is installed on all machines in `/usr/share/ca-certificates/mozilla/iMovies_Root_CA.crt` with a link in `/etc/ssl/certs/iMovies_Root_CA.crt` like all other root certificates.
- **Intermediate CA keys** are generated on the certservers in `/home/coreca/pki/private/iMovies_<hostname>_Intermediate_CA_key.pem` then a CSR is sent to the config server and signed with the Root CA key, the resulting certificate is transferred back to the certserver and stored in `/home/coreca/pki/certs/iMovies_<hostname>_Intermediate_CA.crt`.
- **TLS keys** are generated on each host in `/home/<username>/pki/private/iMovies_aslcert01_tls_key.pem`, then a CSR is sent over the config server to the first certserver (aslcert01), which signs a certificate with the intermediate key. The resulting certificate is sent back over the config server to the respective servers and stored in the public folder `/etc/pki/tls/certs/iMovies_aslcert01_tls.crt`.

The ansible script will not regenerate those keys every time it is run. Instead it can be constructed to regenerate the keys as follows:
- Regenerate only TLS keys:
```bash
ansible-playbook -e "FORCE_TLS_CERT_REGEN=true" -i production site.yml
```
- Regenerate intermediate keys and TLS keys (the latter have to be regenerated because they were signed by the old intermediate keys)
```bash
ansible-playbook -e "FORCE_INTERMEDIATE_CA_CERT_REGEN=true" -i production site.yml
```
- Regenerate root CA keys and all other keys
```bash
ansible-playbook -e "FORCE_ROOT_CA_CERT_REGEN=true" -i production site.yml
```

**Important note:
The old keys are not automatically revoked! This would have to be done manually in a case of key compromise.**

## Install environment
### Vagrant
Vagrant creates all VMs automatically, configures their networks and initializes the ansible master server.

To install, do the following:
- Install [vagrant](https://www.vagrantup.com/)
- Run `vagrant up` inside this repository

To stop the VMs:
- `vagrant halt`

To reinstall the architecture:
- Run `vagrant destroy` (add option `-f` to destroy all VMs without asking)
- Run `vagrant up`

View running instances:
- `vagrant status`

Connect to an instance:
- `vagrant ssh <name>`

### Ansible
```
TODO
```

### Certificate generation
We use OpenSSL to generate the certificates required for client certificate based authentication.

First, we create the CA's RSA keypair:
```
openssl genrsa -out ca.key 4096
```

We generate the CA's certificate using the key as follows (entering the various data fields when prompted):
```
openssl req -new -x509 -key ca.key -out ca.crt
```

Another keypair is created for the client certificate:
```
openssl genrsa -out client.key 4096
```

Followed by a Certificate Signing Request:

```
openssl req -new -key client.key -out client.csr
```

Before signing the CSR, create a config file (in this case, `client-ext.conf`) with the following contents:
```ini
[v3_ca]
basicConstraints = CA:FALSE
extendedKeyUsage=clientAuth,emailProtection
```

To sign the CSR:
```
openssl x509 -req -in client.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out client.crt -extensions v3_ca -extfile client-ext.conf
```

For a server-side TLS cert, begin by generating yet another keypair and CSR:
```
openssl genrsa -out webserver.key 4096

openssl req -new -key webserver.key -out webserver.csr
```

Before siging the CSR, create a config file (e.g. `server-ext.conf`) with the following contents:
```ini
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

# Modify the domain name as required
[alt_names]
DNS.1 = localhost
```

To sign the CSR:
```
openssl x509 -req -in webserver.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out webserver.crt -extfile server-ext.conf
```

To export the certificate and private key info into PKCS#12:
```
openssl pkcs12 -export -out client.pfx -inkey client.key -in client.crt
```
