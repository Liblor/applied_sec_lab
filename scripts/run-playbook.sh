#!/usr/bin/env bash

TAG="$1"

if [ -z "$TAG" ] ; then
	echo "No tag given." >&2
	exit 1
fi

PROJECT_ROOT="${BASH_SOURCE%/*}/.."

source "$PROJECT_ROOT/settings.env"

CLIENT=aslclient01
ANSIBLE_MASTER=aslans01
ANSIBLE_MASTER_IP=$(cat $PROJECT_ROOT/Vagrantfile | awk "/$ANSIBLE_MASTER/,0" | grep 'public_net_ip' | head -1 | cut -f2 -d'"')

cleanup_script="\
	exec ssh-agent bash -c '\
	sshpass -P Enter -p \$(cat $ANSIBLE_PASSPHRASE_FILE) ssh-add ~/.ssh/id_rsa;\
	ansible-playbook -i ~/production ~/site.yml --tags $TAG\
	'\
"

ssh \
	-o UserKnownHostsFile=/dev/null \
	-o StrictHostKeyChecking=no \
	-i "./vagrant_share/sshkey_store/$CLIENT/imovies_$ANSIBLE_MASTER" \
	"$ADMIN_UNAME@$ANSIBLE_MASTER_IP" <<-EOF
	echo $ADMIN_REMOTE_PASSWORD | sudo -S -i -u $ANSIBLE_UNAME bash -c "$cleanup_script"
EOF
