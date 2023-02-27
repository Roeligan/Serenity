﻿using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Serenity.Data;

/// <summary>
/// Default connection factory
/// </summary>
public class DefaultSqlConnections : ISqlConnections
{
    protected readonly IConnectionStrings connectionStrings;
    protected readonly IConnectionProfiler profiler;
    protected readonly ILoggerFactory loggerFactory;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="connectionStrings">Named connection strings</param>
    /// <param name="profiler">Profiler if any</param>
    /// <param name="loggerFactory">Optional logger factory (to be used by static SqlHelper methods)</param>
    public DefaultSqlConnections(IConnectionStrings connectionStrings, IConnectionProfiler profiler = null, ILoggerFactory loggerFactory = null)
    {
        this.connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
        this.profiler = profiler;
        this.loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Lists all known connections strings
    /// </summary>
    /// <returns>List of all registered connections</returns>
    public IEnumerable<IConnectionString> ListConnectionStrings()
    {
        return connectionStrings.ListConnectionStrings();
    }

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> for given connection string, provider name and dialect.</summary>
    /// <param name="connectionString">Connection string</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="dialect">Dialect</param>
    /// <returns>A new <see cref="IDbConnection"/> object.</returns>
    public virtual IDbConnection New(string connectionString, string providerName, ISqlDialect dialect)
    {
        if (providerName == null)
            throw new ArgumentNullException(nameof(providerName));

        if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));

        var factory = DbProviderFactories.GetFactory(providerName);
        var connection = factory.CreateConnection();
        try
        {
            connection.ConnectionString = connectionString;
        }
        catch
        {
            connection.Dispose();
            return null;
        }

        if (profiler != null)
            return new WrappedConnection(profiler.Profile(connection), dialect, loggerFactory?.CreateLogger<ISqlConnections>());

        return new WrappedConnection(connection, dialect, loggerFactory?.CreateLogger<ISqlConnections>());
    }

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> for given connection key.</summary>
    /// <param name="connectionKey">Connection key</param>
    /// <returns>A new <see cref="IDbConnection"/> object.</returns>
    public virtual IDbConnection NewByKey(string connectionKey)
    {
        var info = connectionStrings.TryGetConnectionString(connectionKey);

        if (info == null)
            throw new InvalidOperationException(string.Format("No connection string with key {0} in configuration file!", connectionKey));

        return New(info.ConnectionString, info.ProviderName, info.Dialect);
    }

    /// <summary>
    /// Gets a connection string by its key
    /// </summary>
    /// <param name="connectionKey">Connection key</param>
    /// <returns>Connection string or null if not found</returns>
    public IConnectionString TryGetConnectionString(string connectionKey)
    {
        return connectionStrings.TryGetConnectionString(connectionKey);
    }
}