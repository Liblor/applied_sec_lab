#!/usr/bin/env bash

PLAYBOOK="$1"

if [ -z "$PLAYBOOK" ] ; then
	echo "No playbook given." >&2
	exit 1
fi

PROJECT_ROOT="${BASH_SOURCE%/*}/.."

source "$PROJECT_ROOT/settings.env"

CLIENT=aslclient01
ANSIBLE_MASTER=aslans01
ANSIBLE_MASTER_IP=$(awk "/$ANSIBLE_MASTER/,0" $PROJECT_ROOT/Vagrantfile | grep 'public_net_ip' | head -1 | cut -f2 -d'"')

script="\
	exec ssh-agent bash -c '\
	sshpass -P Enter -p \$(cat $ANSIBLE_PASSPHRASE_FILE) ssh-add ~/.ssh/id_rsa;\
	ansible-playbook -i ~/production ~/$PLAYBOOK.yml;\
	$REMOVE_SENSITIVE_DATA\
	'\
"

ssh \
	-o UserKnownHostsFile=/dev/null \
	-o StrictHostKeyChecking=no \
	-o IdentitiesOnly=yes \
	-i "./vagrant_share/sshkey_store/$CLIENT/imovies_$ANSIBLE_MASTER" \
	"$ADMIN_UNAME@$ANSIBLE_MASTER_IP" <<-EOF
	echo $ADMIN_REMOTE_PASSWORD | sudo -S -i -u $ANSIBLE_UNAME bash -c "$script"
EOF
