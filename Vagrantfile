# -*- mode: ruby -*-
# vi: set ft=ruby :

load File.join(File.dirname(__FILE__), 'settings.env')

VAGRANTFILE_API_VERSION = "2"

# List of all hosts
# Naming:
#  All internal machines have the prefix als (Applied Security Lab) and
#  two digits suffix, which is increment for multiple machines of the same type
#  Types:
#    cert = Certificate Authority
#    db   = Database server
#    web  = Web Server
#    ld   = Load Balancer
#    log  = Log Server
#    bkp  = Backup Server
master = {
	"ansservers" => {
		"aslans01" => {
			:ip => "172.17.0.11",
			:public_net_ip => "172.16.0.11"
		},
	}
}

hosts = {
	"certservers" => {
		"aslcert01" => { :ip => "172.17.0.21" },
		# "aslcert02" => { :ip => "172.17.0.22" },
	},
	"dbservers" => {
		"asldb01" => { :ip => "172.17.0.31" },
		# "asldb02" => { :ip => "172.17.0.32" },
	},
	"webservers" => {
		"aslweb01" => { :ip => "172.17.0.41" },
		# "aslweb02" => { :ip => "172.17.0.42" },
	},
	"ldservers" => {
		"aslld01" => {
			:ip => "172.17.0.51",
			:public_net_ip => "172.16.0.51",
		},
	},
	"logservers" => {
		"asllog01" => { :ip => "172.17.0.61" },
	},
	"bkpservers" => {
		"aslbkp01" => { :ip => "172.17.0.71" },
	}
}

clients = {
	"publicnetclients" => {
		"aslclient01" => { :public_net_ip => "172.16.0.81" },
	},
}

# aslans01 is 'hardcoded' as local host
backupclients = [hosts["certservers"], hosts["dbservers"], hosts["logservers"]]

