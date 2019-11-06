# -*- mode: ruby -*-
# vi: set ft=ruby :

VAGRANTFILE_API_VERSION = "2"
VB_INTRANET_NAME = "asl_intranet"
# Simulate "public internet" clients through a different VirtualBox virtual network
VB_PUBLIC_NET_NAME = "asl_public_net"
OS_BOX = "generic/debian10"

ANSIBLE_PASSPHRASE_FILE = "ansible_passphrase.txt"
ANSIBLE_UNAME = "ansible"
ANSIBLE_REMOTE_TMP_PW = "ygqD-jh3LII1oNhurzQwAhoYe"

MASTER_MEM = 1024
REMOTE_MEM = 512
CPU_CAP_PERCENTAGE = 60
VRAM = 8
CLIENT_VRAM = 64

# List of all hosts
# Naming:
#   All internal machines have the prefix als (Applied Security Lab) and
#   two digits suffix, which is increment for multiple machines of the same type
#   Types:
#       cert    = Certificate Authority
#       db      = Database server
#       certDB  = Database for CA
#       web     = Web Server
#       ld      = Load Balancer
#       legDB   = Legacy Database
#       log     = Log Server
#       bkp     = Backup Server
master = {
    "ansservers" => {
        "aslans01" => { :ip => "10.0.0.11" },
    }
}

hosts = {
  "certservers" => {
      "aslcert01" => { :ip => "10.0.0.21" },
      # "aslcert02" => { :ip => "10.0.0.22" },
  },
  "dbservers" => {
      "asldb01" => { :ip => "10.0.0.31" },
      "asldb02" => { :ip => "10.0.0.32" },
  },
  "webservers" => {
      "aslweb01" => {
          :ip => "10.0.0.41",
          :public_net_ip => "172.16.0.41"
      },
      # "aslweb02" => { :ip => "10.0.0.42" },
  },
  # "ldservers" => {
  #     "aslld01" => { :ip => "10.0.0.51" },
  # },
  # "logservers" => {
  #     "asllog01" => { :ip => "10.0.0.61" },
  # },
  # "bkpservers" => {
  #     "aslbkp01" => { :ip => "10.0.0.71" },
  # }
}

