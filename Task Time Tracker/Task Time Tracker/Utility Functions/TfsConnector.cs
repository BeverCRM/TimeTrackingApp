using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Model.TFS;

namespace Task_Time_Tracker.Utility_Functions
{
    public class TfsConnector
    {
        private readonly string _personalAccessToken;
        private readonly string _tfsUrl;

        public TfsConnector(string personalAccessToken, string tfsUrl)
        {
            _personalAccessToken = personalAccessToken;
            _tfsUrl = tfsUrl;
        }

        public async Task<TaskList> GetTasksFromTfs(string taskIds)
        {
            return await SendRequest(taskIds);
        }

        private async Task<TaskList> SendRequest(string taskIds)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));

                using (HttpResponseMessage response = await client.GetAsync(
                    _tfsUrl + "DefaultCollection/_apis/wit/workitems/?ids=" + taskIds +
                    "&fields=Microsoft.VSTS.Common.Priority,Microsoft.VSTS.Scheduling.DueDate," +
                    "Microsoft.VSTS.Scheduling.OriginalEstimate,System.AssignedTo,System.Description," +
                    "System.Title,System.Id&api-version=1.0"))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    return Deserialiser.DeserialiseObject<TaskList>(responseBody);
                }
            }
        }
    }
}

