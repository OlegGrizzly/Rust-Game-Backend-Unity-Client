using GameBackend.WebSocket;
using NUnit.Framework;

namespace GameBackend.Tests.WebSocket
{
    /// <summary>
    /// Tests for ReconnectHandler exponential backoff logic.
    /// These tests define the contract for reconnection delays (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by WS agent):
    ///   - GameBackend.WebSocket.ReconnectHandler
    ///
    /// ReconnectHandler manages exponential backoff: base delay doubles each attempt,
    /// capped at max delay. Reset() returns to base.
    /// </summary>
    [TestFixture]
    public class ReconnectHandlerTests
    {
        // =====================================================================
        // NextDelay — exponential backoff
        // =====================================================================

        [Test]
        public void NextDelay_FirstAttempt_ReturnsBaseDelay()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);

            var delay = handler.NextDelay();

            Assert.AreEqual(1f, delay, 0.01f);
        }

        [Test]
        public void NextDelay_ExponentialGrowth()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);

            Assert.AreEqual(1f, handler.NextDelay(), 0.01f);   // attempt 1
            Assert.AreEqual(2f, handler.NextDelay(), 0.01f);   // attempt 2
            Assert.AreEqual(4f, handler.NextDelay(), 0.01f);   // attempt 3
            Assert.AreEqual(8f, handler.NextDelay(), 0.01f);   // attempt 4
            Assert.AreEqual(16f, handler.NextDelay(), 0.01f);  // attempt 5
            Assert.AreEqual(30f, handler.NextDelay(), 0.01f);  // attempt 6 — capped at max
        }

        [Test]
        public void NextDelay_NeverExceedsMax()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);

            // Call many times to go way past max
            float lastDelay = 0f;
            for (int i = 0; i < 20; i++)
            {
                lastDelay = handler.NextDelay();
            }

            Assert.LessOrEqual(lastDelay, 30f,
                "Delay should never exceed maxDelay");
        }

        // =====================================================================
        // Reset
        // =====================================================================

        [Test]
        public void Reset_ResetsToBase()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);

            // Advance a few times
            handler.NextDelay(); // 1
            handler.NextDelay(); // 2
            handler.NextDelay(); // 4

            handler.Reset();

            var delay = handler.NextDelay();
            Assert.AreEqual(1f, delay, 0.01f, "After Reset(), NextDelay should return baseDelay");
        }

        // =====================================================================
        // ShouldReconnect
        // =====================================================================

        [Test]
        public void ShouldReconnect_WhenEnabled_ReturnsTrue()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);
            handler.Enabled = true;

            Assert.IsTrue(handler.ShouldReconnect);
        }

        [Test]
        public void ShouldReconnect_WhenDisabled_ReturnsFalse()
        {
            var handler = new ReconnectHandler(baseDelay: 1f, maxDelay: 30f, multiplier: 2f);
            handler.Enabled = false;

            Assert.IsFalse(handler.ShouldReconnect);
        }
    }
}
