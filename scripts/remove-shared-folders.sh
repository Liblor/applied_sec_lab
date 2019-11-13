#!/usr/bin/env bash

for vm in `VBoxManage list runningvms | awk '{print $1}'`;
do
  vm=$(echo "$vm" | tr -d '"');
  vagrant halt $vm;
  # Remove shared folder vagrant if it exists, silent error if it doesn't
  VBoxManage sharedfolder remove $vm --name vagrant 2> /dev/null;
  if [[ $vm =~ "aslclient" ]]
  then
     VBoxManage startvm $vm
  else
    VBoxManage startvm $vm --type headless
  fi
done
