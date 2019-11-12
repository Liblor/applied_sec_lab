RELEASE := 1
DEBUG   := 2

CLEAR_COLOR := \x1b[0m
ERROR_COLOR := \x1b[31;01m

ifeq ($(BUILD_TYPE),)
	BUILD_TYPE := $(DEBUG)
endif

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

ifeq ($(BUILD_TYPE), $(DEBUG))
.PHONY: up
up:
	@printf 'Build for development.\n'
	@printf '${ERROR_COLOR}Vagrant setup will NOT be purged after install.\n'
	@printf "Use 'BUILD_TYPE=${RELEASE} vagrant up' to purge Vagrant.${CLEAR_COLOR}\n"
	vagrant up
else
.PHONY: up
up:
	@printf 'Build for release.\n'
	PURGE_VAGRANT="true" vagrant up
endif

.PHONY: build
build: purge update_box up
