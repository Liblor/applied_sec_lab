.PHONY: init_submodules
init_submodules:
	git submodule update --init --recursive

.PHONY: update_submodules
update_submodules:
	git submodule update --remote

.PHONY: vnetwork
vnetwork:
	vagrant up aslcert01 asldb01 aslweb01 aslans01

.PHONY: client
client:
	vagrant up aslclient01

.PHONY: purge
purge:
	vagrant destroy -f
