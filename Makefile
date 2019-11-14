CLEAR_COLOR := \x1b[0m
PASS_COLOR  := \x1b[32;01m
ERROR_COLOR := \x1b[31;01m

.PHONY: submodules
submodules:
	git submodule update --init --recursive
	git submodule update --remote

.PHONY: vnetwork
vnetwork:
	vagrant up asllog01 asldb01 aslcert01 aslweb01 aslans01

.PHONY: client
client:
	vagrant up aslclient01

.PHONY: clean
clean:
	@printf "[${PASS_COLOR}clean${CLEAR_COLOR}] %s\n" 'Destoying VMs'
	vagrant destroy -f
	@printf "[${PASS_COLOR}clean${CLEAR_COLOR}] %s\n" 'Removing SSH key store'
	rm -rf ./vagrant_share/sshkey_store/*

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
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Deploying new builds'
	@vagrant ssh aslans01 -- 'sudo su ansible -c /vagrant/scripts/deploy_builds.sh'
	@printf "[${PASS_COLOR}push${CLEAR_COLOR}] %s\n" 'Cleaning up'
	@rm -rf ./vagrant_share/CertServer.tar.gz ./vagrant_share/WebServer.tar.gz

.PHONY: build
build:
	@printf "${ERROR_COLOR}%s${CLEAR_COLOR}\n" 'Make sure to use "make release" for the final submission'
	@printf "${ERROR_COLOR}%s${CLEAR_COLOR}\n" 'To start a clean build, use "make clean build"'
	@printf "[${PASS_COLOR}build${CLEAR_COLOR}] %s\n" 'Updating base box'
	@vagrant box update --force
	@printf "[${PASS_COLOR}build${CLEAR_COLOR}] %s\n" 'Building new VMs'
	vagrant up
	@printf "[${PASS_COLOR}build${CLEAR_COLOR}] %s\n" 'Running hardening script'
	./scripts/run-playbook.sh hardening

.PHONY: cleanup
cleanup_vms:
	@printf "[${PASS_COLOR}cleanup${CLEAR_COLOR}] %s\n" 'Running cleanup script'
	./scripts/run-playbook.sh cleanup
	@printf "[${PASS_COLOR}cleanup${CLEAR_COLOR}] %s\n" 'Removing shared folders'
	./scripts/remove-shared-folders.sh

.PHONY: export
export:
	@printf "[${PASS_COLOR}export${CLEAR_COLOR}] %s\n" 'Preparing environment'
	rm -rf ./build
	mkdir ./build
	@printf "[${PASS_COLOR}export${CLEAR_COLOR}] %s\n" 'Exporting VMs'
	./scripts/export-vms.sh

.PHONY: release
release: clean build cleanup_vms export
