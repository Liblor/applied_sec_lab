[![Build Status](https://travis-ci.com/Liblor/applied_sec_lab.svg?token=v2htoQjxNh7zAtUbzeQt&branch=master)](https://travis-ci.com/Liblor/applied_sec_lab)

# applied_sec_lab
Applied Security Laboratory - AS19

## Details

### Web server

### Core CA
#### Ansible Certificate Generation

Rough overview over the certificate generation by ansible:
- **Root CA keys and certificates** are generated in `/vagrant/key_store/iMovies_Root_CA_key.pem`. This folder is considered to be offline, because we will remove this shared folder, together with all vagrant users, for the final production environment as the last step of ansible. The Root CA certificate is copied to all machines into `/usr/local/share/ca-certificates/iMovies_Root_CA.crt` and installed using `update-ca-certificates`.
- **Intermediate CA keys** are generated on the certservers in `/home/coreca/pki/private/iMovies_<purpose>_<hostname>_Intermediate_CA_key.pem`. Each core CA has two intermediate certificates for the following purposes:
  - **internal**: To sign TLS certificates for the internal infrastructure
  - **external**: To sign client certificates

  The reason for this is that the serial numbers of those certificates do not collide, as internal certificates are not in the DB. The CSR of those intermediate certificates is sent to the config server and signed with the Root CA key, the resulting certificate is transferred back to the certserver and stored in `/home/coreca/pki/certs/iMovies_<purpose>_<hostname>_Intermediate_CA.crt`.
- **TLS keys** are generated on each host in `/etc/ssl/trusted/private/iMovies_aslcert01_tls_key.pem`. All members of the group `tls-trusted` have access to this key. A CSR is created and sent over the config server to the first certserver (aslcert01), which signs a certificate with the intermediate key. The resulting certificate is sent back over the config server to the respective servers and stored in the public folder `/etc/pki/tls/certs/iMovies_aslcert01_tls.crt`.

Short summary of paths for normal servers (neither cert nor config servers):
- `/usr/local/share/ca-certificates/iMovies_Root_CA.crt` trusted root CA certificate
- `/etc/ssl/trusted/` private key material accessible to `tls-trusted` group
- `/etc/pki/tls/` publicly readable certificates and CRLs

The ansible script will not regenerate those keys every time it is run. Instead it can be instructed to regenerate the keys as follows:
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

#### Key revocation
Intermediate and/or TLS certificates are revoked automatically. A new CRL is generated with openssl every time new certificates are generated with ansible. One CRL for the intermediate certificates is signed by the Root CA, the CRL for the TLS certificates is signed by the intermediate certificate. Both are valid for 30 days and stored in `/etc/pki/tls/crl/`. **The concatenated CRL `/etc/pki/tls/crl/tls_crl_chain.pem` has to be checked everywhere TLS certificates are used.**

#### Private key backup
To recover private keys encrypted with the backup servers public key, transfer the encrypted file to a machine with access to the private key of the backup server and run:
```
openssl rsautl -decrypt -inkey /vagrant/key_store/iMovies_bkp_key.pem -in <encrypted key>.pem.enc -out key.pem -oaep
```

To extract a EC private key from a Pkcs12 certificate (to compare to the above), run:
```
openssl pkcs12 -in <archive name>.pfx -nocerts -out privateKey.pem
openssl ec -in privateKey.pem -out privateKey_dec.pem
md5sum privateKey_dec.pem
# Compare with the MD5 sum of the above key:
md5sum key.pem
```
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

To purge vagrant from all VMs, create a new instance with:
- `PURGE_VAGRANT="true" vagrant up`

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
