using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.ViewModels
{
    public class PreviousCredentialsViewModel : ReactiveObject
    {
        private readonly Credentials model;

        public PreviousCredentialsViewModel(Credentials model)
        {
            this.model = model;

            this.Select = ReactiveCommand.Create();
            this.Remove = ReactiveCommand.Create();
        }

        public Credentials Model { get { return this.model; } }

        public ReactiveCommand<object> Select { get; private set; }

        public ReactiveCommand<object> Remove { get; private set; }
    }
}
