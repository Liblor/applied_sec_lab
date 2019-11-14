.PHONY: submodules
submodules:
	git submodule update --init --recursive
	git submodule update --remote

.PHONY: update_box
update_box:
	vagrant box update --force

.PHONY: vnetwork
vnetwork:
	vagrant up asllog01 asldb01 aslcert01 aslweb01 aslans01

.PHONY: client
client:
	vagrant up aslclient01

.PHONY: purge
purge:
	vagrant destroy -f

.PHONY: halt
halt:
	vagrant ssh asldb02 -c "sudo systemctl stop mariadb"
	vagrant halt

.PHONY: build
build: purge update_box
	vagrant up
