# ASL Project of Group 1

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

Note: the 02 VMs are for the redundant system only, they are not required to
find the backdoors and to run the setup.


## Credentials

### Config Server aka aslans01 

In order to access other machines, ssh into aslans01 as `admin`, change to the
`ansible` using `sudo su ansible`. You can now access the other servers
using ssh.

#### admin user
```
Username: admin
Password: CKLwhksWCuLzFWXRqoZdMinUfVj_aS
```

#### ansible user
```
Passphrase for ~/.ssh/id_rsa: UURf_Uko5s0qDxEkhKkX0A6lGAJO1WzWWy2XJMJd
```

### Borg Backup:
```
aslcert01: t4YT3duRqCAjw7KVewM2JZxTcpQ7Pm
aslcert02: CjshGpKEeJVHnvRt5Q9W75zCaBGeSg
asldb01:   0TxHbU7NEpDBCWr95ZpXbBiF4pb9uh
aslans01:  DFcox3FrYnVyRUWjKrhpBy8qPoM6CH
asllog01:  7FLnAnajiNcfqcCJPcY2tu9LvuHEFu
```

### Mysql:
```
root: Zfm-u5abCoR4VwuubR.XR8GPMy6BVM
aslcert01: qecyKAFp7_xwYYbc8t6b!5VtGV,9Zu
aslcert02: "_e6DBAffv7bN2whs6aXNM57p-RDbvY"
aslweb01: "L-MVpXEWLNykPf..4Ji-cPxbfZNJKL"
aslweb02: H,maSD9v7riNthPf8AJv-,hpZZJufv
backup: iZ4x0ZXIFZzXs4SDWwy1O3VlxUi9k0OV
```

### Client aka aslclient01:
```
Username: user
Password: password
```

### CA Administrator Panel:
Client Certificate for CA administrator is located at
`/home/admin/caadmin_cert_with_priv_key_aslans01.pfx`, this is the `ms` user.
```
Passphrase: correct horse battery staple
```

### iMovies employees
Same as in legacy DB, except that `ms` got promoted to Admin,
see "CA Administrator Panel".


## Requirements
512MiB for each nonclient VM
1024MiB for the client VM

If you have more RAM available, feel free to add some RAM in the VirtualBox
settings for a faster environment.

## Help
* Loris Reiff <lreiff@student.ethz.ch>
* Miro Haller <mihaller@student.ethz.ch>
* Raphael Eikenberg <reikenberg@student.ethz.ch>
* Robertas Maleckas <rmaleckas@student.ethz.ch>
