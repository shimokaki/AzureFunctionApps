# AzureFunctionApps 

This function finds ADD groups subs with using ADD groups display name in MS Azure Function apps.

## packages 
NuGet
1. Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory
1. Install-Package System.Configuration.ConfigurationManager


## settings
go to `Function app settings > Settings > Application settings > App Settings` in azure portal.
Add following parameters
```
    "B2C_TENANT_DEV": "YOUR_AAD_DOMAIN.onmicrosoft.com",
    "B2C_CLIENTID_DEV": "YOUR_AAD_REGISTED_APP_CLIENT_ID",
    "B2C_CLIENTSECRET_DEV": "YOUR_AAD_REGISTED_APP_CLIEN_SECRET_KEY",
    "B2C_GRAPHURL_DEV": "https://graph.windows.net/YOUR_AAD_DOMAIN.onmicrosoft.com/",
    "API_VERSION": "api-version=1.6"
```

you can refer following link for Azure AD application setting. 
https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications

**[important]**
in this code,  
 "https://graph.windows.net/YOUR_AAD_DOMAIN.onmicrosoft.com/", is specialy for AzureAD B2C.
When you want to use regular ADD, please modify this address to "https://graph.microsoft.com/v1.0/me/".
and modify modified registered application at ADD too.


## input 
input json body format
```
{
  "Groups": {
	"key0": "Group0(displayName)",
	"key1": "Group1",
	"key2": "Group2"
  },
	"token": ""
}
```
When you do not have token, this function get token and out put it.

## output
output json body format
```
  "Content": {
    "token": "",
	"GroupsSub": [
		"GROUP0's GUID",
		"GROUP1's GUID",
		"GROUP2's GUID"
    ]
  }
}
```