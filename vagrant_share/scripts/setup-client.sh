#!/usr/bin/env bash

UNAME="$1"

if [ -z "$UNAME" ] ; then
	echo "No username given." >&2
	exit 1
fi

# Upgrade all packages, install user interface, Firefox, and other needed tools.
sudo apt-get update
DEBIAN_FRONTEND=noninteractive sudo -E apt-get upgrade -y
DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y x-window-system lightdm xfce4 firefox-esr libnss3-tools --no-install-recommends

# Use default XFCE panel without user prompt
sudo cp /etc/xdg/xfce4/panel/default.xml /etc/xdg/xfce4/xfconf/xfce-perchannel-xml/xfce4-panel.xml

# Activate autologin
sudo mkdir /etc/lightdm/lightdm.conf.d
sudo bash -c "echo -e '[SeatDefaults]\nautologin-user=$UNAME' > /etc/lightdm/lightdm.conf.d/12-autologin.conf"

# Hack to initialize Firefox so that its certificate DB is initialized
sudo su - $UNAME -c "firefox --screenshot /dev/null https://google.com 2> /dev/null"

# Add our root CA to firefox root of trust
sudo /vagrant/scripts/mozilla-import-certificates.sh /home/$UNAME

# Add launcher to open login page
sudo mkdir /home/$UNAME/Desktop
sudo chown $UNAME: /home/$UNAME/Desktop
sudo chmod 700 /home/$UNAME/Desktop
sudo cp /vagrant/Login.desktop /home/$UNAME/Desktop/Login.desktop
sudo chown $UNAME: /home/$UNAME/Desktop/Login.desktop

# Switch to GUI.
sudo systemctl set-default graphical.target
systemctl isolate graphical.target
