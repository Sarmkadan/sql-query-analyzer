#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates index analysis, fragmentation detection, and optimization
class IndexAnalyzer
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddScoped<IIndexAnalyzerService, IndexAnalyzerService>()
            .BuildServiceProvider();

        var indexAnalyzer = services.GetRequiredService<IIndexAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<IndexAnalyzer>>();

        logger.LogInformation("Index Analysis Example");
        logger.LogInformation("======================\n");

        // Analyze indexes on Orders table
        await AnalyzeTableIndexes(indexAnalyzer, "Orders", logger).ConfigureAwait(false);

        logger.LogInformation("\n---\n");

        // Check for fragmented indexes
        await AnalyzeFragmentation(indexAnalyzer, logger).ConfigureAwait(false);

        logger.LogInformation("\n---\n");

        // Find unused indexes
        await FindUnusedIndexes(indexAnalyzer, logger).ConfigureAwait(false);

        logger.LogInformation("\n---\n");

        // Generate maintenance scripts
        await GenerateMaintenanceScripts(indexAnalyzer, logger).ConfigureAwait(false);
    }

    static async Task AnalyzeTableIndexes(
        IIndexAnalyzerService indexAnalyzer,
        string tableName,
        ILogger logger)
    {
        logger.LogInformation("Analyzing indexes on table: {TableName}\n", tableName);

        try
        {
            var suggestions = await indexAnalyzer.AnalyzeIndexesAsync(tableName).ConfigureAwait(false);

            if (suggestions.Count == 0)
            {
                logger.LogInformation("No missing index suggestions for this table.");
                return;
            }

            logger.LogInformation("Found {Count} index opportunity/ies:\n", suggestions.Count);

            foreach (var suggestion in suggestions.OrderByDescending(s => s.Roi).Take(5))
            {
                logger.LogInformation("Suggested Index: {SuggestedIndexName}", suggestion.SuggestedIndexName);
                logger.LogInformation("  Table: {TableName}", suggestion.TableName);
                logger.LogInformation($"  Columns: {string.Join(", ", suggestion.Columns)}");

                if (suggestion.IncludedColumns?.Count > 0)
                {
                    logger.LogInformation($"  Included: {string.Join(", ", suggestion.IncludedColumns)}");
                }

                logger.LogInformation("  ROI: {Roi}%", suggestion.Roi);
                logger.LogInformation("  Est. Size: {EstimatedSizeKB} KB", suggestion.EstimatedSizeKB);
                logger.LogInformation("  Est. Improvement: {EstimatedImprovementPercent}%", suggestion.EstimatedImprovementPercent);
                logger.LogInformation();
                logger.LogInformation($"  SQL: {suggestion.ToCreateIndexSql()}");
                logger.LogInformation();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to analyze indexes");
        }
    }

    static async Task AnalyzeFragmentation(
        IIndexAnalyzerService indexAnalyzer,
        ILogger logger)
    {
        logger.LogInformation("Analyzing index fragmentation...\n");

        try
        {
            // Get fragmented indexes (> 10%)
            var fragmented = await indexAnalyzer.GetFragmentedIndexesAsync(10.0).ConfigureAwait(false);

            if (fragmented.Count == 0)
            {
                logger.LogInformation("✓ No significantly fragmented indexes (all < 10%)");
                return;
            }

            // Categorize by fragmentation level
            var rebuild = fragmented.Where(i => i.FragmentationPercent > 30).ToList();
            var reorganize = fragmented.Where(i => i.FragmentationPercent <= 30).ToList();

            logger.LogWarning("Found {Count} fragmented indexes:\n", fragmented.Count);

            if (rebuild.Count > 0)
            {
                logger.LogError("REBUILD (Fragmentation > 30%): {Count} indexes", rebuild.Count);
                foreach (var idx in rebuild.OrderByDescending(i => i.FragmentationPercent).Take(5))
                {
                    logger.LogError("  • {TableName}.{IndexName}", idx.TableName, idx.IndexName);
                    logger.LogError("    Fragmentation: {FragmentationPercent}%", idx.FragmentationPercent);
                    logger.LogError("    Size: {SizeInKB} KB", idx.SizeInKB);
                    logger.LogError("    Action: ALTER INDEX [{IndexName}] ON [{TableName}] REBUILD;", idx.IndexName, idx.TableName);
                }
            }

            if (reorganize.Count > 0)
            {
                logger.LogWarning("\nREORGANIZE (10% < Fragmentation <= 30%): {Count} indexes", reorganize.Count);
                foreach (var idx in reorganize.OrderByDescending(i => i.FragmentationPercent).Take(3))
                {
                    logger.LogWarning("  • {TableName}.{IndexName}: {FragmentationPercent}%", idx.TableName, idx.IndexName, idx.FragmentationPercent);
                    logger.LogWarning("    ALTER INDEX [{IndexName}] ON [{TableName}] REORGANIZE;", idx.IndexName, idx.TableName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to analyze fragmentation");
        }
    }

    static async Task FindUnusedIndexes(
        IIndexAnalyzerService indexAnalyzer,
        ILogger logger)
    {
        logger.LogInformation("Looking for unused indexes...\n");

        try
        {
            var unused = await indexAnalyzer.GetUnusedIndexesAsync().ConfigureAwait(false);

            if (unused.Count == 0)
            {
                logger.LogInformation("✓ All indexes are being used");
                return;
            }

            logger.LogWarning("Found {Count} unused indexes that could be removed:\n", unused.Count);

            var totalWastedSpace = 0L;

            foreach (var idx in unused.OrderByDescending(i => i.SizeInKB).Take(10))
            {
                logger.LogWarning("  • {TableName}.{IndexName}", idx.TableName, idx.IndexName);
                logger.LogWarning("    Type: {IndexType}", idx.IndexType);
                logger.LogWarning("    Size: {SizeInKB} KB", idx.SizeInKB);
                logger.LogWarning("    Drops: {UnusedDays} days since last use", idx.UnusedDays);
                logger.LogWarning("    Action: DROP INDEX [{IndexName}] ON [{TableName}];", idx.IndexName, idx.TableName);
                totalWastedSpace += idx.SizeInKB;
                logger.LogWarning();
            }

            logger.LogWarning($"Total wasted space: {totalWastedSpace:N0} KB ({totalWastedSpace / 1024.0:F2} MB)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to find unused indexes");
        }
    }

    static async Task GenerateMaintenanceScripts(
        IIndexAnalyzerService indexAnalyzer,
        ILogger logger)
    {
        logger.LogInformation("Generating maintenance scripts...\n");

        try
        {
            var scripts = await indexAnalyzer.GenerateMaintenanceScriptsAsync().ConfigureAwait(false);

            if (scripts.Count == 0)
            {
                logger.LogInformation("No maintenance needed.");
                return;
            }

            logger.LogInformation("Generated {Count} maintenance scripts:\n", scripts.Count);

            // Save to file
            var scriptPath = "./index_maintenance.sql";
            var scriptContent = string.Join("\n\nGO\n\n", scripts);
            await File.WriteAllTextAsync(scriptPath, scriptContent).ConfigureAwait(false);

            logger.LogInformation("✓ Maintenance scripts saved to: {ScriptPath}", scriptPath);
            logger.LogInformation($"\nScript Preview (first 3):");

            foreach (var script in scripts.Take(3))
            {
                logger.LogInformation($"\n{script.Substring(0, Math.Min(100, script.Length))}...");
            }

            if (scripts.Count > 3)
            {
                logger.LogInformation($"\n... and {scripts.Count - 3} more scripts");
            }

            logger.LogInformation($"\nTo apply changes, execute:\n");
            logger.LogInformation("sqlcmd -S YOUR_SERVER -d YOUR_DATABASE -i {ScriptPath}", scriptPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate maintenance scripts");
        }
    }
}
