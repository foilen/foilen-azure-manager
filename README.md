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

