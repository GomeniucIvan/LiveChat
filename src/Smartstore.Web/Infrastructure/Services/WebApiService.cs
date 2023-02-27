using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Smartstore.Caching;
using Smartstore.Core.Configuration;
using Smartstore.Core.Data;
using Smartstore.Core.Identity;
using Smartstore.Core.Stores;
using Smartstore.Engine;
using Smartstore.Engine.Modularity;
using Smartstore.Threading;
using Smartstore.Web.Api.Models;

namespace Smartstore.Web.Api
{
    public partial class WebApiService : IWebApiService
    {
        // {0} = StoreId
        internal const string StateKey = "smartstore.webapi:state";
        internal const string StatePatternKey = "smartstore.webapi:state-*";

        internal const string UsersKey = "smartstore.webapi:users";
        internal const string AttributeUserDataKey = "WebApiUserData";

        private readonly SmartDbContext _db;
        private readonly ICacheManager _cache;
        private readonly IMemoryCache _memCache;
        private readonly ISettingFactory _settingFactory;
        private readonly CancellationToken _appShutdownCancellationToken;

        public WebApiService(
            SmartDbContext db,
            ICacheManager cache,
            IMemoryCache memCache,
            ISettingFactory settingFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _db = db;
            _cache = cache;
            _memCache = memCache;
            _settingFactory = settingFactory;
            _appShutdownCancellationToken = hostApplicationLifetime.ApplicationStopping;
        }

        /// <summary>
        /// Creates a pair of of cryptographic random numbers as a hex string.
        /// </summary>
        /// <param name="key1">First created key.</param>
        /// <param name="key2">Second created key.</param>
        /// <param name="length">The length of the keys to be generated.</param>
        /// <returns><c>true</c> succeeded otherwise <c>false</c>.</returns>
        public static bool CreateKeys(out string key1, out string key2, int length = 32)
        {
            key1 = key2 = null;

            using (var rng = RandomNumberGenerator.Create())
            {
                for (var i = 0; i < 9999; i++)
                {
                    var data1 = new byte[length];
                    var data2 = new byte[length];

                    rng.GetNonZeroBytes(data1);
                    rng.GetNonZeroBytes(data2);

                    key1 = data1.ToHexString(false, length);
                    key2 = data2.ToHexString(false, length);

                    if (key1 != key2)
                    {
                        break;
                    }
                }
            }

            return key1.HasValue() && key2.HasValue() && key1 != key2;
        }

        public WebApiState GetState()
        {

            return _cache.Get(StateKey.FormatInvariant(), (o) =>
            {
                o.ExpiresIn(TimeSpan.FromDays(30));

                var settings = _settingFactory.LoadSettings<WebApiSettings>();

                var state = new WebApiState
                {
                    IsActive = settings.IsActive,
                    MaxTop = settings.MaxTop,
                    MaxExpansionDepth = settings.MaxExpansionDepth
                };

                return state;
            });
        }
    }
}
