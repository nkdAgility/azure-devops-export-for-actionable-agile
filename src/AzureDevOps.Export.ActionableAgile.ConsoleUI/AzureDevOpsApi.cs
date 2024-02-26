using AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI
{
    internal class AzureDevOpsApi
    {
        private readonly string _authHeader;
        private OrgItem _orgItem;

        public AzureDevOpsApi(string authHeader)
        {
            _authHeader = authHeader;
        }

        public void SetOrganisation(OrgItem orgItem)
        {
            _orgItem = orgItem;
        }

        public async Task<string> GetBoard(string projectItemId, string teamItemId, string boardName)
        {
            
            string apiGetSingle = $"{_orgItem.accountUri}/{projectItemId}/{teamItemId}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
            return await GetResult(apiGetSingle);
        }

        public async Task<string> GetOrgs(ProfileItem profileItem)
        {
            string apiGetSingle = $"https://app.vssps.visualstudio.com/_apis/accounts?memberId={profileItem.id}&api-version=7.1-preview.1";
            return await GetResult(apiGetSingle);
        }

        public async Task<string> GetBoards(string projectItemId, string teamItemId)
        {
            string apiCallUrl = $"{_orgItem.accountUri}/{projectItemId}/{teamItemId}/_apis/work/boards?api-version=7.2-preview.1";
            return await GetResult(apiCallUrl);
        }

        private async Task<string> GetResult(string apiToCall)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Add("Authorization", _authHeader);

                HttpResponseMessage response = await client.GetAsync(apiToCall);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    Console.WriteLine("Result::{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }
            }
            return string.Empty;
        }

    }
}
