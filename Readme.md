# Prerequisites
- Install .Net v4.6.2 offline installer

# Installation
- Install "IATI Data Services {version}"  to {installFolder}, e.g. "IATI Data Services v2" to "C:\program files (x86)\globeteam\IATI Data Services v2"

# Configuration
- Create C:\inetpub\iati_{version}\files path, e.g. "C:\inetpub\iati_v2\files"
- Create C:\inetpub\iati\api (root node for {version} and future api versions)
- Create application under C:\inetpub\iati\api
	- Name should be {version}, e.g. "v2"
	- Path should be {installFolder}, e.g. "C:\program files (x86)\globeteam\IATI Data Services v2"
- Make sure IUsr has read-permissions to the folder where the programme is installed