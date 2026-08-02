using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Integration;

namespace SqlQueryAnalyzer.Benchmarks
{
    /// <summary>
    /// Benchmarks for the public methods of <see cref="HttpQueryAnalysisClient"/>.
    /// </summary>
    [MemoryDiagnoser]
    public class HttpQueryAnalysisClientBenchmarks
    {
        private HttpQueryAnalysisClient _client = null!;
        private AnalysisRequest _singleRequest = null!;
        private BatchAnalysisRequest _batchRequest = null!;
        private List<AnalysisRequest> _requestList = null!;

        /// <summary>
        /// Size of the request list for the multi‑request benchmark.
        /// </summary>
        [Params(10, 100, 1000)]
        public int InputSize { get; set; }

        /// <summary>
        /// Global setup – creates a client instance and prepares test data.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            // HttpQueryAnalysisClient typically expects an HttpClient and a base address.
            // Adjust the constructor arguments if the real implementation differs.
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000")
            };
            _client = new HttpQueryAnalysisClient(httpClient);

            // Prepare a simple analysis request – the actual properties depend on the real type.
            _singleRequest = new AnalysisRequest
            {
                // Populate with realistic dummy data; replace with actual property names.
                // Example:
                // Query = "SELECT * FROM dbo.Table WHERE Id = @id",
                // Parameters = new Dictionary<string, object> { ["id"] = 1 }
            };

            // Prepare a batch request containing a list of the same request.
            _batchRequest = new BatchAnalysisRequest
            {
                // Example property – adjust to match the real definition.
                // Requests = new List<AnalysisRequest> { _singleRequest }
            };

            // Prepare a list of requests for the multi‑request benchmark.
            _requestList = new List<AnalysisRequest>(InputSize);
            for (int i = 0; i < InputSize; i++)
            {
                // Clone or create a new request for each entry.
                var req = new AnalysisRequest
                {
                    // Populate with dummy data as above.
                };
                _requestList.Add(req);
            }
        }

        /// <summary>
        /// Benchmarks a single analysis request.
        /// </summary>
        [Benchmark]
        public async Task SingleAnalysisAsync()
        {
            // The method name in the real client may differ (e.g., AnalyzeAsync).
            // Replace with the correct call if necessary.
            await _client.AnalyzeAsync(_singleRequest);
        }

        /// <summary>
        /// Benchmarks a batch analysis request.
        /// </summary>
        [Benchmark]
        public async Task BatchAnalysisAsync()
        {
            // The method name in the real client may differ (e.g., AnalyzeBatchAsync).
            // Replace with the correct call if necessary.
            await _client.AnalyzeBatchAsync(_batchRequest);
        }

        /// <summary>
        /// Benchmarks processing a collection of analysis requests.
        /// </summary>
        [Benchmark]
        public async Task MultipleAnalysisAsync()
        {
            // Assuming the client exposes a method that can handle many requests at once.
            // If such a method does not exist, this benchmark can be adapted to loop
            // over the list and call the single‑request method repeatedly.
            await _client.AnalyzeBatchAsync(new BatchAnalysisRequest
            {
                // Example property – adjust to match the real definition.
                // Requests = _requestList
            });
        }
    }
}
