# PeopleWithResearch.UnitTests

TDD test project for ticket **PD2-8** — written before production code.

## Status

Tests will **not compile** until `sub-write-code` creates the production files listed in the plan. This is expected and intentional.

## Compilation Blockers

| Type referenced in tests | Production file (plan) |
|--------------------------|------------------------|
| `ImperialViewModel` | `PeopleWithResearch/ViewModels/ImperialViewModel.cs` |
| `RegField` | existing `Models/RegField.cs` |
| `OptionDetails` | existing `Models/OptionDetails.cs` |
| `user`, `signupcode`, `newuser`, etc. | existing `Models/` files |
| `CrashDetected.IsIgnoredException` | `Helpers/CrashDetected.cs` — needs 2 new `internal static` helpers |
| `CrashDetected.UnwrapException` | `Helpers/CrashDetected.cs` — needs 2 new `internal static` helpers |

## MAUI Project Reference Note

The main `PeopleWithResearch.csproj` targets `net9.0-android;net9.0-ios;net9.0-maccatalyst` (MAUI workloads).
A plain `net9.0` test project **cannot directly reference** a MAUI TFM project.

**Resolution options** (for sub-write-code to choose):

1. **Extract shared logic** — move `ImperialViewModel`, models, and `CrashDetected` into a `PeopleWithResearch.Core` class library targeting `net9.0`. Both the MAUI app and the test project reference the shared library.
2. **Multi-targeting** — add `net9.0` as an additional TFM on the main project (`<TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0</TargetFrameworks>`) and guard MAUI-only APIs with `#if MAUI` or conditional includes.

Option 1 is cleaner for long-term testability. Option 2 is faster for a single ticket.

## CrashDetected Testability

`CrashDetectedTests.cs` tests `IsIgnoredException(Exception)` and `UnwrapException(Exception)`.
These must be added as `internal static` (or `public static`) helpers to `CrashDetected.cs`:

```csharp
// Add to Helpers/CrashDetected.cs
internal static bool IsIgnoredException(Exception ex)
    => IgnoredExceptions.Any(type => type.IsAssignableFrom(ex.GetType()));

internal static Exception UnwrapException(Exception ex)
    => ex is AggregateException aggEx ? aggEx.InnerException ?? ex : ex;
```

## Running Tests

```
dotnet test c:\Users\conal\Documents\PeopleWith-Research\PeopleWithResearch.UnitTests\PeopleWithResearch.UnitTests.csproj
```
