﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DMT.Common;
using DMT.Core.Interfaces;
using DMT.Matcher.Interfaces;
using DMT.Matcher.Module.Exceptions;
using DMT.Module.Common.Service;

namespace DMT.Matcher.Module
{
    internal class Job : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IMatcherJob job;
        private bool jobStarted;

        public Job(IMatcherJob job)
        {
            this.job = job;
            job.Done += HandleJobDone;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDependencies;
        }

        public void Start(IModel model, MatchMode mode)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (this.jobStarted)
            {
                throw new JobAlreadyStartedException(string.Format("Job with name {0} has already started.", this.job.Name));
            }
            this.jobStarted = true;

            // start the task on a background thread
            Task.Run(() => this.job.Start(model, mode));

            logger.Info("Matcher job (name: {0}) has been started in {1} mode", this.job.Name, mode);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveDependencies;
        }

        private void HandleJobDone(object sender, MatcherJobDoneEventArgs e)
        {
            this.jobStarted = false;

            logger.Info("Matcher job (name: {0}) has been finished.", this.job.Name);
            var client = MatcherModule.Instance.CreatePartitionServiceClient();
            client.MarkMatcherDone(MatcherModule.Instance.Id, new MatchFoundRequest { MatchFound = e.HasMatches });
            logger.Debug("Matcher job reported done to partition module.");
        }

        private Assembly ResolveDependencies(object sender, ResolveEventArgs args)
        {
            logger.Debug("Resolving dependency for {0}", args.Name);
            foreach (var dep in this.job.Dependencies)
            {
                if (args.Name.Contains(dep))
                {
                    return LoadDepencdency(dep);
                }
            }

            logger.Warn("Could not resolve dependency for {0}", args.Name);

            return null;
        }

        private Assembly LoadDepencdency(string depName)
        {
            string[] paths = new[]
            {
                string.Concat(Configuration.Current.DefaultsFolder, "/", depName, ".dll"),
                string.Concat(Configuration.Current.PluginsFolder, "/", depName, ".dll"),
            };

            foreach (var path in paths)
            {
                var fi = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                if (fi.Exists)
                {
                    byte[] assembly;
                    using (FileStream stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        assembly = new byte[stream.Length];
                        stream.Read(assembly, 0, (int)stream.Length);
                    }
                    return Assembly.Load(assembly);
                }
            }

            logger.Warn("No dll file exists in the additional probing paths.");
            return null;
        }
    }
}
