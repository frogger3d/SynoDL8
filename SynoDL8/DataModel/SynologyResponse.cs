using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.DataModel
{
    public static class SynologyResponseMixins
    {
        public static bool IsSuccess(string result, Func<int, string> errorLookup = null)
        {
            var response = ToResponse(result, errorLookup);
            return response.Success;
        }

        public static SynologyResponse ToResponse(string result, Func<int, string> errorLookup = null)
        {
            return SynologyResponse.FromJason(result, errorLookup);
        }

        public static async Task<bool> IsSuccess(this Task<string> task, Func<int, string> errorLookup = null)
        {
            var response = await task.ToResponse(errorLookup);
            return response.Success;
        }

        public static Task<SynologyResponse> ToResponse(this Task<string> task, Func<int, string> errorLookup = null)
        {
            return task.ContinueWith(t => SynologyResponse.FromJason(t.Result, errorLookup));
        }
    }

    public class SynologyResponse
    {
        public SynologyResponse()
        {
        }

        public bool Success { get; private set; }
        public int ErrorCode { get; private set; }
        public string Error { get; private set; }

        /// <summary>
        /// Jason result
        /// </summary>
        public string Content { get; private set; }

        public static SynologyResponse FromJason(string result, Func<int, string> errorLookup = null)
        {
            var o = JObject.Parse(result);
            if ((bool)o["success"])
            {
                return new SynologyResponse() { Success = true, Content = result };
            }
            else
            {
                int errorCode = (int)o["error"]["code"];
                string error = GetCommonError(errorCode);
                if (error == null && errorLookup != null)
                {
                    error = errorLookup(errorCode);
                }

                return new SynologyResponse()
                {
                    Success = false,
                    Content = result,
                    ErrorCode = errorCode,
                    Error = error ?? "Unknown error"
                };
            }
        }

        private static string GetCommonError(int code)
        {
            switch (code)
            {
                case 100: return "Unknown error";
                case 101: return "Invalid parameter";
                case 102: return "The requested API does not exist";
                case 103: return "The requested method does not exist";
                case 104: return "The requested version does not support the functionality";
                case 105: return "The logged in session does not have permission";
                case 106: return "Session timeout";
                case 107: return "Session interrupted by duplicate login";
                default: return null;
            }
        }

        public static string GetAuthError(int code)
        {
            switch (code)
            {
                case 400: return "No such account or incorrect password";
                case 401: return "Account disabled";
                case 402: return "Permission denied";
                case 403: return "2-step verification code required";
                case 404: return "Failed to authenticate 2-step verification code";
                default: return null;
            }
        }
    }
}
