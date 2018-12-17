# card checker 
Demo code for the .Net User Groups 

## set your project
gcloud config set project <projectId>

## Setup as a developer 
- Download the Google Cloud SDK, instructions can be  found (https://cloud.google.com/sdk/docs/quickstart-windows)
- Create an environment variable:
     GOOGLE_APPLICATION_CREDENTIALS=<path to your code>\_deploy\gauth.json
	- Confirm it works by typing 
	  gcloud app instances list

## Deploying the API from Command line
- `dotnet restore`
- `dotnet publish -c Release`
- `gcloud config set <projectId>'
- `gcloud app deploy .\bin\Release\netcoreapp2.0\publish\app.yaml`

## Verify Deployment  
`gcloud app browse` or (https://<projectId>.appspot.com/index.html)
