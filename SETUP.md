# ASL Project of Group 1 - the real iMovies

Please read this whole document before starting.

Most importantly, all credentials you need to explore the functionality of the system are provided below in a dedicated section.

**See the troubleshooting section below in case you need help.**

## Setup

Install and start the virtual machines in the following order:
* aslans01
* aslcert01
* [aslcert02]
* asldb01
* [asldb02]
* aslweb01
* [aslweb02]
* aslld01
* asllog01
* aslbkp01
* aslclient01

Shut them down in the reverse order. This is especially important for the
database servers (asldb02 asldb01), as the synchronization might break
otherwise and it will need manual intervention.

Note that the VMs with the name `asl*02` are for the redundant system only, they are not required to
find the backdoors and to run the setup.

In the following we will explain how to access the machines.

## Access the user interface

The client machine (`aslclient01`) is pre-configured and can simply be used to access the web UI.
Start the client machine and login as `user`.
Then click the `Login` launcher on the desktop.
There you can login as one of the users provided in the legacy database from the assignment.

## Access the CA administrator panel

Unfortunately the deadline was strict, so we did not have time to explain this section.
It's there, promised!

Okay, one hint.
Only the user `ms` has administrative permissions.

## Access the network as Admin

The Ansible master is the central configuration server.
Its SSH port is exposed to the public Internet (here: the public network).
Admins can use this server to control other hosts in the network and to access them.

The client machine can be used to access Ansible master.
Simply login as user `admin` using the VirtualBox UI, and issue the following command in a terminal.

```
ssh -i ~/.ssh/imovies_aslans01 admin@aslans01
```

In order to access other machines, you have to switch to the user `ansible` using `sudo su ansible`.

To access the Ansible master from your own host, SSH into `aslans01` as `admin`.
This can be done using the provided private SSH key only.
We provide you the following command for convenience.
You will have to find the missing environment variable `$ANSIBLE_MASTER_IP` yourself.

Hint: You were already provided with some other way to access this machine.

```
ssh \
	-o UserKnownHostsFile=/dev/null \
	-o StrictHostKeyChecking=no \
	-o IdentitiesOnly=yes \
	-i "./imovies_aslans01" \
	admin@$ANSIBLE_MASTER_IP
```

## Credentials for using the system

### User passwords
```
Host: aslclient01
Username: user
Password: password

Host: aslclient01
Username: admin
Password: admin
```

### CA administrator certificates

The client certificate for the CA administrator is located on the client machine at `/home/admin/caadmin_cert_with_priv_key_aslans01.pfx`.
The password for the certificate is `correct horse battery staple`.
In case this password does not work, check your keyboard layout (y <-> z) and make sure that you type the spaces correctly.

## Credentials for managing the system

### User passwords
```
Host: aslans01
Username: admin
Password: CKLwhksWCuLzFWXRqoZdMinUfVj_aS
```

### Private keys
```
Host: aslans01
Username: ansible
Location: ~/.ssh/id_rsa
Passphrase: UURf_Uko5s0qDxEkhKkX0A6lGAJO1WzWWy2XJMJd
```

### Borg backup repository passphrases
```
aslcert01: t4YT3duRqCAjw7KVewM2JZxTcpQ7Pm
aslcert02: CjshGpKEeJVHnvRt5Q9W75zCaBGeSg
asldb01:   0TxHbU7NEpDBCWr95ZpXbBiF4pb9uh
aslans01:  DFcox3FrYnVyRUWjKrhpBy8qPoM6CH
asllog01:  7FLnAnajiNcfqcCJPcY2tu9LvuHEFu
```

### MySQL database
```
root:      Zfm-u5abCoR4VwuubR.XR8GPMy6BVM
aslcert01: qecyKAFp7_xwYYbc8t6b!5VtGV,9Zu
aslcert02: _e6DBAffv7bN2whs6aXNM57p-RDbvY
aslweb01:  L-MVpXEWLNykPf..4Ji-cPxbfZNJKL
aslweb02:  H,maSD9v7riNthPf8AJv-,hpZZJufv
backup:    iZ4x0ZXIFZzXs4SDWwy1O3VlxUi9k0OV
```

### iMovies employees

The users in the system are as has been defined in the legacy database assignment.
Only the `ms` got promoted to an administrator.

## VM Requirements

- 1024 MiB for the client VM
- 512 MiB for other VMs

If you have more RAM available, feel free to add some RAM in the VirtualBox settings for a faster environment.

## Troubleshooting

In case you have trouble installing the setup or need help otherwise, mail us at `help@imovies.ch` (yes, for real).
Our office hours are from Monday to Friday, 10am to 5pm.
