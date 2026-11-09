// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast
{
    /// <summary>
    /// Yields main-thread time back to glTFast from code running under a <see cref="DeferringSynchronizationContext"/>.
    /// </summary>
    /// <remarks>
    /// Outside governed code, and on threads other than the main thread, there is no frame budget to consult, and the
    /// returned awaitable completes right away.
    /// </remarks>
    static class MainThreadBudget
    {
        /// <summary>
        /// Yields until glTFast grants main-thread time again.
        /// </summary>
        /// <returns>An awaitable that completes when work may continue. Completes synchronously when the frame
        /// budget allows work to continue right away.</returns>
        public static MainThreadBudgetAwaitable YieldAsync() => YieldAsync(0f);

        /// <summary>
        /// Yields until glTFast grants main-thread time for work of the given predicted duration.
        /// </summary>
        /// <param name="predictedDurationSeconds">Predicted duration of the upcoming work in seconds, forwarded to
        /// <see cref="IDeferAgent.ShouldDefer(float)"/>. Zero queries the budget without a duration.</param>
        /// <returns>An awaitable that completes when work may continue. Completes synchronously when the frame
        /// budget allows work to continue right away.</returns>
        public static MainThreadBudgetAwaitable YieldAsync(float predictedDurationSeconds) =>
            new(DeferringSynchronizationContext.Ambient, predictedDurationSeconds);
    }
}
