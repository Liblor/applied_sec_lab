# -*- mode: ruby -*-
# vi: set ft=ruby :

VAGRANTFILE_API_VERSION = "2"
VB_INTRANET_NAME = "asl_intranet"
OS_BOX = "generic/debian10"

ANSIBLE_PASSPHRASE_FILE = "ansible_passphrase.txt"
ANSIBLE_UNAME = "ansible"
ANSIBLE_REMOTE_TMP_PW = "ygqD-jh3LII1oNhurzQwAhoYe"

# List of all hosts
# Naming:
#   All internal machines have the prefix als (Applied Security Lab) and
#   two digits suffix, which is increment for multiple machines of the same type
#   Types:
#       cert    = Certificate Authority
#       certDB  = Database for CA
#       web     = Web Server
#       ld      = Load Balancer
#       legDB   = Legacy Database
#       log     = Log Server
#       bkp     = Backup Server
master = {
    "aslans01" => { :ip => "10.0.0.11" }
}

hosts = {
  "aslcert01" => { :ip => "10.0.0.21" },
  # "aslcert02" => { :ip => "10.0.0.22" },
  # "aslcertDB01" => { :ip => "10.0.0.23" },
  "aslweb01" => { :ip => "10.0.0.31" },
  # "aslweb02" => { :ip => "10.0.0.32" },
  # "aslld01" => { :ip => "10.0.0.41" },
  # "asllegDB01" => { :ip => "10.0.0.51" },
  # "asllegDB02" => { :ip => "10.0.0.52" },
  # "asllog01" => { :ip => "10.0.0.61" },
  # "aslbkp01" => { :ip => "10.0.0.71" },
}

# TODO: Create client outside company network for testing
# clients = {}

Vagrant.configure(VAGRANTFILE_API_VERSION) do |config|
    config.vm.provider "virtualbox"
    config.vagrant.plugins = "vagrant-vbguest"
    # Set correct locale for guests to prevent annoying errors
    ENV['LC_ALL']="en_US.UTF-8"

    # Create hosts
    hosts.each do |hostname, info|
        config.vm.define hostname do |hostconf|
            hostconf.vm.box = OS_BOX
            hostconf.vm.hostname = hostname
            hostconf.vm.network "private_network",
                ip: "#{info[:ip]}",
                virtualbox__intnet: VB_INTRANET_NAME

            hostconf.vm.provision "shell", inline: <<-SHELL
                # Install Ancible
                sudo apt-get update
                sudo apt-get install -y ansible

                # Add ansible user
                sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}
                echo -e "#{ANSIBLE_REMOTE_TMP_PW}\n#{ANSIBLE_REMOTE_TMP_PW}" | sudo passwd #{ANSIBLE_UNAME}

                # Give the ansible user passwordless sudo
                echo '#{ANSIBLE_UNAME} ALL=(ALL) NOPASSWD: ALL' | sudo EDITOR='tee -a' visudo

                # Remove sensitive data from history
                history -c
                unset HISTFILE
                rm -f ~/.bash_history
            SHELL

            # Add hostnames
            hosts.each do |peer_hostname, peer_info|
                hostconf.vm.provision "shell", inline: <<-SHELL
                    # Add hostname
                    echo "#{peer_info[:ip]} #{peer_hostname}" | sudo tee -a /etc/hosts
                SHELL
            end # hosts

            hostconf.vm.provider "virtualbox" do |vb|
                vb.customize ["modifyvm", :id, "--name", "#{hostname}"]
            end # provider
        end # hostconf
    end # hosts.each


    # Ansible master
    master.each do |hostname, info|
        config.vm.define hostname do |hostconf|
            hostconf.vm.box = OS_BOX
            hostconf.vm.hostname = hostname
            hostconf.vm.network "private_network",
                ip: "#{info[:ip]}",
                virtualbox__intnet: VB_INTRANET_NAME
            hostconf.vm.synced_folder "./vagrant_share", "/vagrant", SharedFoldersEnableSymlinksCreate: false

            hostconf.vm.provision "shell", inline: <<-SHELL
                # Install Ancible
                sudo apt-get update
                sudo apt-get install -y ansible sshpass

                # Add ansible user
                sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}

                # Generate SSH keys for ansible remote configuration
                sudo su #{ANSIBLE_UNAME} -c 'ssh-keygen -t ed25519 -a 100 -f $HOME/.ssh/id_rsa -N $(cat /vagrant/#{ANSIBLE_PASSPHRASE_FILE})'
            SHELL

            # Add hostnames, install SSH keys
            hosts.each do |hostname, info|
                hostconf.vm.provision "shell", inline: <<-SHELL
                    # Add hostname
                    echo "#{info[:ip]} #{hostname}" | sudo tee -a /etc/hosts

                    # Add ssh key to remote servers
                    sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh-copy-id -i /home/#{ANSIBLE_UNAME}/.ssh/id_rsa.pub -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname}

                    # Remove temporary password for ansible users of remote servers again
                    sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh #{ANSIBLE_UNAME}@#{hostname} "sudo passwd -d #{ANSIBLE_UNAME}"

                    # Remove sensitive data from history
                    history -c
                    unset HISTFILE
                    rm -f ~/.bash_history
                SHELL
            end # hosts

            hostconf.vm.provider "virtualbox" do |vb|
                vb.customize ["modifyvm", :id, "--name", "#{hostname}"]
            end # provider
        end # hostconf
    end # master.each


    # Create clients with VirtualBox GUI
    # TODO

end # config
