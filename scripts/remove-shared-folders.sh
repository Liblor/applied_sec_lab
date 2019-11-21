#!/usr/bin/env bash

for vm in `VBoxManage list vms | tac | awk '{print $1}'`;
do
	vm=$(echo "$vm" | tr -d '"');

	if ! [[ "$vm" =~ ^asl.* ]]
	then
		continue
	fi

	printf "Halting $vm\n"
	VBoxManage controlvm "$vm" acpipowerbutton 2> /dev/null

	# Remove shared folder vagrant if it exists, silent error if it doesn't
	VBoxManage sharedfolder remove "$vm" --name vagrant 2> /dev/null
done
