KeePassHIBP
=================================

OVERVIEW
-----
KeePassHIBP (Have I Been Pwned) is a plug-in for KeePass 2.x which tests your passwords against the [Have I Been Pwned](https://haveibeenpwned.com/) database.

The plugin hooks into two windows in KeePass (the 'Create Composite Master Key' form and the 'Edit Entry' form). When you type in the password field your input gets checked if it has previously appeared in a data breach. You can get [here](https://www.troyhunt.com/ive-just-launched-pwned-passwords-version-2/) more informations about how the API of HIBP works.

INSTALLATION
-----
- Download from https://github.com/JanisEst/KeePassHIBP/releases
- Copy the plug-in (KeePassHIBP.plgx) into the KeePass plugin directory
- Start KeePass

HOW TO USE
-----
Type into the password field in one of the mentioned forms. If the password is weak you will see a little tooltip:

![alt tag](https://abload.de/img/hibpksq5z.jpg)
