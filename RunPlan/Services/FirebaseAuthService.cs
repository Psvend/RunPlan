using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic;

using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RunPlan.Services;

public class FirebaseAuthService
{
    private readonly HttpClient _httpClient = new();
    private const string ApiKey = "DIN_FIREBASE_WEB_API_KEY";
    private const string BaseUrl = "https://identitytoolkit.googleapis.com/v1/accounts:";

    public async Task<string?> LoginAsync(string email, string password)
    {
        var payload = new { email, password, returnSecureToken = true };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}signInWithPassword?key={ApiKey}", content);

        return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
    }

    public async Task<string?> RegisterAsync(string email, string password)
    {
        var payload = new { email, password, returnSecureToken = true };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}signUp?key={ApiKey}", content);

        return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
    }
}
