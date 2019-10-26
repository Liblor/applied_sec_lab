.PHONY: init_submodules
init_submodules:
	git submodule update --init --recursive

.PHONY: update_submodules
update_submodules:
	git submodule update --remote
