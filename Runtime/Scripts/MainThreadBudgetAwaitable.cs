// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;

namespace GLTFast
{
    /// <summary>
    /// Awaitable returned by <see cref="MainThreadBudget.YieldAsync()"/>.
    /// </summary>
    readonly struct MainThreadBudgetAwaitable
    {
        readonly DeferringSynchronizationContext m_Scheduler;
        readonly float m_PredictedDurationSeconds;

        internal MainThreadBudgetAwaitable(DeferringSynchronizationContext scheduler, float predictedDurationSeconds)
        {
            m_Scheduler = scheduler;
            m_PredictedDurationSeconds = predictedDurationSeconds;
        }

        /// <summary>
        /// Gets the awaiter for this awaitable.
        /// </summary>
        /// <returns>The awaiter.</returns>
        public Awaiter GetAwaiter() => new(m_Scheduler, m_PredictedDurationSeconds);

        /// <summary>
        /// Awaits main-thread time granted by glTFast.
        /// </summary>
        internal readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly DeferringSynchronizationContext m_Scheduler;
            readonly float m_PredictedDurationSeconds;

            internal Awaiter(DeferringSynchronizationContext scheduler, float predictedDurationSeconds)
            {
                m_Scheduler = scheduler;
                m_PredictedDurationSeconds = predictedDurationSeconds;
            }

            /// <summary>
            /// True if work may continue right away.
            /// </summary>
            public bool IsCompleted => m_Scheduler == null || m_Scheduler.MayContinueInline(m_PredictedDurationSeconds);

            /// <summary>
            /// Ends the await.
            /// </summary>
            public void GetResult() { }

            /// <summary>
            /// Schedules the continuation to resume on the main thread once glTFast grants main-thread time.
            /// </summary>
            /// <param name="continuation">The continuation to resume.</param>
            public void OnCompleted(Action continuation) => Schedule(continuation);

            /// <inheritdoc cref="OnCompleted"/>
            public void UnsafeOnCompleted(Action continuation) => Schedule(continuation);

            void Schedule(Action continuation)
            {
                if (m_Scheduler == null)
                    continuation();
                else
                    m_Scheduler.Schedule(continuation);
            }
        }
    }
}
