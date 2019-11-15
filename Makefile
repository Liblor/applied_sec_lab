RELEASE := 1
DEBUG   := 2

CLEAR_COLOR := \x1b[0m
PASS_COLOR  := \x1b[32;01m
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

.PHONY: halt
halt:
	vagrant ssh asldb02 -c "sudo systemctl stop mariadb"
	vagrant halt

ifeq ($(BUILD_TYPE), $(DEBUG))
.PHONY: up
up: purge update_box
	@printf 'Build for development.\n'
	@printf "${ERROR_COLOR}Vagrant setup will NOT be purged after install.\n"
	@printf "Use 'BUILD_TYPE=${RELEASE} vagrant up' to purge Vagrant.${CLEAR_COLOR}\n"
	vagrant up
else
.PHONY: up
up: purge update_box
	@printf 'Build for release.\n'
	PURGE_VAGRANT="true" vagrant up
	@printf 'Remove shared folders\n'
	./scripts/remove-shared-folders.sh
endif

.PHONY: push
push:
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Prepare environment'
	@rm -rf ./vagrant_share/CertServer.tar.gz ./vagrant_share/WebServer.tar.gz
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Building CertServer'
	@cd CertServer/ \
		&& dotnet publish --configuration=Releases --verbosity q -o ../vagrant_share/CertServer \
		&& cd ../vagrant_share \
		&& tar -czf ./CertServer.tar.gz ./CertServer \
		&& rm -r ./CertServer \
		&& cd ../
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Building WebServer'
	@cd WebServer/ \
		&& dotnet publish --configuration=Releases --verbosity q -o ../vagrant_share/WebServer \
		&& cd ../vagrant_share \
		&& tar -czf ./WebServer.tar.gz ./WebServer \
		&& rm -r ./WebServer \
		&& cd ../
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s builds\n" 'Deploying new'
	@vagrant ssh aslans01 -- 'sudo su ansible -c /vagrant/scripts/deploy_builds.sh'
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Cleaning up'
	@rm -rf ./vagrant_share/CertServer.tar.gz ./vagrant_share/WebServer.tar.gz
