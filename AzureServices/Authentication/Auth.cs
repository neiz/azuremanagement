using System;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureServices.Authentication
{
  public static class Auth
  {
    public static string GetAuthorizationHeader()
    {
      AuthenticationResult result = null;

      var context = new AuthenticationContext("");

      var thread = new Thread(() =>
      {
        result = context.AcquireToken(
          "https://management.core.windows.net/",
          "",
          new Uri("http://localhost"));
      });

      thread.SetApartmentState(ApartmentState.STA);
      thread.Name = "AquireTokenThread";
      thread.Start();
      thread.Join();

      if (result == null)
      {
        throw new InvalidOperationException("Failed to obtain the JWT token");
      }

      var token = result.AccessToken;
      return token;
    }
  }
}