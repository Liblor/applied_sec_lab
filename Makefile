CLEAR_COLOR := \x1b[0m
PASS_COLOR := \x1b[32;01m

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

.PHONY: build
build: purge update_box
	vagrant up

.PHONY: push
push:
	@echo -e "[${PASS_COLOR}push${CLEAR_COLOR}] Prepare environment"
	@rm -rf ./vagrant_share/CertServer.tar.gz ./vagrant_share/WebServer.tar.gz
	@echo -e "[${PASS_COLOR}push${CLEAR_COLOR}] Building CertServer"
	@cd CertServer/ \
		&& dotnet publish --configuration=Releases --verbosity q -o ../vagrant_share/CertServer \
		&& cd ../vagrant_share \
		&& tar -czf ./CertServer.tar.gz ./CertServer \
		&& rm -r ./CertServer \
		&& cd ../
	@echo -e "[${PASS_COLOR}push${CLEAR_COLOR}] Building WebServer"
	@cd WebServer/ \
		&& dotnet publish --configuration=Releases --verbosity q -o ../vagrant_share/WebServer \
		&& cd ../vagrant_share \
		&& tar -czf ./WebServer.tar.gz ./WebServer \
		&& rm -r ./WebServer \
		&& cd ../
	@echo -e "[${PASS_COLOR}push${CLEAR_COLOR}] Deploying new builds"
	@vagrant ssh aslans01 -- 'sudo su ansible -c /vagrant/scripts/deploy_builds.sh'
	@echo -e "[${PASS_COLOR}push${CLEAR_COLOR}] Cleaning up"
	@rm -rf ./vagrant_share/CertServer.tar.gz ./vagrant_share/WebServer.tar.gz
