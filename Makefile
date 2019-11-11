.PHONY: init_submodules
init_submodules:
	git submodule update --init --recursive

.PHONY: update_submodules
update_submodules:
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

.PHONY: build
build: purge update_box
	# XXX: Change to the following after development:
	# PURGE_VAGRANT="true" vagrant up
	vagrant up
