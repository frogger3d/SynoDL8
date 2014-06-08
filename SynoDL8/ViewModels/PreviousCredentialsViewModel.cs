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

            this.Select = new ReactiveCommand();
            this.Remove = new ReactiveCommand();
        }

        public Credentials Model { get { return this.model; } }

        public ReactiveCommand Select { get; private set; }

        public ReactiveCommand Remove { get; private set; }
    }
}
