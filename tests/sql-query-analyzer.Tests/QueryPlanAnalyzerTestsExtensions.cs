using System;
using System.Threading.Tasks;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Extension methods that provide convenient wrappers around the public test methods
    /// of <see cref="QueryPlanAnalyzerTests"/>.
    /// These helpers can be used by other test suites or custom test runners to invoke the existing tests programmatically.
    /// </summary>
    public static class QueryPlanAnalyzerTestsExtensions
    {
        /// <summary>
        /// Executes the test that verifies an invalid query plan throws the expected exception.
        /// </summary>
        /// <param name="tests">The test instance to invoke the method on.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void VerifyInvalidQueryPlanThrows(this QueryPlanAnalyzerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException();
        }

        /// <summary>
        /// Executes the test that parses a valid XML execution plan and asserts a <see cref="global::SqlQueryAnalyzer.Models.QueryPlan"/> is returned.
        /// </summary>
        /// <param name="tests">The test instance to invoke the method on.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void VerifyValidPlanParsing(this QueryPlanAnalyzerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan();
        }

        /// <summary>
        /// Runs the asynchronous test that retrieves table scans from a plan containing scans.
        /// </summary>
        /// <param name="tests">The test instance to invoke the method on.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static async Task RunTableScansTestAsync(this QueryPlanAnalyzerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            await tests.GetTableScans_WithTableScans_ReturnsTableScans();
        }

        /// <summary>
        /// Runs the asynchronous test that retrieves missing‑index recommendations from a plan with table scans.
        /// </summary>
        /// <param name="tests">The test instance to invoke the method on.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static async Task RunMissingIndexesTestAsync(this QueryPlanAnalyzerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            await tests.GetMissingIndexes_WithTableScans_ReturnsRecommendations();
        }
    }
}
