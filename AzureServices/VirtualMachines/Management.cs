using System.Net;
using System.Threading.Tasks;
using Data.Models;

namespace AzureServices.VirtualMachines
{
  public static class Management
  {
    public const string PostRequest =
      "https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roleinstances/{3}/Operations";

    public const string GetRequest =
      "https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roles/{3}";

    /// <summary>
    ///   Extract the current configuration of the VM
    ///   Docs: https://msdn.microsoft.com/en-us/library/azure/jj157206.aspx
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> GetRole(ViewModel vm)
    {
      var uri = string.Format(GetRequest, vm.SubscriptionId, vm.CloudServiceName, vm.DeploymentName, vm.RoleName);

      var request = (HttpWebRequest) WebRequest.Create(uri);
      request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + vm.Token);
      var response = await request.GetResponseAsync();

      return true;
    }

    public static async Task<bool> StartRole(ViewModel vm)
    {
      var uri = string.Format(PostRequest, vm.SubscriptionId, vm.CloudServiceName, vm.DeploymentName, vm.RoleName);

      var request = (HttpWebRequest) WebRequest.Create(uri);
      request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + vm.Token);
      var response = await request.GetResponseAsync();

      return true;
    }

    public static async Task<bool> ShutdownRole(ViewModel vm)
    {
      var uri = string.Format(PostRequest, vm.SubscriptionId, vm.CloudServiceName, vm.DeploymentName, vm.RoleName);

      var request = (HttpWebRequest) WebRequest.Create(uri);
      request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + vm.Token);
      var response = await request.GetResponseAsync();
      return true;
    }
  }
}