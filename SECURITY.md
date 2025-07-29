// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Security Policy

## Reporting Security Vulnerabilities

If you discover a security vulnerability in SQL Query Analyzer, **do not** open a public GitHub issue. Instead:

1. **Email**: Contact the maintainer with security details
2. **Include**: Vulnerability description, reproduction steps, potential impact
3. **Timeline**: Allow 90 days for patching before public disclosure
4. **Do Not**: Publicly disclose vulnerability until patch is released

We take all security reports seriously and appreciate responsible disclosure.

## Security Features

### Input Validation
- All SQL queries are validated before processing
- Size limits enforced (configurable, default 100KB)
- Special characters normalized and escaped

### SQL Injection Detection
Built-in detector for common SQL injection patterns:
- Syntax analysis of WHERE clauses
- Detection of suspicious string concatenation
- Warning on dynamic query construction

Example:
```csharp
var result = await analyzer.AnalyzeQueryAsync(userProvidedQuery);
if (result.Issues.Any(i => i.IssueType == "PotentialSqlInjection"))
{
    // Handle security risk
}
```

### Connection Security
- Connection strings encrypted in configuration
- Support for integrated authentication (Windows Auth)
- TLS/SSL support for database connections
- Connection pooling with size limits

Configuration:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
  }
}
```

### Authentication & Authorization
When integrated into ASP.NET applications:

```csharp
[Authorize(Roles = "Admin,DBA")]
[HttpPost("analyze")]
public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
{
    // Only authorized users can analyze
}
```

### Rate Limiting
API rate limiting available through middleware:

```csharp
services.AddRateLimiting(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.RemoteIpAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

### Logging & Monitoring
All security-relevant events are logged:

```csharp
logger.LogWarning("Suspicious query pattern detected in analysis request");
logger.LogError("Database connection failed with timeout");
logger.LogInformation("User {UserId} analyzed {QueryCount} queries", userId, count);
```

## Dependency Security

### Keeping Dependencies Updated

```bash
# Check for vulnerable packages
dotnet list package --outdated

# Update all packages
dotnet package update
```

### Current Dependencies
All dependencies are from Microsoft or trusted sources:
- `Microsoft.Extensions.*` - Microsoft official
- `System.Data.SqlClient` - Microsoft official
- `Npgsql` - Community-maintained, trusted

### Zero-Dependency Option
Core analysis works without external dependencies (except .NET runtime).

## Platform Security

### Windows
- Uses Windows Authentication when configured
- Supports DPAPI for secret encryption
- Runs as non-admin process

### Linux/Docker
- Non-root execution in Docker images
- SELinux support
- AppArmor compatible

### Cloud Platforms
- Azure Key Vault integration ready
- AWS Secrets Manager compatible
- GCP Secret Manager support

## Secure Configuration Examples

### .NET Application
```csharp
// Use environment variables for secrets
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

// Or use Azure Key Vault
var keyVaultUrl = "https://myvault.vault.azure.net/";
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri(keyVaultUrl), credential);
var secret = await client.GetSecretAsync("db-password");
```

### Docker
```bash
# Use environment file (never commit .env!)
docker run --env-file .env sql-query-analyzer:latest

# Or secrets in docker-compose
docker secret create db_password -
# Then reference in compose:
# DB_PASSWORD_FILE: /run/secrets/db_password
```

### Kubernetes
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: analyzer-secrets
type: Opaque
stringData:
  db-password: YourSecurePassword123!
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: analyzer
spec:
  template:
    spec:
      containers:
      - name: analyzer
        env:
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: analyzer-secrets
              key: db-password
```

## Security Best Practices

### For Users

1. **Update Regularly**
   ```bash
   dotnet tool update -g SqlQueryAnalyzer
   ```

2. **Use Least Privilege**
   - Create database accounts with minimal permissions
   - Don't use sa account for analysis
   - Use read-only accounts when possible

3. **Network Security**
   - Use VPN for remote database connections
   - Enable firewall rules
   - Use TLS/SSL for all connections

4. **Secret Management**
   - Never hardcode credentials
   - Use environment variables or secret managers
   - Rotate credentials regularly

### For Developers

1. **Code Review**
   - All changes reviewed before merge
   - Security-focused review required

2. **Dependency Management**
   - Regular dependency audits
   - Pin major versions for stability
   - Use `dotnet add package --version` for pinning

3. **Secure Coding**
   - Input validation on all boundaries
   - Output encoding for display
   - Proper error handling without info leakage

4. **Testing**
   - Unit tests for security features
   - Integration tests with real data
   - Negative test cases

## Vulnerability Handling Process

### Discovery
1. Vulnerability reported responsibly
2. Confirmed by maintainers
3. Severity assessed (Critical/High/Medium/Low)

### Development
1. Issue tracked internally (not public)
2. Patch developed on private branch
3. Fix reviewed and tested

### Release
1. Security patch released in new version
2. Security advisory published
3. GitHub security page updated
4. CVE assigned if applicable

### Timeline
- **Critical**: Patched within 7 days
- **High**: Patched within 30 days
- **Medium**: Patched within 90 days
- **Low**: Included in next scheduled release

## Compliance

### Standards Followed
- OWASP Top 10 Web Application Security Risks
- NIST Cybersecurity Framework
- CIS Benchmarks

### Data Protection
- No sensitive data stored in logs
- Query text never sent to external services
- Results only stored locally

### Encryption
- TLS 1.2+ for network communication
- Encrypted connection strings
- Optional at-rest encryption

## Responsible Disclosure

When we receive security vulnerability reports:
- ✓ We acknowledge receipt within 24 hours
- ✓ We provide status updates every 7 days
- ✓ We work with the reporter on disclosure timing
- ✓ We credit the researcher (if desired)
- ✓ We provide a CVE when appropriate

## Contact

For security issues, contact the maintainers through responsible disclosure channels. Do not use public issue tracker.

---

**Last Updated**: 2026-05-04  
**Policy Version**: 1.0