# TODO: Create client outside company network for testing
clients = {
    "publicnetclients" => {
        "aslclient01" => { :public_net_ip => "172.16.0.11" },
    },
}

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

                if info.key?(:public_net_ip)
                    hostconf.vm.network "private_network",
                        ip: "#{info[:public_net_ip]}",
                        virtualbox__intnet: VB_PUBLIC_NET_NAME
                end

                hostconf.vm.provision "shell", inline: <<-SHELL
                    # Add ansible user
                    sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}
                    echo -e "#{ANSIBLE_REMOTE_TMP_PW}\n#{ANSIBLE_REMOTE_TMP_PW}" | sudo passwd #{ANSIBLE_UNAME}

                    # Allow password authentication for setting the machines up.
                    sudo sed -i "s|^PasswordAuthentication no$|PasswordAuthentication yes|" /etc/ssh/sshd_config
                    sudo systemctl reload sshd

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
    master.each do |master_category_name, master_category_hosts|
        master_category_hosts.each do |master_hostname, master_info|
            config.vm.define master_hostname do |hostconf|
                hostconf.vm.box = OS_BOX
                hostconf.vm.hostname = master_hostname
                hostconf.vm.network "private_network",
                    ip: "#{master_info[:ip]}",
                    virtualbox__intnet: VB_INTRANET_NAME
                hostconf.vm.synced_folder "./vagrant_share", "/vagrant", SharedFoldersEnableSymlinksCreate: false

                hostconf.vm.provision "shell", inline: <<-SHELL
                    # Install Ansible
                    sudo apt-get update
                    DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y ansible sshpass

                    # Add ansible user
                    sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}

                    # Give the ansible user passwordless sudo
                    echo '#{ANSIBLE_UNAME} ALL=(ALL) NOPASSWD: ALL' | sudo EDITOR='tee -a' visudo

                    # Generate SSH keys for ansible remote configuration
                    sudo su - #{ANSIBLE_UNAME} -c "ssh-keygen -t ed25519 -a 100 -f /home/#{ANSIBLE_UNAME}/.ssh/id_rsa -N $(cat /vagrant/#{ANSIBLE_PASSPHRASE_FILE})"
                SHELL

                hostconf.vm.provision "shell", inline: <<-SHELL
                    sudo cp -r /vagrant/ansible/* "/home/#{ANSIBLE_UNAME}"

                    echo '###################################################' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
                    echo '# This file is automatically generated by Vagrant #' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
                    echo '###################################################' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"

                    # Add Ansible host itself to inventory
                    echo -e '\n[#{master_category_name}]\nlocalhost ansible_connection=local' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
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
                            echo "#{hostname}" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"

                            # Add ssh key to remote servers
                            sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh-copy-id -i /home/#{ANSIBLE_UNAME}/.ssh/id_rsa -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname}

                            # Remove temporary password for ansible users of remote servers again
                            sudo sshpass -p "#{ANSIBLE_REMOTE_TMP_PW}" ssh #{ANSIBLE_UNAME}@#{hostname} "sudo passwd -d #{ANSIBLE_UNAME}; sudo usermod --lock #{ANSIBLE_UNAME}"

                            # Accept host keys for ansible user
                            sudo su - #{ANSIBLE_UNAME} -c 'sshpass -P "Enter" -p $(cat /vagrant/#{ANSIBLE_PASSPHRASE_FILE}) ssh -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname} exit'
                        SHELL
                    end # hosts
                end # category_hosts.each

                hostconf.vm.provision "shell", inline: <<-SHELL
                    sudo su - #{ANSIBLE_UNAME} -c 'eval "$(ssh-agent -s)" ; sshpass -P "Enter" -p $(cat /vagrant/#{ANSIBLE_PASSPHRASE_FILE}) ssh-add ~/.ssh/id_rsa ; ansible-galaxy install -r requirements.yml ; ansible-playbook -e "FORCE_ROOT_CA_CERT_REGEN=true" -i production site.yml --tags "all,setup" ; history -c ; unset HISTFILE ; rm -f ~/.bash_history'

                    # Remove sensitive data from history
                    history -c
                    unset HISTFILE
                    rm -f ~/.bash_history
                SHELL

                hostconf.vm.provider "virtualbox" do |vb|
                    vb.customize ["modifyvm", :id, "--name", "#{master_hostname}"]
                    vb.memory = MASTER_MEM
                end # provider
            end # hostconf
        end # master_category_hosts.each
    end # master.each

    # Create clients with VirtualBox GUI
    clients.each do |client_cat_name, client_cat_boxes|
        client_cat_boxes.each do |client_hostname, client_info|
            config.vm.define client_hostname do |clientconf|
                clientconf.vm.box = OS_BOX
                clientconf.vm.hostname = client_hostname
                clientconf.vm.network "private_network",
                    ip: "#{client_info[:public_net_ip]}",
                    virtualbox__intnet: VB_PUBLIC_NET_NAME
                clientconf.vm.synced_folder "./vagrant_share", "/vagrant", SharedFoldersEnableSymlinksCreate: false

                # Add public-net-connected host names
                hosts.each do |host_cat_name, host_cat_boxes|
                    host_cat_boxes.each do |host_name, host_info|
                        if host_info.key?(:public_net_ip)
                            clientconf.vm.provision "shell", inline: <<-SHELL
                                # Add hostname
                                echo "#{host_info[:public_net_ip]} #{host_name}" | sudo tee -a /etc/hosts
                            SHELL
                        end # if public_net_ip exists
                    end # host_peer_category.each
                end # hosts.each (peer)

                # Configure client machine hostname & GUI access
                clientconf.vm.provider "virtualbox" do |vb, override|
                    # Uncomment to launch VirtualBox GUI upon `vagrant up` (user:password = user:password)
                    vb.gui = true

                    vb.customize ["modifyvm", :id, "--vram", CLIENT_VRAM]
                    vb.customize ["modifyvm", :id, "--name", "#{client_hostname}"]
                end # virtualbox provider

                clientconf.vm.provision "shell", inline: <<-SHELL
                    # Add a normal user
                    sudo adduser --disabled-login --gecos "User" user
                    echo "user:password" | sudo chpasswd

                    # Upgrade all packages
                    sudo apt-get update
                    DEBIAN_FRONTEND=noninteractive sudo -E apt-get upgrade -y

                    # Install user interface and Firefox
                    DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y x-window-system lightdm xfce4 firefox-esr --no-install-recommends
                    sudo systemctl set-default graphical.target

                    # Install root certificate
                    sudo cp /vagrant/key_store/iMovies_Root_CA_crt.pem /usr/local/share/ca-certificates
                    sudo chown root: /usr/local/share/ca-certificates/iMovies_Root_CA_crt.pem
                    sudo update-ca-certificates

                    # Remove sensitive data from history
                    history -c
                    unset HISTFILE
                    rm -f ~/.bash_history

                    # Reboot to start to user interface
                    sudo reboot
                SHELL
            end # clientconf
        end # client_cat_boxes.each
    end # clients.each

end # config
