﻿using cloudscribe.Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace cloudscribe.Core.Identity
{
    public class SiteAuthCookieEvents : CookieAuthenticationEvents, ISiteAuthCookieEvents
    {
        public SiteAuthCookieEvents(
             IOptions<MultiTenantOptions> multiTenantOptions,
             ILogger<SiteAuthCookieEvents> logger
            )
        {
            _multiTenantOptions = multiTenantOptions.Value;
            _log = logger;
        }

        private readonly MultiTenantOptions _multiTenantOptions;
        private readonly ILogger _log;

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var tenant = context.HttpContext.GetTenant<SiteContext>();

            if (tenant == null)
            {
                context.RejectPrincipal();
            }

            var siteGuidClaim = new Claim("SiteGuid", tenant.Id.ToString());

            if (_multiTenantOptions.UseRelatedSitesMode || context.Principal.HasClaim(siteGuidClaim.Type, siteGuidClaim.Value))
            {
                
                await SecurityStampValidator.ValidatePrincipalAsync(context);
                return;
            }
            else
            {
                _log.LogInformation("rejecting principal because it does not have siteguid");
                context.RejectPrincipal();
            }
            

        }


        public Type GetCookieAuthenticationEventsType()
        {
            return typeof(SiteAuthCookieEvents);
        }
    }
}
