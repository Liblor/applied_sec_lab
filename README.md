# applied_sec_lab
Applied Security Laboratory - AS19

## Build status

### Web server
[![Build Status](https://travis-ci.com/Liblor/applied_sec_lab.svg?token=v2htoQjxNh7zAtUbzeQt&branch=master)](https://travis-ci.com/Liblor/applied_sec_lab)

### Core CA
TODO

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
