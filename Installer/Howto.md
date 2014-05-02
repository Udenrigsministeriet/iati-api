# Howto

## Building the WIX-based installer

Building the WIX-based installer is done using msbuild, and not the VS build like the rest of this solution. 
To build the installer, do the following:

1. Open a Developer Command Prompt for VS2013
2. cd to the folder of the Installer project
3. Creat the installer using this build command: msbuild /t:WIX Setup.build