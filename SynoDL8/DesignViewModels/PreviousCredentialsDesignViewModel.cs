using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.DesignViewModels
{
    public class PreviousCredentialsDesignViewModel
    {
        private readonly Credentials model;

        public PreviousCredentialsDesignViewModel()
        {
            this.model = new Credentials() { Hostname="host", User = "user"};
        }

        public Credentials Model { get { return this.model; } }

        public ReactiveCommand<object> Select { get; private set; }

        public ReactiveCommand<object> Remove { get; private set; }
    }
}
