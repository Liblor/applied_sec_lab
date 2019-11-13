#!/usr/bin/env bash

cs_archive='/vagrant/CertServer.tar.gz'
ws_archive='/vagrant/WebServer.tar.gz'

if ! [ -f "$cs_archive" -a -f "$ws_archive" ] ; then
	echo "Cannot find builds." >&2
	exit 1
fi

eval "$(ssh-agent -s)"
sshpass -P "Enter" -p "$(cat /vagrant/ansible_passphrase.txt)" ssh-add ~/.ssh/id_rsa

function deploy {
	local from=$1
	local to=$2
	local host=$3
	local service=$4

	scp "$from" "$host":~
	ssh "$host" "
	sudo tar --overwrite --strip-components 2 -xf $(basename $from) -C $to;
	rm $(basename $from);
	sudo systemctl restart $service
	"
}

echo "Deploying to aslcert01 ..."
deploy "$cs_archive" '/home/coreca/app' 'aslcert01' 'asl-certserver.service'
echo "Deploying to aslweb01 ..."
deploy "$ws_archive" '/home/webserver/app' 'aslweb01' 'asl-webserver.service'
