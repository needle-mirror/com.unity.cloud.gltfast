// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace GLTFast
{
    /// <summary>
    /// A <see cref="SynchronizationContext"/> that resumes continuations on the main thread only while the
    /// <see cref="IDeferAgent"/> grants frame time. Installing it around a call makes every <c>await</c> in that
    /// call's tree a scheduling point that glTFast governs, so the called code does not have to consult the
    /// defer agent itself.
    /// </summary>
    sealed class DeferringSynchronizationContext : SynchronizationContext
    {
        static readonly SendOrPostCallback k_DrainCallback =
            state => ((DeferringSynchronizationContext)state).Drain();

        static readonly SendOrPostCallback k_InvokeActionCallback = state => ((Action)state)();

        readonly IDeferAgent m_DeferAgent;
        readonly Queue<WorkItem> m_Queue = new();
        readonly object m_QueueLock = new();

        /// <summary>
        /// The context that was current when this one was first installed; drains are scheduled through it, so this
        /// context inherits Unity's existing pump cadence instead of owning a pump.
        /// </summary>
        volatile SynchronizationContext m_Outer;

        CancellationToken m_CancellationToken;
        int m_MainThreadId;
        bool m_DrainScheduled;
        bool m_Draining;

        public DeferringSynchronizationContext(IDeferAgent deferAgent)
        {
            m_DeferAgent = deferAgent;
        }

        /// <summary>
        /// The scheduler governing the calling code, or null outside a governed call or off the main thread.
        /// </summary>
        internal static DeferringSynchronizationContext Ambient
        {
            get
            {
                var scheduler = Current as DeferringSynchronizationContext;
                return scheduler is { IsUsableOnCurrentThread: true } ? scheduler : null;
            }
        }

        bool IsUsableOnCurrentThread =>
            m_Outer != null && Thread.CurrentThread.ManagedThreadId == m_MainThreadId;

        /// <summary>
        /// Makes this context current until the returned scope is disposed. Call on the main thread, around the
        /// synchronous invocation of the code to govern; the callee runs up to its first <c>await</c>, capturing this
        /// context, and the scope then restores the previous context.
        /// </summary>
        /// <param name="cancellationToken">Token of the operation being governed. Once cancellation is requested,
        /// queued continuations run without waiting for the budget, so they can observe it and finish.</param>
        /// <returns>A scope that restores the previously current context. Inert if there is no context to schedule
        /// drains through, in which case behavior is unchanged.</returns>
        public InstallScope Install(CancellationToken cancellationToken)
        {
            var previous = Current;
            if (ReferenceEquals(previous, this))
                return default;

            // Never chain drains through another instance of this type; use the context that one schedules through.
            var outer = previous is DeferringSynchronizationContext nested ? nested.m_Outer : previous;
            if (outer == null)
                return default;

            if (m_Outer == null)
            {
                m_Outer = outer;
                m_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            m_CancellationToken = cancellationToken;

            SetSynchronizationContext(this);
            return new InstallScope(previous);
        }

        /// <summary>
        /// Yields until the defer agent grants main-thread time.
        /// </summary>
        /// <param name="predictedDurationSeconds">Predicted duration of the upcoming work in seconds. Zero queries
        /// the budget without a duration.</param>
        /// <returns>An awaitable that completes right away if this scheduler cannot resume on the calling thread.
        /// </returns>
        public MainThreadBudgetAwaitable YieldAsync(float predictedDurationSeconds) =>
            new(IsUsableOnCurrentThread ? this : null, predictedDurationSeconds);

        /// <summary>
        /// True if code running under this context may continue inline without yielding.
        /// </summary>
        internal bool MayContinueInline(float predictedDurationSeconds) =>
            ReferenceEquals(Current, this) && MayRun(predictedDurationSeconds);

        internal void Schedule(Action continuation) => Post(k_InvokeActionCallback, continuation);

        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));

            var outer = m_Outer;
            if (outer == null)
            {
                // Never installed, so there is no drain chain to append to.
                base.Post(d, state);
                return;
            }

            bool scheduleDrain;
            lock (m_QueueLock)
            {
                m_Queue.Enqueue(new WorkItem(d, state));
                scheduleDrain = !m_DrainScheduled;
                if (scheduleDrain)
                    m_DrainScheduled = true;
            }

            if (scheduleDrain)
                outer.Post(k_DrainCallback, this);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));

            // Blocking the main thread on its own queue would deadlock.
            if (Thread.CurrentThread.ManagedThreadId == m_MainThreadId)
            {
                d(state);
                return;
            }

            if (m_Outer != null)
                m_Outer.Send(d, state);
            else
                base.Send(d, state);
        }

        // The base implementation returns a plain context that posts to the thread pool. Copies have to share this
        // instance's queue and budget.
        public override SynchronizationContext CreateCopy() => this;

        void Drain()
        {
            if (m_Draining)
            {
                // Reentrant pump: this callback is consumed without draining anything, so clear the flag or the
                // in-progress drain will decline to reschedule and leave the queue stranded.
                lock (m_QueueLock)
                    m_DrainScheduled = false;
                return;
            }

            m_Draining = true;
            var previous = Current;

            // Continuations must observe this context as current, otherwise only the first await of a governed call
            // would be governed: SynchronizationContext.Current is thread-local and not flowed through ExecutionContext.
            SetSynchronizationContext(this);
            try
            {
                int count;
                lock (m_QueueLock)
                {
                    m_DrainScheduled = false;
                    count = m_Queue.Count;
                }

                // Work posted while draining waits for the next drain. Otherwise a continuation that reposts itself,
                // like `while (!done) await Task.Yield();`, never lets the frame end without a budget.
                for (; count > 0 && MayRun(0f) && TryTakeNext(out var item); count--)
                    item.Invoke();
            }
            finally
            {
                SetSynchronizationContext(previous);
                m_Draining = false;
                ScheduleDrainIfPending();
            }
        }

        bool TryTakeNext(out WorkItem item)
        {
            lock (m_QueueLock)
            {
                if (m_Queue.Count > 0)
                {
                    item = m_Queue.Dequeue();
                    return true;
                }
            }

            item = default;
            return false;
        }

        void ScheduleDrainIfPending()
        {
            var outer = m_Outer;
            if (outer == null)
                return;

            bool scheduleDrain;
            lock (m_QueueLock)
            {
                scheduleDrain = m_Queue.Count > 0 && !m_DrainScheduled;
                if (scheduleDrain)
                    m_DrainScheduled = true;
            }

            if (scheduleDrain)
                outer.Post(k_DrainCallback, this);
        }

        bool MayRun(float predictedDurationSeconds) =>
            m_CancellationToken.IsCancellationRequested || !ShouldDefer(predictedDurationSeconds);

        bool ShouldDefer(float predictedDurationSeconds)
        {
            // A destroyed MonoBehaviour defer agent must not leave queued continuations undrainable.
            if (m_DeferAgent == null || (m_DeferAgent is UnityEngine.Object agent && agent == null))
                return false;

            return predictedDurationSeconds > 0f
                ? m_DeferAgent.ShouldDefer(predictedDurationSeconds)
                : m_DeferAgent.ShouldDefer();
        }

        readonly struct WorkItem
        {
            readonly SendOrPostCallback m_Callback;
            readonly object m_State;

            public WorkItem(SendOrPostCallback callback, object state)
            {
                m_Callback = callback;
                m_State = state;
            }

            public void Invoke() => m_Callback(m_State);
        }

        /// <summary>
        /// Restores the previously current <see cref="SynchronizationContext"/> when disposed.
        /// </summary>
        internal readonly struct InstallScope : IDisposable
        {
            readonly SynchronizationContext m_Previous;
            readonly bool m_Restore;

            public InstallScope(SynchronizationContext previous)
            {
                m_Previous = previous;
                m_Restore = true;
            }

            public void Dispose()
            {
                if (m_Restore)
                    SetSynchronizationContext(m_Previous);
            }
        }
    }
}
