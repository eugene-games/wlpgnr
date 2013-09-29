﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WallpaperGenerator.Utilities.ProgressReporting
{
    public sealed class ProgressReportScope : IDisposable, IObservable<double>, IObserver<double>
    {
        private double _progress;
        private double _progressBeforeChildScopeCreated;
        private bool _isCompleted;
        private readonly List<IObserver<double>> _progressObservers;
        private IDisposable _childScopeUnsubscriber;

        public double ChildScopeSpan { get; private set; }

        public ProgressReportScope ChildScope { get; private set; }

        public string Name { get; private set; }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value > 1)
                    throw new InvalidOperationException(GetExceptionMessage(string.Format("Attempt to setup progress with value \"{0}\" more then 1.", value)));
                _progress = value;
                _progressObservers.ForEach(o => o.OnNext(_progress));
            }
        }

        public ProgressReportScope([CallerMemberName] string name = "")
        {
            Name = name;
            _progressObservers = new List<IObserver<double>>();
        }

        public ProgressReportScope CreateChildScope(double childScopeSpan = 1, [CallerMemberName] string name = "") 
        {
            if (ChildScope != null)
                throw new InvalidOperationException(GetExceptionMessage("Child scope is alredy created and not completed yet."));

            if (childScopeSpan <= 0)
                throw new ArgumentException(GetExceptionMessage("ChildScopeSpan should be greater then 0."), "childScopeSpan");

            if (Progress + childScopeSpan > 1)
                throw new ArgumentException(GetExceptionMessage("Child scope span plus current progress is more then 1."), "childScopeSpan");

            ChildScopeSpan = childScopeSpan;
            ChildScope = new ProgressReportScope(name);
            _progressBeforeChildScopeCreated = Progress;
            _childScopeUnsubscriber = ChildScope.Subscribe(this);
            return ChildScope;
        }

        public void Complete()
        {
            if (_isCompleted)
                throw new InvalidOperationException(GetExceptionMessage("Progress scope is already completed."));

            if (ChildScope != null)
                ChildScope.Complete();

            Progress = 1;
            _isCompleted = true;

            _progressObservers.ForEach(o => o.OnCompleted());
        }
        
        public void Dispose()
        {
            Complete();
        }

        public IDisposable Subscribe(IObserver<double> progressObserver)
        {
            if (progressObserver == null)
                throw new ArgumentNullException("progressObserver");

            _progressObservers.Add(progressObserver);
            return new Disposable(() => _progressObservers.Remove(progressObserver));
        }

        void IObserver<double>.OnNext(double value)
        {
            Progress = _progressBeforeChildScopeCreated + value * ChildScopeSpan;
        }

        void IObserver<double>.OnError(Exception error)
        {
        }

        void IObserver<double>.OnCompleted()
        {
            ChildScope = null;
            _childScopeUnsubscriber.Dispose();
            _childScopeUnsubscriber = null;
            Progress = _progressBeforeChildScopeCreated + ChildScopeSpan;
        }

        private string GetExceptionMessage(string messageBase)
        {
            return string.Format("{0}=[Name:\"{1}\"]. {2}", GetType().Name, Name, messageBase);
        }
    }

    /*public int TicksCount { get; private set; }

        public int Ticks { get; private set; }

        public double Progress
        {
            get { return (double)Ticks / TicksCount; }
        }

        public bool IsCompleted
        {
            get { return Ticks >= TicksCount; }
        }

        public ProgressReportScope(string name, int ticksCount)
        {
            if (ticksCount < 1)
                throw new ArgumentException("ticksCount can't be less then 1.", "ticksCount");

            Name = name;
            TicksCount = ticksCount;
        }

        public void Tick()
        {
            if (IsCompleted)
                throw new InvalidOperationException(string.Format("Progress scope \"{0}\" is completed.", Name));

            Ticks++;
        }

        public void Complete()
        {
            Ticks = TicksCount;
        }*/
}
