using System;

namespace SqlQueryAnalyzer.Tests
{
	/// <summary>
	/// Provides extension methods that provide convenient wrappers around the public test members of
	/// <see cref="QueryValidatorTests"/>. These helpers allow grouping related assertions
	/// and improve readability of test code without duplicating logic.
	/// </summary>
	public static class QueryValidatorTestsExtensions
	{
		/// <summary>
		/// Executes all public validity-check test methods on the supplied <see cref="QueryValidatorTests"/>
		/// instance.
		/// </summary>
		/// <param name="tests">The test class instance on which to invoke the checks. Must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
		public static void RunAllValidityChecks(this QueryValidatorTests tests)
		{
			ArgumentNullException.ThrowIfNull(tests);
			tests.IsValidQuery_WellFormedSelectStatement_ReturnsTrue();
			tests.IsValidQuery_EmptyString_ReturnsFalse();
			tests.IsValidQuery_TextWithNoRecognisedSqlKeyword_ReturnsFalse();
		}

		/// <summary>
		/// Verifies that the sanitization logic correctly truncates a query that exceeds the
		/// maximum display length, delegating to the existing test method.
		/// </summary>
		/// <param name="tests">The test class instance. Must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
		public static void VerifySanitizationTruncates(this QueryValidatorTests tests)
		{
			ArgumentNullException.ThrowIfNull(tests);
			tests.SanitizeQueryForDisplay_QueryLongerThanMaxLength_TruncatesWithEllipsis();
		}

		/// <summary>
		/// Executes the custom-rule validation test, ensuring that a registered rule is invoked exactly once.
		/// </summary>
		/// <param name="tests">The test class instance. Must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
		public static void EnsureCustomRuleInvoked(this QueryValidatorTests tests)
		{
			ArgumentNullException.ThrowIfNull(tests);
			tests.ValidateQuery_CustomRuleRegistered_RuleIsInvokedExactlyOnce();
		}
	}
}