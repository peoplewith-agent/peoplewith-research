// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Net;
using FluentAssertions;
using PeopleWithResearch;
using Xunit;

namespace PeopleWithResearch.UnitTests.Helpers;

/// <summary>
/// Tests for CrashDetected.LogCrash behaviour.
/// AC3: Accurate exceptions are caught and logged; transient network noise is filtered.
///
/// Strategy: CrashDetected.RecordSentryCrash is private/async. We test the
/// observable side-effects that are accessible without mocking Sentry SDK:
///   1. The method completes without throwing for all exception types.
///   2. AggregateException is correctly unwrapped (inner exception is used).
///   3. Ignored exception types do not cause observable error propagation.
///
/// Full Sentry integration is tested in the production Release build via the
/// Sentry dashboard. These unit tests cover the filtering and unwrapping logic.
/// </summary>
public class CrashDetectedTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // LogCrash overloads — do not throw
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LogCrash_WithNavigationOverload_DoesNotThrow()
    {
        var ex = new InvalidOperationException("test error");

        var action = () => CrashDetected.LogCrash(ex, null!, "TestContext");

        action.Should().NotThrow();
    }

    [Fact]
    public void LogCrash_WithoutNavigationOverload_DoesNotThrow()
    {
        var ex = new InvalidOperationException("test error");

        var action = () => CrashDetected.LogCrash(ex, "TestContext");

        action.Should().NotThrow();
    }

    [Fact]
    public void LogCrash_NullException_DoesNotThrow()
    {
        // RecordSentryCrash guards with: if (ex == null) return;
        var action = () => CrashDetected.LogCrash(null!, "TestContext");

        action.Should().NotThrow();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IgnoredExceptions filter (re-enabled per AC3)
    // Tests validate the LOGIC of the filter, not the Sentry SDK call itself.
    // We expose a testable IsIgnored helper or use reflection on the private array.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsIgnoredException_HttpRequestException_ReturnsTrue()
    {
        // AC3: HttpRequestException must be silently filtered (noise)
        var ex = new HttpRequestException("Network unavailable");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsIgnoredException_TaskCanceledException_ReturnsTrue()
    {
        var ex = new TaskCanceledException("Request timed out");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsIgnoredException_WebException_ReturnsTrue()
    {
        var ex = new WebException("Connection refused");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsIgnoredException_OperationCanceledException_ReturnsTrue()
    {
        var ex = new OperationCanceledException("Operation was cancelled");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsIgnoredException_NullReferenceException_ReturnsFalse()
    {
        // AC3: NullReferenceException is NOT transient noise — it must NOT be filtered
        var ex = new NullReferenceException("Object reference not set");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsIgnoredException_InvalidOperationException_ReturnsFalse()
    {
        var ex = new InvalidOperationException("Invalid state");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsIgnoredException_ArgumentNullException_ReturnsFalse()
    {
        var ex = new ArgumentNullException("param");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsIgnoredException_IndexOutOfRangeException_ReturnsFalse()
    {
        var ex = new IndexOutOfRangeException("Index was out of range");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AggregateException unwrapping
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void UnwrapException_AggregateWithInnerException_ReturnsInner()
    {
        // AC3: AggregateException must be unwrapped to InnerException
        var inner = new NullReferenceException("inner crash");
        var aggEx = new AggregateException("Wrapped", inner);

        var result = CrashDetected.UnwrapException(aggEx);

        result.Should().BeSameAs(inner);
    }

    [Fact]
    public void UnwrapException_AggregateWithNullInner_ReturnsSelf()
    {
        // Guard: if InnerException is null, return the AggregateException itself
        var aggEx = new AggregateException("No inner");

        var result = CrashDetected.UnwrapException(aggEx);

        result.Should().BeOfType<AggregateException>();
    }

    [Fact]
    public void UnwrapException_NonAggregateException_ReturnsSelf()
    {
        var ex = new InvalidOperationException("plain error");

        var result = CrashDetected.UnwrapException(ex);

        result.Should().BeSameAs(ex);
    }

    [Fact]
    public void UnwrapException_AggregateWrappingHttpRequest_IsFilteredAfterUnwrap()
    {
        // Verify the combined behaviour: AggregateException wrapping HttpRequestException
        // should be unwrapped → then the inner HttpRequestException should be filtered.
        var inner = new HttpRequestException("Network error");
        var aggEx = new AggregateException("Wrapped http error", inner);

        var unwrapped = CrashDetected.UnwrapException(aggEx);
        var isIgnored = CrashDetected.IsIgnoredException(unwrapped);

        isIgnored.Should().BeTrue("unwrapped HttpRequestException must be silently filtered");
    }

    [Fact]
    public void UnwrapException_AggregateWrappingNullRef_IsNotFilteredAfterUnwrap()
    {
        var inner = new NullReferenceException("crash in UI");
        var aggEx = new AggregateException("Wrapped NRE", inner);

        var unwrapped = CrashDetected.UnwrapException(aggEx);
        var isIgnored = CrashDetected.IsIgnoredException(unwrapped);

        isIgnored.Should().BeFalse("NullReferenceException must NOT be filtered");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Derived type filtering (IsAssignableFrom semantics)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsIgnoredException_DerivedFromTaskCanceledException_ReturnsTrue()
    {
        // OperationCanceledException is the base of TaskCanceledException
        // IsAssignableFrom must catch subclasses too
        var ex = new TaskCanceledException("sub-type cancel");

        var result = CrashDetected.IsIgnoredException(ex);

        result.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LogCrash does not propagate exceptions from the Sentry call
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LogCrash_WhenSentryNotInitialised_DoesNotBubbleException()
    {
        // Sentry SDK is not initialised in unit test context — the inner
        // try/catch in RecordSentryCrash must swallow any Sentry-internal errors.
        var ex = new Exception("generic crash");

        var action = () => CrashDetected.LogCrash(ex, "UnitTestContext");

        action.Should().NotThrow();
    }
}
