# applied_sec_lab
Applied Security Laboratory - AS19

## Build status

### Web server
[![Build Status](https://travis-ci.com/Liblor/applied_sec_lab.svg?token=v2htoQjxNh7zAtUbzeQt&branch=master)](https://travis-ci.com/Liblor/applied_sec_lab)

### Core CA
TODO

## Install environment
### Vagrant
Vagrant creates all VMs automatically, configures their networks and initializes the ansible master server.

To install, do the following:
- Install [vagrant](https://www.vagrantup.com/)
- Run `vagrant up` inside this repository

To stop the VMs:
- `vagrant halt`

To reinstall the architecture:
- Run `vagrant destroy` (add option `-f` to destroy all VMs without asking)
- Run `vagrant up`

View running instances:
- `vagrant status`

Connect to an instance:
- `vagrant ssh <name>`

### Ansible
```
TODO
```
