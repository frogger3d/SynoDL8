using ReactiveUI;
using ReactiveUI.Xaml;
using SynoDL8.Model;
using SynoDL8.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SynoDL8.ViewModels
{
    public class DownloadTaskViewViewModel : ReactiveObject
    {
        private readonly ISynologyService SynologyService;
        private readonly ObservableAsPropertyHelper<string> visualState;
        private readonly ObservableAsPropertyHelper<bool> isActive;
        private readonly BehaviorSubject<bool> AreAllAvailable;

        private bool busy;
        private DownloadTask task;

        public DownloadTaskViewViewModel(ISynologyService synologyService, DownloadTask task)
        {
            this.SynologyService = synologyService.ThrowIfNull("synologyService");
            this.Task = task.ThrowIfNull("task");

            this.AreAllAvailable = new BehaviorSubject<bool>(true);

            this.PauseCommand = new ReactiveCommand(AreAllAvailable);
            this.PlayCommand = new ReactiveCommand(AreAllAvailable);
            this.DeleteCommand = new ReactiveCommand(AreAllAvailable);

            Observable.CombineLatest(PauseCommand.IsExecuting, PlayCommand.IsExecuting, (pa, pl) => !(pa || pl))
                      .Subscribe(n => AreAllAvailable.OnNext(n));

            this.PlayCommand.RegisterAsyncTask(_ => this.SynologyService.ResumeTaskAsync(this.Task.Id)).Subscribe();
            this.PauseCommand.RegisterAsyncTask(_ => this.SynologyService.PauseTaskAsync(this.Task.Id)).Subscribe();
            this.DeleteCommand.RegisterAsyncTask(_ => this.SynologyService.DeleteTaskAsync(this.Task.Id)).Subscribe();

            this.isActive = this.WhenAny(v => v.Task.Status, a => a)
                .Select(v =>
                {
                    switch (this.Task.Status)
                    {
                        case DownloadTask.status.downloading:
                        case DownloadTask.status.extracting:
                        case DownloadTask.status.filehosting_waiting:
                        case DownloadTask.status.finishing:
                        case DownloadTask.status.waiting:
                        case DownloadTask.status.paused:
                        case DownloadTask.status.hash_checking:
                            return true;

                        case DownloadTask.status.error:
                        case DownloadTask.status.finished:
                        case DownloadTask.status.seeding:
                        case DownloadTask.status.unknown:
                            return false;

                        default:
                            throw new InvalidOperationException("Unknown status");
                    }
                })
                .ToProperty(this, v => v.IsActive);

            this.visualState = this.WhenAny(v => v.Task.Status, a => a)
                                     .Select(v =>
                                     {
                                         switch (this.Task.Status)
                                         {
                                             case DownloadTask.status.paused:
                                                 return "Paused";
                                             case DownloadTask.status.downloading:
                                                 return "Downloading";
                                             case DownloadTask.status.waiting: // cannot start because it is queued
                                             case DownloadTask.status.finished:
                                                 return "Finished";
                                             default:
                                                 return "Normal";
                                         }
                                     })
                                     .ToProperty(this, v => v.VisualState);

            this.title = this.WhenAny(v => v.Task.Title, a => a.Value)
                             .ToProperty(this, v => v.Title);

            this.status = this.WhenAny(v => v.Task.Status, a => a.Value.ToString())
                              .ToProperty(this, v => v.Status);
        }

        ObservableAsPropertyHelper<string> title, status;
        public string Title { get { return this.title.Value; } }
        public string Status { get { return this.status.Value; } }

        public DownloadTask Task
        {
            get { return this.task; }
            set { this.RaiseAndSetIfChanged(ref this.task, value); }
        }

        public string VisualState
        {
            get { return this.visualState.Value; }
        }

        public bool Busy
        {
            get { return this.busy; }
            private set { this.RaiseAndSetIfChanged(ref this.busy, value); }
        }

        public double Progress
        {
            get { return (double)this.Task.SizeDownloaded / this.Task.Size; }
        }

        public bool IsActive
        {
            get { return this.isActive.Value; }
        }

        public ReactiveCommand PlayCommand { get; set; }
        public ReactiveCommand PauseCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        private void Pause()
        {
            this.Busy = true;

            this.SynologyService.PauseTaskAsync(this.Task.Id)
                          .ToObservable()
                          .ObserveOnDispatcher()
                          .Subscribe(
                              next =>
                              {
                                  this.Busy = false;
                              },
                              error => 
                              {
                                  this.Busy = false;
                                  new MessageDialog("Could not pause due to error");
                              });
        }

        private void Resume()
        {
            this.Busy = true;

            this.SynologyService.ResumeTaskAsync(this.Task.Id)
                          .ToObservable()
                          .ObserveOnDispatcher()
                          .Subscribe(
                              next =>
                              {
                                  this.Busy = false;
                              },
                              error =>
                              {
                                  this.Busy = false;
                                  new MessageDialog("Could not resume due to error");
                              });
        }
    }
}
