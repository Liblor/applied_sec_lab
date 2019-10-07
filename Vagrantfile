# -*- mode: ruby -*-
# vi: set ft=ruby :

VAGRANTFILE_API_VERSION = "2"
VB_INTRANET_NAME = "asl_intranet"
OS_BOX = "generic/debian10"

ANSIBLE_PASSPHRASE_FILE = "ansible_passphrase.txt"
ANSIBLE_UNAME = "ansible"
ANSIBLE_REMOTE_TMP_PW = "ygqD-jh3LII1oNhurzQwAhoYe"

MASTER_MEM = 1024
REMOTE_MEM = 512
CPU_CAP_PERCENTAGE = 60
VRAM = 8

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
  "certservers" => {
      "aslcert01" => { :ip => "10.0.0.21" },
      # "aslcert02" => { :ip => "10.0.0.22" }
  },
  # "dbserver" => {
  #     "aslcertDB01" => { :ip => "10.0.0.23" },
  #     "asllegDB01" => { :ip => "10.0.0.51" },
  #     "asllegDB02" => { :ip => "10.0.0.52" }
  # },
  "webservers" => {
      "aslweb01" => { :ip => "10.0.0.31" },
      # "aslweb02" => { :ip => "10.0.0.32" }
  },
  # "ldservers" => {
  #     "aslld01" => { :ip => "10.0.0.41" }
  # },
  # "logservers" => {
  #     "asllog01" => { :ip => "10.0.0.61" },
  # },
  # "bkpservers" => {
  #     "aslbkp01" => { :ip => "10.0.0.71" }
  # }
}

# TODO: Create client outside company network for testing
# clients = {}

Vagrant.configure(VAGRANTFILE_API_VERSION) do |config|
    config.vm.provider "virtualbox"
    config.vagrant.plugins = "vagrant-vbguest"
    # Set correct locale for guests to prevent annoying errors
    ENV['LC_ALL']="en_US.UTF-8"

    # Limit resource usage
    config.vm.provider "virtualbox" do |vb|
        vb.customize ["modifyvm", :id, "--vram", VRAM]
    end # provider

    # Create hosts
    hosts.each do |category_name, category_hosts|
        category_hosts.each do |hostname, info|
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
                hosts.each do |peer_category_name, peer_category_hosts|
                    peer_category_hosts.each do |peer_hostname, peer_info|
                        hostconf.vm.provision "shell", inline: <<-SHELL
                            # Add hostname
                            echo "#{peer_info[:ip]} #{peer_hostname}" | sudo tee -a /etc/hosts
                        SHELL
                    end # host_peer_category.each
                end # hosts.each (peer)

                hostconf.vm.provider "virtualbox" do |vb|
                    vb.customize ["modifyvm", :id, "--cpuexecutioncap", CPU_CAP_PERCENTAGE]
                    vb.customize ["modifyvm", :id, "--name", "#{hostname}"]
                    vb.memory = REMOTE_MEM
                end # provider
            end # hostconf
        end # category_hosts.each
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

            hostconf.vm.provision "shell", inline: <<-SHELL
                sudo cp -r /vagrant/ansible/* "/home/#{ANSIBLE_UNAME}"
                sudo sh -c 'echo "###################################################" > "/home/#{ANSIBLE_UNAME}/production"'
                sudo echo "# This file is automatically generated by vagrant #" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
                sudo echo "###################################################" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
            SHELL

            # Add hostnames, install SSH keys
            hosts.each do |category_name, category_hosts|
                hostconf.vm.provision "shell", inline: <<-SHELL
                    echo -e "\n[#{category_name}]" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
                SHELL

                category_hosts.each do |hostname, info|
                    hostconf.vm.provision "shell", inline: <<-SHELL
                        # Add hostname
                        echo "#{info[:ip]} #{hostname}" | sudo tee -a /etc/hosts
                        echo "#{info[:ip]} # #{hostname}" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"

                        # Add ssh key to remote servers
                        sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh-copy-id -i /home/#{ANSIBLE_UNAME}/.ssh/id_rsa.pub -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname}

                        # Remove temporary password for ansible users of remote servers again
                        sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh #{ANSIBLE_UNAME}@#{hostname} "sudo passwd -d #{ANSIBLE_UNAME}; sudo usermod --lock #{ANSIBLE_UNAME}"

                        # Accept host keys for ansible user
                        sudo su - ansible -c 'sshpass -P "Enter" -p $(cat /vagrant/#{ANSIBLE_PASSPHRASE_FILE}) ssh -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname} exit'

                        # Remove sensitive data from history
                        history -c
                        unset HISTFILE
                        rm -f ~/.bash_history
                    SHELL
                end # hosts
            end # category_hosts.each

            hostconf.vm.provision "shell", inline: <<-SHELL
                sudo su - #{ANSIBLE_UNAME} -c 'eval "$(ssh-agent -s)"; sshpass -P "Enter" -p $(cat /vagrant/ansible_passphrase.txt) ssh-add ~/.ssh/id_rsa; ansible-galaxy install nginxinc.nginx; ansible-playbook -i production site.yml'
            SHELL

            hostconf.vm.provider "virtualbox" do |vb|
                vb.customize ["modifyvm", :id, "--name", "#{hostname}"]
                vb.memory = MASTER_MEM
            end # provider
        end # hostconf
    end # master.each

    # Create clients with VirtualBox GUI
    # TODO

end # config
