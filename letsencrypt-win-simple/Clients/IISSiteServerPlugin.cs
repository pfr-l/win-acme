﻿using ACMESharp;
using LetsEncrypt.ACME.Simple.Plugins.TargetPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LetsEncrypt.ACME.Simple.Clients
{
    public class IISSiteServerPlugin : IISClient
    {
        public new const string PluginName = "IISSiteServer";

        public override string Name => PluginName;

        public override void Install(Target target, string pfxFilename, X509Store store, X509Certificate2 certificate)
        {
            foreach (var subTarget in SplitTarget(target))
            {
                subTarget.Plugin.Install(subTarget, pfxFilename, store, certificate);
            }
        }

        public override void Install(Target target)
        {
            foreach (var subTarget in SplitTarget(target))
            {
                subTarget.Plugin.Install(subTarget);
            }
        }

        public override void Auto(Target target)
        {
            foreach (var subTarget in SplitTarget(target))
            {
                var auth = Program.Authorize(subTarget);
                if (auth.Status != "valid")
                {
                    Program.OnAutoFail(auth);
                }
            }
            Program.OnAutoSuccess(target);
        }

        private List<Target> SplitTarget(Target totalTarget)
        {
            var plugin = (IISSite)Program.Plugins.GetByName(Program.Plugins.Target, nameof(IISSite));
            List<Target> targets = plugin.GetSites(Program.Options, false);
            string[] siteIDs = totalTarget.Host.Split(',');
            var filtered = targets.Where(t => siteIDs.Contains(t.SiteId.ToString())).ToList();
            filtered.ForEach(x => {
                x.ExcludeBindings = totalTarget.ExcludeBindings;
                x.ValidationPluginName = totalTarget.ValidationPluginName;
            });

            return filtered;
        }
    }
}