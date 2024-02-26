# azure-devops-export-for-actionable-agile



## Usage

`AzureDevOpsExportAA.exe --output "c:\Temp\export.csv" --token [PAT] --org nkdagility-learn --project ApplicationDemo --team "Application Overview"  --board Applications`


- `--output` [requried] - sets the location where we will save the CSV. 
- `--token` [requried] - sets the PAT token to authenticate with.
- `--org` - the name of the organsaition/account. If not provided you will be offered a list to select from
- `--project` - the name of the project. If not provided you will be offered a list to select from
- `--team` - the name of the team. If not provided you will be offered a list to select from
- `--board` - the name of the board. If not provided you will be offered a list to select from


Recommended: Set a [PAT token](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows) as the primary method of authentication. 
