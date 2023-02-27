using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Smartstore.Web.Api.Security
{
    internal class ODataExceptionHandlerPathFeature : IExceptionHandlerPathFeature
    {
        public ODataExceptionHandlerPathFeature(Exception error, HttpRequest request)
        {
            Error = error;
            Path = request?.Path;
        }

        public Exception Error { get; }
        public string Path { get; }
    }
}
