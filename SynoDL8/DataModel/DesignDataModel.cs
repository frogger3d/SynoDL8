﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.DataModel
{
    internal class DesignDataModel : IDataModel
    {
        public Task<bool> Login()
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> Logout()
        {
            return new Task<bool>(() => true);
        }

        public Task<string> GetVersions()
        {
            return new Task<string>(() => "Lorem ipsum");
        }

        public Task<string> GetInfo()
        {
            return new Task<string>(() => "Lorem ipsum");
        }

        public Task<string> Create(string url)
        {
            return new Task<string>(() => "Lorem ipsum");
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return new Task<IEnumerable<DownloadTask>>(() => CreateFakeList());
        }

        private IEnumerable<DownloadTask> CreateFakeList()
        {
            yield return new DownloadTask() { Title = "Lorem ipsum" };
            yield return new DownloadTask() { Title = "Lorem ipsum" };
            yield return new DownloadTask() { Title = "Lorem ipsum" };
        }
    }
}
