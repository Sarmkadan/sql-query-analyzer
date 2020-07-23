using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods for <see cref="Index"/> providing common calculations and formatting helpers.
/// </summary>
namespace SqlQueryAnalyzer.Models
{
    /// <summary>
    /// Provides additional functionality for <see cref="Index"/> objects.
    /// </summary>
    public static class IndexExtensions
    {
        /// <summary>
        /// Returns the fully qualified name of the index in the form <c>SchemaName.TableName.IndexName</c>.
        /// </summary>
        /// <param name="index">The index to format.</param>
        /// <returns>A string containing the qualified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="index"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when any of the name components are <c>null</c> or empty.</exception>
        public static string GetQualifiedName(this Index index)
        {
            ArgumentNullException.ThrowIfNull(index);
            ArgumentException.ThrowIfNullOrEmpty(index.SchemaName);
            ArgumentException.ThrowIfNullOrEmpty(index.TableName);
            ArgumentException.ThrowIfNullOrEmpty(index.IndexName);

            return $"{index.SchemaName}.{index.TableName}.{index.IndexName}";
        }

        /// <summary>
        /// Calculates the total number of user operations performed on the index
        /// (seeks, scans, lookups and updates).
        /// </summary>
        /// <param name="index">The index whose usage is being summed.</param>
        /// <returns>The sum of <see cref="Index.UserSeeks"/>, <see cref="Index.UserScans"/>,
        /// <see cref="Index.UserLookups"/> and <see cref="Index.UserUpdates"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="index"/> is <c>null</c>.</exception>
        public static long GetTotalUserOperations(this Index index)
        {
            ArgumentNullException.ThrowIfNull(index);
            return index.UserSeeks + index.UserScans + index.UserLookups + index.UserUpdates;
        }

        /// <summary>
        /// Returns the size of the index expressed in megabytes.
        /// </summary>
        /// <param name="index">The index whose size is to be converted.</param>
        /// <returns>The size in megabytes (MiB) as a <see cref="double"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="index"/> is <c>null</c>.</exception>
        public static double GetSizeInMegabytes(this Index index)
        {
            ArgumentNullException.ThrowIfNull(index);
            const double bytesPerMiB = 1024.0 * 1024.0;
            return index.SizeInBytes / bytesPerMiB;
        }

        /// <summary>
        /// Calculates the total number of columns that participate in the index,
        /// including both key columns and included columns.
        /// </summary>
        /// <param name="index">The index to evaluate.</param>
        /// <returns>The combined count of <see cref="Index.Columns"/> and <see cref="Index.IncludeColumns"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="index"/> is <c>null</c>.</exception>
        public static int GetEffectiveColumnCount(this Index index)
        {
            ArgumentNullException.ThrowIfNull(index);
            int keyCount = index.Columns?.Count ?? 0;
            int includeCount = index.IncludeColumns?.Count ?? 0;
            return keyCount + includeCount;
        }
    }
}
