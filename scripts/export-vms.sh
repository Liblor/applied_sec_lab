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

	printf "Exporting $vm\n"
	VBoxManage export "$vm" -o "./build/$vm.ova"
done
