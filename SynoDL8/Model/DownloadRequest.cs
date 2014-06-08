using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.Model
{
    public class DownloadRequest
    {
        public DownloadRequest(Uri uri)
        {
            this.Uri = uri;
        }

        public Uri Uri { get; private set; }
    }
}
