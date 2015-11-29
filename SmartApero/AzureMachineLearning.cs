using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class AzureMachineLearning
    {
        public static async Task<string> InvokeRequestResponseService(StringTable p)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {"input1",p},
                        {"input6",p},
                        {"input3",p},
                        {"input4",p},
                        {"input5",p},
                        {"input2",p},
                        {"input7",p},
                        {"input8",p},
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "hAKcxgPQ4MFs3p/FrjQ8EHJHowGgiyuVY3fSub7hOLKbA7W1mM3QkDjVqZBzOulRyHD0IQlXzU5wwJYpEX8Crw=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/c5e785c7a67741039a35c221563ad7ef/services/2f7410441b1748f09fe976688203b080/execute?api-version=2.0&details=true");

                var content = JsonConvert.SerializeObject(scoreRequest);

                HttpResponseMessage response = await client.PostAsync("", new StringContent(content, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    return result;
                    //dynamic res = JsonConvert.DeserializeObject(result);
                    //Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    //Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    //Console.WriteLine(response.Headers.ToString());

                    //string responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseContent);
                }
            }

            return String.Empty;
        }
    }
}
