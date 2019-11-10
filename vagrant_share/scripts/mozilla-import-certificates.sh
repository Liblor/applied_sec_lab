#!/bin/bash

### Script installs root.cert.pem to certificate trust store of applications using NSS
### (e.g. Firefox, Thunderbird, Chromium)
### Mozilla uses cert8, Chromium and Chrome use cert9
### Source: https://thomas-leister.de/en/how-to-import-ca-root-certificate/
certfile="/vagrant/key_store/iMovies_Root_CA.crt"
certname="iMovies Root CA"
homefolder="$1"

###
### For cert8 (legacy - DBM)
###

for certDB in $(find $homefolder -name "cert8.db")
do
    echo "Add certificate '${certname}' to $certDB"
    certdir=$(dirname ${certDB});
    certutil -A -n "${certname}" -t "TCu,Cu,Tu" -i ${certfile} -d dbm:${certdir}
done


###
### For cert9 (SQL)
###

for certDB in $(find $homefolder -name "cert9.db")
do
    echo "Add certificate '${certname}' to $certDB"
    certdir=$(dirname ${certDB});
    certutil -A -n "${certname}" -t "TCu,Cu,Tu" -i ${certfile} -d sql:${certdir}
done
