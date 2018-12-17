# card checker 
Demo code for the .Net User Groups 

## create aproject
- Brwose to [Google Console](https://console.cloud.google.com)
- Create Project "name" / Select Billing Account
- Note the Project ID <projectid>.
- Create a service Account called "Application Deploymnet"
- Grant "App Engine Admin" and "Storage Admin"
- Download the "gauth.json" service account certificate as JSON

## Setup as a developer 
- Download the Google Cloud SDK, instructions can be  found (https://cloud.google.com/sdk/docs/quickstart-windows)
- Create an environment variable:
     GOOGLE_APPLICATION_CREDENTIALS=<path to your code>\_deploy\gauth.json
	- Confirm it works by typing 
	  gcloud app instances list

## set your project
gcloud config set project <projectId>

## Deploying the API from Command line
- `dotnet restore`
- `dotnet publish -c Release`
- `gcloud config set project <projectId>'
- `gcloud app deploy .\bin\Release\netcoreapp2.0\publish\app.yaml`

## Verify Deployment  
`gcloud app browse` or (https://<projectId>.appspot.com/index.html)
