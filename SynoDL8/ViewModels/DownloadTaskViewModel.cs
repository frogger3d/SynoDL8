﻿using ReactiveUI;
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
    public class DownloadTaskViewModel : ReactiveObject
    {
        private ISynologyService SynologyService;
        private DownloadTask Task;

        private bool busy;
        private BehaviorSubject<bool> areAllAvailable;

        public DownloadTaskViewModel(ISynologyService synologyService, DownloadTask task)
        {
            this.SynologyService = synologyService.ThrowIfNull("synologyService");
            this.Task = task.ThrowIfNull("task");

            areAllAvailable = new BehaviorSubject<bool>(true);

            this.PauseCommand = new ReactiveCommand(areAllAvailable);
            this.PlayCommand = new ReactiveCommand(areAllAvailable);
            this.DeleteCommand = new ReactiveCommand(areAllAvailable);

            Observable.CombineLatest(PauseCommand.IsExecuting, PlayCommand.IsExecuting, (pa, pl) => !(pa || pl))
                      .Subscribe(n => areAllAvailable.OnNext(n));

            this.PlayCommand.RegisterAsyncTask(_ => this.SynologyService.ResumeTaskAsync(this.Task.Id)).Subscribe();
            this.PauseCommand.RegisterAsyncTask(_ => this.SynologyService.PauseTaskAsync(this.Task.Id)).Subscribe();
            this.DeleteCommand.RegisterAsyncTask(_ => this.SynologyService.DeleteTaskAsync(this.Task.Id)).Subscribe();
        }

        public string VisualState
        {
            get
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
            }
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

        public ReactiveCommand PlayCommand { get; set; }
        public ReactiveCommand PauseCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public string Title { get { return this.Task.Title; } }
        public DownloadTask.status Status { get { return this.Task.Status; } }

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
