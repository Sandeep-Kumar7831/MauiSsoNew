#if ANDROID
using Android.Content;
using System.Text;

namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// Debug utility to check token storage state
    /// </summary>
    public static class TokenDebugUtility
    {
        private const string PREFS_NAME = "MauiSsoTokens";

        public static string GetStorageDebugInfo(Context context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Token Storage Debug Info ===");

            try
            {
                var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
                var allEntries = prefs.All;

                sb.AppendLine($"SharedPreferences file: {PREFS_NAME}");
                sb.AppendLine($"Total entries: {allEntries.Count}");
                sb.AppendLine();

                foreach (var entry in allEntries)
                {
                    var value = entry.Value?.ToString() ?? "null";
                    if (value.Length > 50)
                    {
                        value = value.Substring(0, 50) + "... (truncated)";
                    }
                    sb.AppendLine($"Key: {entry.Key}");
                    sb.AppendLine($"Value: {value}");
                    sb.AppendLine($"Length: {entry.Value?.ToString()?.Length ?? 0}");
                    sb.AppendLine();
                }

                // Test direct access
                sb.AppendLine("=== Direct Access Test ===");
                var accessToken = prefs.GetString("sso_access_token", null);
                var refreshToken = prefs.GetString("sso_refresh_token", null);
                var idToken = prefs.GetString("sso_id_token", null);

                sb.AppendLine($"Access Token: {(accessToken != null ? $"Present ({accessToken.Length} chars)" : "Missing")}");
                sb.AppendLine($"Refresh Token: {(refreshToken != null ? $"Present ({refreshToken.Length} chars)" : "Missing")}");
                sb.AppendLine($"ID Token: {(idToken != null ? $"Present ({idToken.Length} chars)" : "Missing")}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        public static void DumpToLog(Context context)
        {
            var info = GetStorageDebugInfo(context);
            System.Diagnostics.Debug.WriteLine(info);
        }
    }
}
#endif