Vagrant.configure(VAGRANTFILE_API_VERSION) do |config|
	config.vm.provider "virtualbox"
	config.vagrant.plugins = "vagrant-vbguest"

	# Set correct locale for guests to prevent annoying errors
	ENV['LC_ALL'] = "en_US.UTF-8"

	# Limit resource usage
	config.vm.provider "virtualbox" do |vb|
		vb.customize ["modifyvm", :id, "--cpuexecutioncap", CPU_CAP_PERCENTAGE]
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
					hostconf.vm.network :private_network, ip: "#{info[:public_net_ip]}"
				end

				hostconf.vm.provider "virtualbox" do |vb|
					vb.customize ["modifyvm", :id, "--name", "#{hostname}"]
					vb.customize ["modifyvm", :id, "--vram", VRAM_CAP]
					vb.memory = MEM_CAP
				end # provider

				hostconf.vm.provision "shell", inline: <<-SHELL
					# Add ansible user
					sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}
					echo "#{ANSIBLE_UNAME}:#{ANSIBLE_REMOTE_PASSWORD}" | sudo chpasswd

					# Give the ansible user passwordless sudo
					echo '#{ANSIBLE_UNAME} ALL=(ALL) NOPASSWD: ALL' | sudo EDITOR='tee -a' visudo

					# Remove root user password
					sudo passwd -d root

					#{REMOVE_SENSITIVE_DATA}
				SHELL

				# Add hostnames
				hosts.each do |peer_category_name, peer_category_hosts|
					peer_category_hosts.each do |peer_hostname, peer_info|
						hostconf.vm.provision "shell", inline: <<-SHELL
							# Add hostname
							echo "#{peer_info[:ip]} #{peer_hostname}" | sudo tee -a /etc/hosts
						SHELL

						if peer_info.key?(:public_net_ip)
							hostconf.vm.provision "shell", inline: <<-SHELL
								# Add hostname
								# use :ip and not :public_net_ip here intentionally! (internal hosts)
								echo "#{peer_info[:ip]} imovies.ch" | sudo tee -a /etc/hosts
								echo "#{peer_info[:ip]} www.imovies.ch" | sudo tee -a /etc/hosts
							SHELL
						end # if public_net_ip exists
					end # host_peer_category.each
				end # hosts.each (peer)
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

				if master_info.key?(:public_net_ip)
					hostconf.vm.network :private_network, ip: "#{master_info[:public_net_ip]}"
				end

				hostconf.vm.synced_folder "./vagrant_share", "/vagrant", SharedFoldersEnableSymlinksCreate: false

				hostconf.vm.provider "virtualbox" do |vb|
					vb.customize ["modifyvm", :id, "--name", "#{master_hostname}"]
					vb.customize ["modifyvm", :id, "--vram", VRAM_CAP]
					vb.memory = MEM_CAP
				end # provider

				hostconf.vm.provision "shell", inline: <<-SHELL
					# Install Ansible
					sudo apt-get update
					DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y ansible sshpass

					# Add ansible user
					sudo adduser --disabled-password --gecos "" #{ANSIBLE_UNAME}

					# Add user for remote admin
					sudo adduser --disabled-password --gecos "" #{ADMIN_UNAME}
					echo "#{ADMIN_UNAME}:#{ADMIN_REMOTE_PASSWORD}" | sudo chpasswd
					sudo usermod -aG sudo #{ADMIN_UNAME}

					# Give the ansible user passwordless sudo
					echo '#{ANSIBLE_UNAME} ALL=(ALL) NOPASSWD: ALL' | sudo EDITOR='tee -a' visudo

					# Remove root user password
					sudo passwd -d root

					# Generate SSH keys for ansible remote configuration
					sudo rm -f /home/#{ANSIBLE_UNAME}/.ssh/id_rsa
					sudo su - #{ANSIBLE_UNAME} -c "ssh-keygen -t ed25519 -a 100 -f /home/#{ANSIBLE_UNAME}/.ssh/id_rsa -N $(cat #{ANSIBLE_PASSPHRASE_FILE})"
				SHELL

				hostconf.vm.provision "shell", inline: <<-SHELL
					sudo cp -r /vagrant/ansible/* "/home/#{ANSIBLE_UNAME}"

					echo '###################################################' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
					echo '# This file is automatically generated by Vagrant #' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
					echo '###################################################' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"

					# Add Ansible host itself to inventory
					echo -e '\n[#{master_category_name}]\n#{master_hostname} ansible_connection=local' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
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
							sudo sshpass -p "#{ANSIBLE_REMOTE_PASSWORD}" ssh-copy-id -i /home/#{ANSIBLE_UNAME}/.ssh/id_rsa -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname}

							# Accept host keys for ansible user
							sudo su - #{ANSIBLE_UNAME} -c 'sshpass -P "Enter" -p $(cat #{ANSIBLE_PASSPHRASE_FILE}) ssh -o StrictHostKeyChecking=accept-new #{ANSIBLE_UNAME}@#{hostname} exit'

							# Remove temporary password for ansible users of remote servers again
							# Disable password authentication
							sudo sshpass -p "#{ANSIBLE_REMOTE_PASSWORD}" ssh #{ANSIBLE_UNAME}@#{hostname} <<-EOF
								sudo passwd -d #{ANSIBLE_UNAME}
								sudo usermod --lock #{ANSIBLE_UNAME}
							EOF
						SHELL
					end # hosts
				end # category_hosts.each

				# Add backupclients
				hostconf.vm.provision "shell", inline: <<-SHELL
				echo -e "\n[backupclients]" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
				# Add ansible master
				echo -e '#{master_hostname} ansible_connection=local' | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
				SHELL
				backupclients.each do |category|
					category.each do |hostname, info|
						hostconf.vm.provision "shell", inline: <<-SHELL
						# Add hostname
						echo "#{hostname}" | sudo tee -a "/home/#{ANSIBLE_UNAME}/production"
						SHELL
					end # hosts
				end # backupclients.each

				hostconf.vm.provision "shell", inline: <<-SHELL
					sudo -i -u #{ANSIBLE_UNAME} bash <<-EOF1
						exec ssh-agent bash -s 10<&0 << EOF2
							sshpass -P 'Enter' -p $(cat #{ANSIBLE_PASSPHRASE_FILE}) ssh-add ~/.ssh/id_rsa
							exec bash <&10-
						EOF2
						ansible-galaxy install --force -r requirements.yml
						ansible-playbook -e 'FORCE_ROOT_CA_CERT_REGEN=true' -i production site.yml --tags 'all, setup'
						#{REMOVE_SENSITIVE_DATA}
					EOF1
					#{REMOVE_SENSITIVE_DATA}
				SHELL
			end # hostconf
		end # master_category_hosts.each
	end # master.each

	# Create clients with VirtualBox GUI
	clients.each do |client_cat_name, client_cat_boxes|
		client_cat_boxes.each_with_index do |(client_hostname, client_info), client_index|
			config.vm.define client_hostname do |clientconf|
				clientconf.vm.box = OS_BOX
				clientconf.vm.hostname = client_hostname
				clientconf.vm.network :private_network, ip: "#{client_info[:public_net_ip]}"
				clientconf.vm.synced_folder "./vagrant_share", "/vagrant", SharedFoldersEnableSymlinksCreate: false

				# Add public-net-connected host names
				hosts.each do |host_cat_name, host_cat_boxes|
					host_cat_boxes.each do |host_name, host_info|
						if host_info.key?(:public_net_ip)
							clientconf.vm.provision "shell", inline: <<-SHELL
								# Add hostname
								echo "#{host_info[:public_net_ip]} imovies.ch" | sudo tee -a /etc/hosts
								echo "#{host_info[:public_net_ip]} www.imovies.ch" | sudo tee -a /etc/hosts
							SHELL
						end # if public_net_ip exists
					end # host_peer_category.each
				end # hosts.each (peer)

				# Configure client machine hostname & GUI access
				clientconf.vm.provider "virtualbox" do |vb, override|
					# Uncomment to launch VirtualBox GUI upon `vagrant up` (user:password = user:password)
					vb.gui = true

					vb.customize ["modifyvm", :id, "--vram", CLIENT_VRAM_CAP]
					vb.customize ["modifyvm", :id, "--name", "#{client_hostname}"]
					vb.memory = CLIENT_MEM_CAP
				end # virtualbox provider

				clientconf.vm.provision "shell", inline: <<-SHELL
					# Add user for administrating our infrastructure from remote
					sudo adduser --disabled-login --gecos "User" #{ADMIN_UNAME}
					echo "#{ADMIN_UNAME}:#{ADMIN_PASSWORD}" | sudo chpasswd

					# Add a normal user
					sudo adduser --disabled-login --gecos "User" #{CLIENT_UNAME}
					echo "#{CLIENT_UNAME}:#{CLIENT_PASSWORD}" | sudo chpasswd

					# Install sshpass
					DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y sshpass
				SHELL

				master.each do |master_category_name, master_category_hosts|
					master_category_hosts.each do |master_hostname, master_info|
						clientconf.vm.provision "shell", inline: <<-SHELL
							# Add hostname
							echo "#{master_info[:public_net_ip]} #{master_hostname}" | sudo tee -a /etc/hosts

							# Generate admin key material, install them on the config servers and
							# disable password login
							sudo -i -u #{ADMIN_UNAME} bash <<-EOF1
								mkdir ~/.ssh;
								chmod 700 ~/.ssh
								ssh-keygen -t ecdsa -b 521 -f ~/.ssh/imovies_#{master_hostname} -P '' -C '#{ADMIN_UNAME}@imovies.ch'
								sshpass -p '#{ADMIN_REMOTE_PASSWORD}' ssh-copy-id -i ~/.ssh/imovies_#{master_hostname} -o StrictHostKeyChecking=accept-new #{ADMIN_UNAME}@#{master_hostname}
								scp -i ~/.ssh/imovies_#{master_hostname} #{ADMIN_UNAME}@#{master_hostname}:/home/ansible/ca_admin_cert_with_priv_key.pfx ~/ca_admin_cert_with_priv_key_#{master_hostname}.pfx
							EOF1

							# Copy the generated key material to the shared directory.
							mkdir /vagrant/sshkey_store/#{client_hostname}
							sudo cp /home/#{ADMIN_UNAME}/.ssh/imovies_#{master_hostname} /vagrant/sshkey_store/#{client_hostname}/
						SHELL
					end # master_category_hosts.each
				end # master.each

				clientconf.vm.provision "shell", inline: <<-SHELL
					sudo /vagrant/scripts/setup-client.sh "#{CLIENT_UNAME}"

					#{REMOVE_SENSITIVE_DATA}
				SHELL
			end # clientconf
		end # client_cat_boxes.each
	end # clients.each
end # config
