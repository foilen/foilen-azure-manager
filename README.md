# About

This application helps creating Web Application Services on Microsoft Azure. It also adds some features that are not in the default containers.

# Global features

- Lists all the Web Application Services you have access to.
- Automatically updates the DNS for DNS Zones available in your Azure account.

# Supported languages

## PHP

- Uses the Docker image https://github.com/foilen/az-docker-apache_php which contains:
  - a lot of PHP extensions installed
  - a sendmail replacement that supports a lot of different ways of sending emails with PHP
- Configures the default email address and the email account for relaying the emails (you can relay using Sendmail, AWS or any other email account)
- Configure some PHP parameters

# Prerequisite

You need to have "az" installed.
You can download it on https://aka.ms/installazurecliwindows

# Install on your machine

- Go on https://deploy.foilen.com/foilen-azure-manager/Publish.html
- Click "Install"
- For now, the application is not signed (it is costly) and it will tell you "Windows protected your PC"
  - Click "More info"
  - Click "Run anyway"
- It will tell you the "Publisher cannot be verified. Are you sure you want to install this application?"
  - Click "Install"

# Create release

The revision version number automatically increments, but if you want to change the major or minor version:
- Open Visual Studio
- Right-click the "Azure Manager" project and choose "Publish"
- Click "More actions" and "Edit"
- Progress to "Settings" and set the version you want

Then, follow these steps:
- Open Visual Studio
- Right-click the "Azure Manager" project and choose "Publish"
- Click "Publish"

You will get the files in `/wpf app/bin/publish/`.

Finally, to create a git tag:
```
VERSION=$(grep version "wpf app/bin/publish/Azure Manager.application" | tail -n 1 | cut -d'"' -f 4)
echo $VERSION

git tag -a $VERSION -m $VERSION
git push && git push --tags
```

# Deploy to IPFS

## Check if local is up to date

```
ipfs resolve /ipns/deploy.foilen.com | cut -d'/' -f 3 ; ipfs files stat /com.foilen.deploy | head -n 1
```

## Update local

```
ipfs files rm -r /com.foilen.deploy
ipfs files cp $(ipfs resolve /ipns/deploy.foilen.com) /com.foilen.deploy

ipfs files ls /
ipfs files ls /com.foilen.deploy/
ipfs files ls /com.foilen.deploy/foilen-azure-manager
```

## Add the file

```
IPFS_ID=$(ipfs add -rq 'wpf app/bin/publish/' | tail -n 1)
ipfs files rm -r /com.foilen.deploy/foilen-azure-manager
ipfs files cp /ipfs/$IPFS_ID /com.foilen.deploy/foilen-azure-manager
```

## Update the dnslink

```
ipfs files stat /com.foilen.deploy | head -n 1
```

https://infra.foilen.com/pluginresources/edit/5fee2ba579436931f9dba248
