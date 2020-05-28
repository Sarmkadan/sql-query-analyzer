using System.Threading.Tasks;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Extension methods that provide convenient wrappers around the public test methods
    /// of <see cref="QueryPlanAnalyzerTests"/>. These helpers can be used by other test
    /// suites or custom test runners to invoke the existing tests programmatically.
    /// </summary>
    public static class QueryPlanAnalyzerTestsExtensions
    {
        /// <summary>
        /// Executes the test that verifies an invalid query plan throws the expected exception.
        /// </summary>
        public static void VerifyInvalidQueryPlanThrows(this QueryPlanAnalyzerTests tests)
        {
            tests.AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException();
        }

        /// <summary>
        /// Executes the test that parses a valid XML execution plan and asserts a <c>QueryPlan</c> is returned.
        /// </summary>
        public static void VerifyValidPlanParsing(this QueryPlanAnalyzerTests tests)
        {
            tests.ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan();
        }

        /// <summary>
        /// Runs the asynchronous test that retrieves table scans from a plan containing scans.
        /// </summary>
        public static async Task RunTableScansTestAsync(this QueryPlanAnalyzerTests tests)
        {
            await tests.GetTableScans_WithTableScans_ReturnsTableScans();
        }

        /// <summary>
        /// Runs the asynchronous test that retrieves missing‑index recommendations from a plan with table scans.
        /// </summary>
        public static async Task RunMissingIndexesTestAsync(this QueryPlanAnalyzerTests tests)
        {
            await tests.GetMissingIndexes_WithTableScans_ReturnsRecommendations();
        }
    }
}
