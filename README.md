# azure-devops-export-for-actionable-agile

  The Azure DevOps Export for Actionable Agile allows you to export data from Azure DevOps for import into Actionable
  Agile's standalone analytics tool on https://https://analytics.actionableagile.com/. Use when you are unable to use
  the extension or already have a standalone licence.

## Usage:

  AzureDevOpsExportAA [options]

## Options:

  --org <org>                       the name of the organisation / account to connect to. If not provided you will be
                                    asked to select.
  --token <token>                   The auth token to use.
  --team <team>                     The name of the team to connect to. If not provided you will be asked to select.
  --project <project>               The name of the project to connect to. If not provided you will be asked to select.
  --backlog, --board <board>        The name of the board/backlog to connect to. If not provided you will be asked to
                                    select.
  --format <Backlog|Board|Project>  Do you want to use board columns or backlog work item states, or the whole project
                                    [default: Board]
  --output <output> (REQUIRED)      Where to output the file to.
  --version                         Show version information
  -?, -h, --help                    Show help and usage information

  ## Example

`AzureDevOpsExportAA.exe --output "c:\Temp\export.csv" --token [PAT] --org nkdagility-learn --project ApplicationDemo --team "Application Overview"  --board Applications`