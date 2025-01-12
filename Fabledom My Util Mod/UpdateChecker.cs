using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

public class UpdateChecker
{
    private const string VersionUrl = "https://raw.githubusercontent.com/Gaaraszauber/Fabledom-My-Util-Mod/refs/heads/master/Fabledom%20My%20Util%20Mod/version.txt";

    public async Task<string> GetLatestVersion()
    {
        using (var client = new HttpClient())
        {
            try
            {
                return await client.GetStringAsync(VersionUrl);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Error retrieving latest version: {e.Message}");
                return null;
            }
        }
    }
}
