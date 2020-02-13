[![Build Status](https://travis-ci.com/Liblor/applied_sec_lab.svg?token=v2htoQjxNh7zAtUbzeQt&branch=master)](https://travis-ci.com/Liblor/applied_sec_lab)

# Applied Security Laboratory - AS19

## Certificate Authority
> [...] the students will also perform a team project: based on a set of functional and security requirements, they will design and implement a prototypical IT system. In addition, they will conduct a thorough security analysis and devise appropriate security measures for their systems. Finally, they will carry out a technical and conceptual review of another team's system. All project work will be performed in teams and must be properly documented. 

[ETHZ - Applied Security Lab](https://infsec.ethz.ch/education/as2019/seclab.html)

The task description can be found under [assignment](https://github.com/Liblor/applied_sec_lab#Assignment).
A description of our design can be found [here](https://github.com/Liblor/applied_sec_lab/tree/master/description/Applied_Security_Lab_group_1_System_Description_incl_backdoors.pdf).

## Group Members
* Loris Reiff - [Liblor](https://github.com/Liblor/applied_sec_lab)
* Miro Haller - [Miro-H](https://github.com/Miro-H)
* Raphael Eikenberg - [eikendev](https://github.com/eikendev)
* Robertas Maleckas - [RequestForCoffee](https://github.com/RequestForCoffee)

## Requirements
* VirtualBox
* vagrant

## Build
Create virtual machines and start them:
```
make build
```

Export virtual machines as .ova
```
make release
```

## Assignment
The (fictional) company iMovies produces independent movies of various kind
but with a focus on investigative reporting. Therefore, information exchanged
within the company and with informants must be handled confidentially.
To do so, iMovies wants to take its first steps towards PKI-based services.
For this reason, a simple certificate authority (CA) should be implemented, with
which employees can be provided with digital certificates. These certificates will
be used for secure e-mail communication.

### Functional Requirements
The entire functionality must be accessible to a remote client (outside the company’s network).

#### Certificate Issuing Process
The company already maintains a MySQL database in which all employees are
listed, along with their personal data as well as a user ID and a password. This
database is a legacy system, which cannot be migrated. The CA should verify
authorized certificate requests on the basis of this database.
The process of granting certificates should be as follows:

1. The user logs in via a web form by entering his user ID and his password.
The user ID and password are verified by consulting the information stored
in the database. Alternatively, login using a valid certificate is also possible;
2. The user is shown the user information stored in the database. If required,
the user may correct this information and any changes will be applied to
the database;
3. A certificate is issued based on the (possibly corrected) user information
from Step 2;
4. The user is offered the possibility to download the new certificate, including the corresponding private key, in PKCS#12 format.

#### Certificate Revocation Process
Employees need the possibility to revoke certificates, for example, when their
private key is compromised or lost.
The process of revoking a certificate should be as follows:
1. The affected user authenticates himself to a web application. Authentication can either be certificate-based client authentication over SSL/TLS
(if the user still holds the certificate and the corresponding private key)
or the user uses his user name and password stored in the database (if the
user has lost the certificate or the corresponding private key);
2. After successful authentication, the certificate in question (or all affected
certificates of the affected user) will be revoked. Additionally, a new certificate revocation list will be generated and published on the web server.
Make sure that login with revoked certificates is not possible.

#### CA Administrator Interface
Using a dedicated web interface, CA administrators (not necessarily system
administrators!) can consult the CA’s current state. The interface provides at
least the following information:
1. Number of issued certificates;
2. Number of revoked certificates;
3. Current serial number.
CA administrators authenticate themselves with their digital certificate.

#### Key Backup
A copy of all keys and certificates issued must be stored in an archive. The
archive is intended to ensure that encrypted data is still accessible even in the
case of loss of an employee’s certificate or private key, or even the employee
himself.

#### System Administration and Maintenance
The system should provide appropriate and secure interfaces for remote administration from the internet. In addition, an automated back-up solution must
be implemented, which includes configuration and logging information.
Note that these interfaces do not need to be especially comfortable or user
friendly. It is sufficient to provide suitable and simple interfaces with the help
of standard protocols such as SSH and FTP.

#### Components to Be Provided
Web Server: User interfaces, certificate requests, certificate delivery, revocation requests, etc;
Core CA: Management of user certificates, CA configuration, CA certificates
and keys, functionality to issue new certificates, etc;
MySQL Database: Legacy database with user data. The database specification can be found in Section 2.4;
Backup: Backup of keys and certificates from the Core CA and of configuration
and logging information.
Client: Sample client system that allows one to test the CA’s functionality from
outside the company’s network. The client system should be configured
such that all functions can be tested. This includes the configuration of a
special certificate to test the administrator interfaces.
Describe exactly how these components are distributed on various computers
and exactly what additional components are required to enforce the security
measures. The implementation is left up to the project team.
All systems must be built using VirtualBox, which defines the “hardware”.
The software is freely choosable. However, the operating systems must be Linux
variants, and the legacy database requires MySQL.

### Security Requirements
The most important security requirements are:
* Access control with regard to the CA functionality and data, in particular
configuration and keys;
* Secrecy and integrity with respect to the private keys in the key backup.
Note that the protection of the private keys on users’ computers is the
responsibility of the individual users;
* Secrecy and integrity with respect to user data;
* Access control on all components.


Derive the necessary security measures from a risk analysis. Use the methodology provided in the book, and justify your design choices using the security principles. Hint: avoid monolithic designs where many functionalities are
clumped together on a single machine (why?).


### Backdoors
You must build two backdoors into your system. Both backdoors should allow
remote access to the system(s) and compromise its purpose. The reviewers of
your system will later have to search for these backdoors.
Design and implement a first backdoor so that it will be nontrivial but likely
for the reviewers to find it. Give your best effort when it comes to the second
backdoor! Try to hide it so well that the reviewers will not find it. Do not forget
to hide your traces in the end (e.g., shell history).

