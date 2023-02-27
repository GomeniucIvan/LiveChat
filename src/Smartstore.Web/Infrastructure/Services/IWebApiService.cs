using System.Collections.Generic;
using System.Threading.Tasks;
using Smartstore.Core.Stores;
using Smartstore.Web.Api.Models;

namespace Smartstore.Web.Api
{
    public partial interface IWebApiService
    {
        /// Gets the current state of the API including configuration settings.
        /// <returns>Web API state.</returns>
        WebApiState GetState();
    }
}
