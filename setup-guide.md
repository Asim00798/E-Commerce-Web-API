# E-Commerce Modular Monolith — Setup and Configuration Guide

**Project:** E-Commerce Modular Monolith

**Platform:** .NET 8 / ASP.NET Core Web API

**Database:** SQL Server

**Architecture:** Clean Architecture, Domain-Driven Design (DDD), CQRS,
Modular Monolith

**Version:** 1.0 — Final

------------------------------------------------------------------------

## 1. Purpose

This guide explains how to configure, build, and run the E-Commerce
application in a local development environment.

It covers:

-   Prerequisites and development tools.
-   Solution structure.
-   Application configuration.
-   Environment variables and secret management.
-   SQL Server setup.
-   Automatic EF Core database migrations.
-   ReadModel database migrations through DbUp.
-   Redis configuration.
-   HTTPS and Kestrel.
-   CORS and API versioning.
-   Swagger configuration.
-   Authentication and security.
-   External service integrations.
-   File storage.
-   Logging and observability.
-   Application startup and verification.
-   Troubleshooting.
-   Production deployment considerations.

### Important: Automatic Database Migrations

The application is configured to execute database migrations
automatically during startup.

You do not need to run the normal migration commands manually as part of
the standard startup workflow, provided that:

-   Automatic migrations are enabled.
-   The relevant migration runners are registered.
-   The database is reachable.
-   The configured database credentials have the necessary permissions.

EF Core migrations and DbUp migrations remain separate processes, even
when they target the same SQL Server database.

### Critical Security Warning: Connection Strings and Secrets

**Never hardcode real connection strings, database passwords, API keys,
JWT signing secrets, refresh-token hashing keys, verification HMAC keys,
or other credentials into source code or commit them to source
control.**

Use the following configuration strategy:

| Environment | Recommended configuration mechanism |
|---|---|
| Local development | Environment variables, .NET User Secrets, or an appropriate local secrets-management mechanism |
| Local container development | Environment variables supplied through Docker Compose; keep secret-bearing environment files out of source control |
| Production | Deployment environment variables or a dedicated secrets-management service |

The application's `appsettings.json` and `appsettings.Development.json`
files may contain non-sensitive defaults and configuration structure,
but **real credentials must be supplied externally**.

An environment variable is a configuration-delivery mechanism, not
automatically a complete secrets-management solution. Access to
environment variables must still be controlled, and secrets must not be
exposed in logs, source control, diagnostic output, or deployment
artifacts.

------------------------------------------------------------------------

## 2. Prerequisites

Install or configure the following before running the application.

### 2.1 .NET SDK

Install the .NET 8 SDK.

Verify the installation:

`dotnet --info`

`dotnet --list-sdks`

Confirm that a compatible .NET 8 SDK is available.

If the repository contains a `global.json` file, use an SDK version
compatible with that file.

### 2.2 SQL Server

Install or configure a supported SQL Server instance.

The database configuration assumes a database named:

E-Commerce

The example server name is:

.

In a SQL Server connection string, `Server=.` refers to the local
machine's default SQL Server instance.

This configuration assumes that SQL Server is installed locally and can
be reached using that name.

If you use SQL Server Express, LocalDB, a named instance, Docker, or a
remote SQL Server, update the connection strings accordingly.

### 2.3 Redis

The supplied Redis configuration uses:

localhost:6379

Install Redis locally or provide a reachable Redis instance.

Verify that the service is running and listening on the configured port
before testing features that depend on Redis.

### 2.4 Development Tools

Recommended tools:

-   Visual Studio 2022 with the ASP.NET and web development workload, or
    a compatible IDE.
-   Git.
-   SQL Server Management Studio or another SQL client.
-   A REST client such as Postman or the IDE's built-in HTTP client.

### 2.5 External Service Accounts

Some integrations require credentials or provider accounts:

-   Paymob for payment processing.
-   Twilio for SMS.
-   An SMTP provider for email.
-   Firebase credentials for push notifications.
-   Google OAuth credentials if Google authentication is enabled.

These integrations may remain unconfigured during initial local
development if neither application startup nor the features being tested
require them.

However, placeholder credentials must never be treated as valid provider
credentials.

------------------------------------------------------------------------

## 3. Solution Structure

The solution contains five primary projects.

E-Commerce.sln

│

├── E-Commerce.Api

├── E-Commerce.Application

├── E-Commerce.Domain

├── E-Commerce.Infrastructure

└── E-Commerce.ReadModel

**Project Responsibilities**

**Project**

**Responsibility**

E-Commerce.Api

HTTP endpoints, middleware, startup composition, and dependency
registration

E-Commerce.Application

Commands, queries, handlers, application workflows, and abstractions

E-Commerce.Domain

Aggregates, business rules, value objects, and domain events

E-Commerce.Infrastructure

EF Core persistence, ASP.NET Identity implementation, external
integrations, background jobs, and caching

E-Commerce.ReadModel

Specialized ADO.NET queries and DbUp-managed SQL scripts

The API project is the startup project.

Infrastructure and ReadModel implementations are registered through the
composition root.

The Domain and Application projects should not require
infrastructure-specific configuration merely to compile.

------------------------------------------------------------------------

## 4. Obtain and Open the Solution

Clone the repository if you have not already obtained it:

git clone `<repository-url>`{=html}

cd `<repository-directory>`{=html}

Replace the placeholders with the repository's actual URL and directory.

List the projects:

`dotnet sln list`

Restore dependencies:

`dotnet restore`

Build the solution:

`dotnet build`

Resolve build errors before attempting to run the application.

------------------------------------------------------------------------

## 5. Configure `appsettings.json`

The API project contains the application's configuration files.

E-Commerce.Api/

├── `appsettings.json`

├── `appsettings.Development.json`

├── `Program.cs`

└── Properties/

└── `launchSettings.json`

The exact files may differ depending on the repository.

The supplied configuration should be treated as a **local development
baseline**, not as a production configuration containing real
credentials.

Configuration values should be reviewed against the options classes and
validators implemented in the solution.

A setting only has an effect if the application actually binds to or
reads it.

### 5.1 Configuration Providers and Environment Variables

ASP.NET Core supports configuration from multiple sources, including
JSON files, environment variables, and other configured providers.

With the default ASP.NET Core configuration setup, environment variables
override corresponding values from `appsettings.json` and
appsettings.{Environment}.json.

For hierarchical configuration keys, use double underscores (\_\_) in
environment variable names to represent colons (:).

For example:

**Configuration key**

**Environment variable**

ConnectionStrings:`DefaultConnection`

ConnectionStrings\_\_`DefaultConnection`

ReadModel:`MigrationConnectionString`

ReadModel\_\_`MigrationConnectionString`

Payment:Paymob:SecretKey

Payment\_\_Paymob\_\_SecretKey

Jwt:Secret

Jwt\_\_Secret

Authentication:Google:ClientSecret

Authentication\_\_Google\_\_ClientSecret

This allows the application to continue using its normal configuration
keys while receiving sensitive values externally.

**Important:** This behavior assumes the default environment-variable
configuration provider is registered. Verify custom configuration
providers, provider ordering, and configuration binding if the solution
uses a customized setup.

### 5.2 Security Rules for Configuration Files

-   Do not commit real connection strings or secrets.
-   Do not put production credentials in `appsettings.json` or
    `appsettings.Development.json`.
-   Do not place real secrets in `launchSettings.json`.
-   Do not log configuration objects containing credentials.
-   Use appropriate access controls for environment variables and secret
    stores.
-   Validate required settings so missing or invalid credentials produce
    clear errors.

Configuration files can contain non-sensitive settings such as port
numbers, feature limits, provider names, logging levels, and application
metadata.

------------------------------------------------------------------------

## 6. HTTPS and Kestrel

The configuration includes an HTTPS endpoint similar to:

``` json
{

  "Kestrel": {

    "Endpoints": {

      "Https": {

        "Url":
"https://*:60449"

      }

    }

  }

}
```

This configures a Kestrel HTTPS endpoint on port 60449 across the
available network interfaces, subject to the application's actual
Kestrel configuration.

The expected local HTTPS address is:

https://localhost:60449

The exact result also depends on endpoint configuration and the HTTPS
certificate setup.

### 6.1 Configure the Development Certificate

Check the .NET development certificate:

dotnet dev-certs https \--check

If necessary, trust the development certificate:

dotnet dev-certs https \--trust

Restart the application after changing certificate configuration.

### 6.2 Certificate Requirements

The development certificate must be valid for the hostname used by the
client.

The configured Kestrel endpoint does not itself create or trust a
certificate.

If the application fails to start because no suitable HTTPS certificate
is available, inspect the Kestrel and certificate configuration before
troubleshooting the database.

------------------------------------------------------------------------

## 7. CORS Configuration

The supplied configuration is similar to:

``` json
{

  "Cors": {

    "AllowedOrigins": [

      "https://localhost:5001",

      "https://localhost:60449"

    ],

    "AllowCredentials": true

  }

}
```

The origins represent browser-based clients allowed to make cross-origin
requests.

For example:

https://localhost:5001

may represent a local frontend development server, while:

https://localhost:60449

represents the configured API origin.

Update these origins if your frontend runs on another address or port.

**Important Considerations**

-   Origins must match the browser's actual origin, including scheme and
    port.
-   Do not use a wildcard origin with credentialed CORS requests.
-   CORS does not authenticate users or authorize operations.
-   CORS does not prevent non-browser clients from calling the API.

If a browser reports a CORS error, verify the frontend URL, API URL,
configured origin list, and CORS middleware registration.

------------------------------------------------------------------------

## 8. SQL Server Connection Strings

The application defines two connection-string settings:

-   ConnectionStrings:`DefaultConnection`
-   ReadModel:`MigrationConnectionString`

**Security warning: These settings contain database connection
information and must not contain hardcoded production credentials.
Supply the actual connection strings through environment variables or an
appropriate secrets-management service.**

### 8.1 Default Connection

The configuration key is:

ConnectionStrings:`DefaultConnection`

The corresponding environment variable is:

ConnectionStrings\_\_`DefaultConnection`

This connection string is used by the write-side Infrastructure
persistence configuration.

It is expected to support EF Core operations and automatic EF Core
migrations.

The runtime account must have the permissions required for normal
application operations.

During automatic migrations, the migration identity must additionally
have the necessary schema-modification permissions.

### 8.2 ReadModel Migration Connection

The configuration key is:

ReadModel:`MigrationConnectionString`

The corresponding environment variable is:

ReadModel\_\_`MigrationConnectionString`

This setting is used by the ReadModel DbUp migration runner.

It provides a separate configuration path for migration credentials and
permissions.

The current configuration can point to the same database as
`DefaultConnection`.

**Separate connection-string settings do not imply separate physical
databases.**

They allow the application to use different credentials or database
targets in the future without changing the ReadModel migration
implementation.

### 8.3 Configure the SQL Server Instance

The local connection-string configuration assumes:

-   SQL Server is installed locally.
-   The default instance is available through ..
-   SQL Server authentication is enabled if SQL authentication is used.
-   The credentials are valid.
-   The account can connect to the target database.
-   The startup migration implementation supports any required
    database-creation operations.

If you use Windows authentication, a named instance, LocalDB, or a
remote server, adapt the connection string to your environment.

### 8.4 Configure Connection Strings Through Environment Variables

For local development, you can set the variables in PowerShell before
starting the application.

Example using SQL Server authentication:

\$env:ConnectionStrings\_\_`DefaultConnection` =
"Server=localhost;Database=E-Commerce;User
Id=`<development-user>`{=html};Password=`<development-password>`{=html};`TrustServerCertificate=True`;"

\$env:ReadModel\_\_`MigrationConnectionString` =
"Server=localhost;Database=E-Commerce;User
Id=`<migration-user>`{=html};Password=`<migration-password>`{=html};`TrustServerCertificate=True`;"

dotnet run --project
.`\E`{=tex}-Commerce.Api`\E`{=tex}-Commerce.Api.csproj

Replace the placeholders with appropriate local credentials.

The example uses `TrustServerCertificate=True` for development
convenience. Configure proper certificate validation for production.

The variables set this way are available to processes started from that
PowerShell session. They are not automatically permanent system-wide
settings.

If the application is launched through Visual Studio, configure the
variables through a suitable local development configuration mechanism.
Do not put actual credentials into a committed launch profile.

### 8.5 Database Security Requirements

**Never use sa as the application's routine production database
identity.**

For production:

-   Prefer a dedicated application database account.
-   Apply least-privilege permissions.
-   Separate runtime and migration permissions where practical.
-   Store credentials in an approved secret store or deployment
    environment.
-   Avoid exposing connection strings in logs or diagnostic output.
-   Rotate credentials if they have been exposed.

`TrustServerCertificate=True` may be convenient for local development,
but certificate validation should be configured appropriately for
production.

------------------------------------------------------------------------

## 9. Automatic Database Migrations

**Database migrations are configured to run automatically during
application startup.**

The supplied configuration is:

``` json
{

  "Database": {

    "Migrations": {

      "Enabled": true,

      "RetryCount": 3,

      "RetryDelaySeconds": 5

    }

  }

}
```

The application runs its migration process during startup provided that
the migration startup component is registered and enabled.

You do not need to execute EF Core migration commands manually during
the normal startup workflow.

### 9.1 Migration Settings

**Setting**

**Example value**

**Meaning**

Database:Migrations:Enabled

true

Enables automatic migration execution

Database:Migrations:RetryCount

3

Configured retry count for recognized transient failures

Database:Migrations:RetryDelaySeconds

5

Delay between configured retry attempts

The exact retry behavior depends on the implementation.

In the current architecture, retries are intended for recognized
transient SQL Server failures, not every possible migration error.

Invalid SQL, incompatible schema changes, and insufficient permissions
should not be treated as transient network failures.

### 9.2 Startup Migration Workflow

The intended workflow is:

Application Startup

| 

v

Read Migration Configuration

| 

v

Check Whether Migrations Are Enabled

| 

v

Connect to SQL Server

| 

v

Run EF Core Migrations

| 

v

Apply Pending Schema Changes

| 

v

Continue Startup

If ReadModel migrations are also registered for startup, the DbUp
migration runner executes its own process.

The two migration systems remain separate:

**Migration system**

**Responsibility**

EF Core migrations

Write-side Infrastructure schema

DbUp

ReadModel SQL scripts, including stored procedures

The application should not use EF Core migrations to manage ReadModel
stored procedures that are owned by DbUp.

### 9.3 What Happens When Migrations Fail?

Possible causes include:

-   SQL Server is unavailable.
-   Connection-string settings are invalid.
-   Authentication fails.
-   The database cannot be accessed.
-   Migration SQL is invalid.
-   The migration account lacks required permissions.
-   A schema change conflicts with the existing database.

Transient failures may be retried according to the configured policy.

Non-transient failures should be investigated and corrected.

If migrations are required for startup, a migration failure may prevent
the application from starting successfully. This is preferable to
silently running against an incompatible schema.

### 9.4 Disable Automatic Migrations

The configuration supports a migration kill switch:

``` json
{

  "Database": {

    "Migrations": {

      "Enabled": false

    }

  }

}
```

When disabled, the automatic migration component should skip migration
execution.

**Disabling automatic migrations does not apply pending migrations.**
The database must already have a compatible schema, or migrations must
be performed through an explicitly managed deployment process.

### 9.5 Migrations Versus Data Seeding

Migrations change the database schema.

Data seeding inserts or updates initial data.

The configuration separately defines:

``` json
{

  "DataSeeding": {

    "`CreateSeedAdmin`": false

  }

}
```

Automatic schema migration does not mean every seeding operation is
automatically enabled.

Seeders execute according to their own registration, configuration, and
implementation.

Administrator email and password values, if required by the seeder, must
be supplied through secure configuration rather than hardcoded into the
JSON file.

------------------------------------------------------------------------

## 10. ReadModel DbUp Migrations

The ReadModel subsystem uses DbUp to manage SQL scripts.

These scripts may define or update stored procedures used by specialized
ADO.NET queries.

The intended startup flow is:

API Startup

| 

v

ReadModel Migration Runner

| 

v

ReadModel Migration Connection

| 

v

DbUp Script Discovery

| 

v

Applied-Script Journal Check

| 

v

Execute Pending SQL Scripts

| 

v

Continue Startup

DbUp records successfully applied scripts in its migration journal.

Existing applied scripts should be treated as immutable. Schema or
stored-procedure changes should be introduced through new migration
scripts.

**Important Setup Checks**

-   Confirm that the ReadModel migration runner is invoked at startup.
-   Confirm that the migration connection string is correct.
-   Confirm that the migration identity can execute the required DDL.
-   Confirm that the scripts are included in the expected assembly or
    deployment location.
-   Confirm that script naming and ordering follow the project's
    convention.

The presence of a connection string alone does not execute migrations.
The migration runner must be part of the application's startup workflow.

------------------------------------------------------------------------

## 11. Redis Configuration

The supplied Redis configuration is:

``` json
{

  "Redis": {

    "Configuration":
"localhost:6379"

  }

}
```

The application expects Redis to be reachable at the configured
endpoint.

Redis is an Infrastructure dependency, not a Domain or Application
implementation detail.

Before testing Redis-backed functionality:

1.  Start Redis.
2.  Confirm that it listens on port 6379.
3.  Confirm that the configured hostname resolves.
4.  Start the API.
5.  Check startup logs and the Redis health-check result, if exposed.

If Redis is unavailable, behavior depends on the individual feature and
its failure policy.

Do not assume every Redis-backed operation automatically falls back to a
database or in-memory cache.

------------------------------------------------------------------------

## 12. Logging Configuration

The configuration uses standard .NET logging levels and Serilog.

### 12.1 Standard Logging

Example configuration:

``` json
{

  "Logging": {

    "LogLevel": {

      "Default": "Debug",

      "Microsoft.AspNetCore":
"Information",

      "Microsoft.Hosting.Lifetime":
"Information"

    }

  }

}
```

This configures the standard logging filters.

### 12.2 Serilog

Serilog is configured with console and rolling-file sinks.

The file sink writes logs under:

logs/ecommerce-.log

The configuration includes:

-   Console logging.
-   Rolling-file logging.
-   A seven-file retention limit.
-   A 10 MiB file-size limit.
-   Roll-on-size behavior.
-   Log-context enrichment.
-   Correlation ID output.

Ensure the application has permission to create and write to the
configured log directory.

The directory may be created automatically by the sink, depending on its
configuration and path behavior, but deployment permissions should still
be verified.

### 12.3 Development Versus Production

Debug logging is useful during local development but may produce
excessive output in production.

Production logging levels should be reviewed to balance diagnostic
value, performance, storage usage, and sensitive-data exposure.

**Never log passwords, connection strings containing credentials,
authentication tokens, OTPs, payment secrets, signing keys, or other
sensitive configuration values.**

------------------------------------------------------------------------

## 13. Swagger and API Versioning

The supplied Swagger configuration includes:

``` json
{

  "Swagger": {

    "Title": "E-Commerce
API",

    "Version": "v1",

    "Description": "E-Commerce
Modular Monolith API",

    "ContactName": "API
Team",

    "ContactEmail":
"api@ecommerce.local"

  },

  "Versioning": {

    "DefaultMajorVersion": 1,

    "DefaultMinorVersion": 0

  }

}
```

These settings describe the API and its versioning defaults.

If Swagger is enabled in the current environment, access the OpenAPI
interface using the route configured in the application.

For example:

https://localhost:60449/swagger

This is the expected default path if the application maps Swagger UI
there.

The API uses URL-segment versioning, so an endpoint may follow a pattern
such as:

/api/v1/products

The exact route depends on the controller and versioning configuration.

Swagger availability does not establish that the application is healthy
or that every external dependency is configured.

------------------------------------------------------------------------

## 14. Rate Limiting

The supplied configuration includes:

``` json
{

  "RateLimiting": {

    "Ip": {

      "PermitLimit": 100,

      "WindowSeconds": 60,

      "QueueLimit": 0

    },

    "User": {

      "PermitLimit": 50,

      "WindowSeconds": 60,

      "SegmentsPerWindow": 4,

      "QueueLimit": 0

    },

    "Global": {

      "PermitLimit": 500,

      "QueueLimit": 50

    }

  }

}
```

These values define configured limits for IP-based, user-based, and
global rate-limiting policies, subject to the application's
implementation.

The intended configuration indicates:

-   An IP policy allowing up to 100 permits per 60-second window.
-   A user policy allowing up to 50 permits per 60-second window, with
    four segments per window.
-   A global policy with a permit limit of 500 and a queue limit of 50.

The exact partitioning, queueing, and window behavior depends on how the
rate limiters are registered and applied.

After startup, test the actual endpoint policies rather than assuming
every policy is applied globally.

Rate limiting is especially important for login, verification,
password-reset, and other abuse-sensitive endpoints.

------------------------------------------------------------------------

## 15. Application Configuration

The supplied configuration includes application metadata:

``` json
{

  "App": {

    "Name":
"E-Commerce.Api",

    "Version": "1.0.0"

  }

}
```

It also includes business and infrastructure settings.

These should be reviewed before enabling their associated features.

### 15.1 Ordering

``` json
{

  "Ordering": {

    "MaximumItemsPerCart": 500,

    "PendingOrderExpirationHours": 24

  }

}
```

These values configure cart and pending-order behavior where the
corresponding options are bound and used.

They are application policy settings, not SQL Server configuration.

### 15.2 Shipping

``` json
{

  "Shipping": {

    "MaximumDeliveryAttempts": 3,

    "Location": {

      "CompanyLatitude": 25.2048,

      "CompanyLongitude": 55.2708

    }

  }

}
```

The location coordinates represent the configured company location for
the relevant shipping functionality.

Replace them with the actual business location before using
location-dependent calculations in a real deployment.

### 15.3 Outbox

``` json
{

  "Outbox": {

    "MaxRetryCount": 5

  }

}
```

This configures the maximum retry count where the outbox implementation
binds and uses this option.

The retry policy must remain consistent with the actual job and
dispatcher behavior.

Avoid adding a second independent retry loop that duplicates Hangfire's
retry policy.

------------------------------------------------------------------------

## 16. Payment Provider Configuration

The payment configuration specifies Paymob:

``` json
{

  "Payment": {

    "Provider": "Paymob",

    "Paymob": {

      "IntegrationId":
"000000",

      "BaseUrl":
"https://accept.paymob.com/api"

    }

  }

}
```

This example contains only non-secret configuration and an illustrative
integration identifier. Replace the identifier with the appropriate
value for your Paymob environment.

The sensitive settings are supplied separately:

**Configuration key**

**Environment variable**

Payment:Paymob:ApiKey

Payment\_\_Paymob\_\_ApiKey

Payment:Paymob:SecretKey

Payment\_\_Paymob\_\_SecretKey

Payment:Paymob:WebhookSecret

Payment\_\_Paymob\_\_WebhookSecret

**Security warning:** Never hardcode or commit real Paymob API keys,
secret keys, or webhook secrets. Supply them through environment
variables or an appropriate secrets-management service.

Before testing real payment requests, configure valid sandbox
credentials and the correct integration identifier.

The webhook secret must match the provider's configured
webhook-verification secret.

Payment testing should verify:

-   Payment initiation.
-   Provider response handling.
-   Webhook signature verification.
-   Duplicate callback handling.
-   Payment state transitions.
-   Failure handling.
-   Outbox processing for resulting integration events.

Do not mark payments as successful merely because the client returns to
a success page. The application must rely on the authoritative payment
workflow and verified provider information.

------------------------------------------------------------------------

## 17. Email Configuration

The supplied email configuration is:

``` json
{

  "Email": {

    "Host": "smtp.local",

    "Port": 587,

    "SecureSocketOption":
"Auto",

    "Username": "",

    "SenderName":
"E-Commerce",

    "SenderEmail":
"noreply@example.com",

    "TimeoutSeconds": 30

  }

}
```

The SMTP password is supplied separately:

**Configuration key**

**Environment variable**

Email:Password

Email\_\_Password

The host and credentials must be replaced with valid settings for the
selected SMTP service.

**Never commit a real SMTP password to `appsettings.json` or source
control.**

The host smtp.local is an illustrative placeholder and will not
necessarily be reachable.

If email is required for registration verification or password reset,
configure a working SMTP provider before testing those workflows end to
end.

------------------------------------------------------------------------

## 18. SMS Configuration

The supplied non-secret configuration is:

``` json
{

  "Sms": {

    "FromNumber":
"+10000000000"

  }

}
```

The example sender number is a placeholder and must be replaced with a
supported sender number.

Sensitive Twilio settings are supplied separately:

**Configuration key**

**Environment variable**

Sms:AccountSid

Sms\_\_AccountSid

Sms:AuthToken

Sms\_\_AuthToken

**Never hardcode or commit a real Twilio authentication token or other
provider credentials.**

Configure a valid Twilio account, authentication token, and sender
number before testing SMS delivery.

Verify that the sender number and destination are supported by the
provider and the account's current permissions.

SMS verification depends on both the application workflow and successful
delivery through the configured provider.

------------------------------------------------------------------------

## 19. Push Notifications

The supplied configuration is:

``` json
{

  "Push": {

    "CredentialFilePath":
""

  }

}
```

The path is empty, so push-notification credentials have not been
configured in this example.

If push notifications are required, configure the Firebase credential
file path according to the implementation's expectations.

The corresponding environment variable is:

Push\_\_CredentialFilePath

For example, the value may be a path to a service-account JSON file
provisioned securely for the application.

Ensure that:

-   The file exists.
-   The application process can read it.
-   The credential belongs to the correct Firebase project.
-   The file is excluded from source control.
-   The deployment environment securely provides the credential.

The path itself is configuration, but the service-account file contains
sensitive credentials.

**Do not commit Firebase service-account JSON files to source control.**

Do not assume push delivery is available until the Firebase
configuration has been validated.

------------------------------------------------------------------------

## 20. JWT and Refresh Tokens

The non-secret JWT configuration may be represented as:

``` json
{

  "Jwt": {

    "Issuer":
"https://localhost:60449",

    "Audience":
"ecommerce-clients",

    "ExpiryMinutes": 15

  },

  "RefreshToken": {

    "TokenLifetime":
"30.00:00:00"

  }

}
```

The sensitive keys are supplied separately:

**Configuration key**

**Environment variable**

Jwt:Secret

Jwt\_\_Secret

RefreshTokenHasher:SecretKey

RefreshTokenHasher\_\_SecretKey

**Security warning:** Never hardcode or commit JWT signing secrets or
refresh-token hashing keys. Generate suitable cryptographically secure
values and provide them through environment variables or an appropriate
secrets-management service.

These settings control:

-   JWT issuer.
-   JWT audience.
-   Token-signing configuration.
-   Access-token lifetime.
-   Refresh-token lifetime.
-   Refresh-token hashing.

Ensure that:

-   The JWT issuer matches the intended API identity.
-   The audience matches the intended clients.
-   Token signing uses the configured secret correctly.
-   Access-token lifetime is appropriate.
-   Refresh-token expiration is enforced.
-   Refresh-token hashes are stored and compared according to the
    implementation.
-   Secrets are not logged or committed to source control.

If a secret has ever been used in a shared or publicly accessible
environment, rotate it.

------------------------------------------------------------------------

## 21. Verification Codes

The verification subsystem uses an HMAC secret.

Its configuration key is:

Verification:HmacSecretKey

The corresponding environment variable is:

Verification\_\_HmacSecretKey

The secret should be generated securely and supplied externally.

**Do not hardcode or commit the HMAC secret.**

Verification testing should cover:

-   Code generation.
-   Code expiration.
-   Attempt limits.
-   Incorrect-code handling.
-   Email and phone verification.
-   Duplicate requests.
-   Registration lifecycle transitions.

Do not log verification codes or their underlying secrets.

The code lifetime and attempt policy should be taken from the actual
verification and registration options rather than inferred from this
HMAC setting.

------------------------------------------------------------------------

## 22. Google Authentication

The configuration structure is:

``` json
{

  "Authentication": {

    "Google": {

      "ClientId": "",

      "ClientSecret": ""

    }

  }

}
```

Google authentication is not configured in this example.

If Google login is enabled, configure:

**Configuration key**

**Environment variable**

Authentication:Google:ClientId

Authentication\_\_Google\_\_ClientId

Authentication:Google:ClientSecret

Authentication\_\_Google\_\_ClientSecret

Although a client ID is not generally equivalent to a client secret in
sensitivity, externalizing both keeps the environment-specific
configuration consistent.

**Never commit a real Google OAuth client secret to source control.**

If Google authentication is enabled:

1.  Register the application with the relevant Google identity platform.
2.  Configure the correct client ID and client secret.
3.  Configure the required redirect URI.
4.  Provide the credentials securely.
5.  Test the complete login and account-linking workflow.

If Google authentication is not being tested, leave the feature disabled
or unconfigured according to the implementation's validation and startup
behavior.

------------------------------------------------------------------------

## 23. File Storage

The configuration includes:

``` json
{

  "FileStorage": {

    "Provider": "Local",

    "MaxFileSizeBytes": 10485760,

    "AllowedContentTypes": [

      "image/jpeg",

      "image/png",

      "image/webp"

    ],

    "AllowedExtensions": [

      ".jpg",

      ".jpeg",

      ".png",

      ".webp"

    ],

    "OrphanGracePeriod":
"00:30:00",

    "DeletionClaimLease":
"00:10:00",

    "Local": {

      "RootPath":
"storage/files"

    }

  }

}
```

The configured maximum file size is 10 MiB.

The permitted content types and extensions are restricted to JPEG, PNG,
and WebP images.

The configured orphan grace period is 30 minutes, and the deletion claim
lease is 10 minutes.

**Setup Requirements**

-   Ensure the application can create and write to the storage
    directory.
-   Confirm that the configured root path resolves as expected for the
    application's working directory.
-   Test permitted and rejected file types.
-   Test files exceeding the configured maximum size.
-   Test cleanup and deletion workflows.
-   Ensure stored files are not exposed through unintended public paths.

The extension and content-type checks should be supplemented by the
application's actual content-inspection and storage-security mechanisms.

For production, confirm that local disk storage is appropriate for the
deployment topology and backup strategy.

------------------------------------------------------------------------

## 24. Data Seeding

The supplied non-secret configuration is:

``` json
{

  "DataSeeding": {

    "`CreateSeedAdmin`": false

  }

}
```

`CreateSeedAdmin` is set to false.

Therefore, the configuration does not request creation of the seed
administrator.

Other seeders may still run if independently registered and enabled. The
exact behavior depends on the startup seeding implementation.

If a seed administrator is required for local development, the
corresponding credentials can be supplied using:

**Configuration key**

**Environment variable**

DataSeeding:AdminEmail

DataSeeding\_\_AdminEmail

DataSeeding:AdminPassword

DataSeeding\_\_AdminPassword

The password must not be hardcoded into a committed configuration file.

To create a seed administrator:

1.  Enable the setting according to the application's seeding policy.
2.  Supply valid development credentials through a secure configuration
    mechanism.
3.  Run the application.
4.  Confirm that the configured seeder creates the expected identity and
    authorization data.
5.  Disable or remove development credentials when no longer needed.

Do not use predictable seed-administrator credentials in production.

------------------------------------------------------------------------

## 25. Configuring Swagger Through `launchSettings.json`

The `launchSettings.json` file configures how the E-Commerce.Api project
starts during local development, including the application URLs,
environment, and whether the browser opens automatically to Swagger UI.

**File Location**

E-Commerce.Api/

└── Properties/

└── `launchSettings.json`

**Configuration**

The existing launch profile is:

``` json
{

  "$schema":
"https://json.schemastore.org/launchsettings.json",

  "profiles":
{

    "E-Commerce.Api": {

      "commandName":
"Project",

      "dotnetRunMessages": true,

      "`launchBrowser`": true,

      "`launchUrl`":
"swagger",

      "applicationUrl":
"https://localhost:60449;http://localhost:60450",

      "environmentVariables": {

        "ASPNETCORE_ENVIRONMENT":
"Development"

      }

    }

  }

}
```

**What Each Setting Does**

**Setting**

**Purpose**

commandName: "Project"

Runs the API project directly.

dotnetRunMessages: true

Displays startup messages when running the application.

`launchBrowser`: true

Opens a browser automatically when the application starts through the
selected launch profile.

`launchUrl`: "swagger"

Opens Swagger UI instead of the API root URL.

applicationUrl

Configures the local HTTP and HTTPS URLs for the launch profile.

ASPNETCORE_ENVIRONMENT: "Development"

Runs the application in the Development environment, allowing
development-specific configuration and middleware.

**Accessing Swagger**

When you run E-Commerce.Api using this launch profile, the browser
automatically opens Swagger UI at:

-   **HTTPS:** https://localhost:60449/swagger
-   **HTTP:** http://localhost:60450/swagger

Use the HTTPS URL by default during local development.

The HTTP URL will work only if the application actually listens on that
endpoint. Because the Kestrel configuration explicitly defines an HTTPS
endpoint, verify that it does not override or conflict with the HTTP URL
supplied by the launch profile.

**Important Considerations**

1.  **Swagger must be registered and enabled in the API.** Setting
    `launchUrl` only tells the development tooling which URL to open; it
    does not enable Swagger by itself.
2.  **Development-only exposure:** If Swagger is configured to run only
    in the Development environment, it will not automatically be
    available in Production. This is generally appropriate for an
    internal or development API unless production API documentation is
    intentionally exposed with suitable controls.
3.  **Launch profiles are development tooling configuration.**
    `launchSettings.json` does not configure production hosting or
    determine production listening URLs.
4.  **Do not store secrets in `launchSettings.json`.** The file is
    commonly committed to source control. Keep its environment variables
    limited to non-sensitive development settings, such as
    ASPNETCORE_ENVIRONMENT, and supply credentials externally.

**Summary**

`launchBrowser` and `launchUrl` provide a convenient development
experience by automatically opening Swagger UI when the API starts.

They do not affect the application's business logic or API behavior, and
they do not enable Swagger independently of the application's Swagger
registration.

------------------------------------------------------------------------

## 26. Proxy and Forwarded Headers

The configuration includes:

``` json
{

  "Proxy": {

    "Enabled": false,

    "ForwardLimit": 1,

    "ForwardedHeaders": [

      "XForwardedFor",

      "XForwardedProto"

    ],

    "KnownProxies": [],

    "KnownNetworks": []

  }

}
```

Proxy processing is disabled.

For direct local development, this is appropriate when the API is
accessed directly.

If the API is later hosted behind a reverse proxy or load balancer:

1.  Enable proxy processing according to the implementation.
2.  Configure trusted proxy addresses or networks.
3.  Configure the forwarded headers that the proxy actually sends.
4.  Verify forwarded scheme and client-address handling.
5.  Ensure untrusted clients cannot spoof forwarded-header values.

Do not enable forwarded-header processing with broad trust merely to
make HTTPS redirects or client IP detection work.

------------------------------------------------------------------------

## 27. Automatic Startup Workflow

The expected startup workflow is:

Start API

| 

v

Load Configuration

| 

v

Validate Required Options

| 

v

Register Application Services

| 

v

Register Infrastructure Services

| 

v

Register ReadModel Services

| 

v

Execute Configured Startup Operations

| 

├── EF Core Migrations

| 

├── ReadModel DbUp Migrations

| 

└── Configured Seeders / Bootstrappers

| 

v

Build and Run HTTP Pipeline

| 

v

Listen on Configured HTTP/HTTPS Endpoints

This is the intended architectural flow.

The precise ordering of migrations, seeding, and hosted services depends
on the actual `Program.cs` and startup extensions.

A startup migration failure may prevent the application from reaching
the listening state.

Required configuration should be validated so that missing credentials
or invalid connection strings produce clear errors rather than obscure
downstream failures.

------------------------------------------------------------------------

## 28. Run the Application

From the solution directory, run:

dotnet run --project
.`\E`{=tex}-Commerce.Api`\E`{=tex}-Commerce.Api.csproj

This uses the project's default launch profile when one is selected by
the tooling.

Watch the console output for:

-   Configuration-validation failures.
-   SQL Server connection failures.
-   EF Core migration results.
-   ReadModel migration results.
-   Data-seeding output.
-   Redis connectivity problems.
-   HTTPS certificate errors.
-   Kestrel startup messages.

If startup succeeds, Kestrel should listen on the configured endpoints,
subject to the actual Kestrel and launch-profile configuration.

The expected local HTTPS API base address is:

https://localhost:60449

With the E-Commerce.Api launch profile configured to open Swagger, the
browser should navigate to:

https://localhost:60449/swagger

**Supplying Secrets Before Startup**

For a local development run, set the required environment variables in
the shell before launching the application.

For example:

\$env:ConnectionStrings\_\_`DefaultConnection` =
"Server=localhost;Database=E-Commerce;User
Id=`<development-user>`{=html};Password=`<development-password>`{=html};`TrustServerCertificate=True`;"

\$env:ReadModel\_\_`MigrationConnectionString` =
"Server=localhost;Database=E-Commerce;User
Id=`<migration-user>`{=html};Password=`<migration-password>`{=html};`TrustServerCertificate=True`;"

dotnet run --project
.`\E`{=tex}-Commerce.Api`\E`{=tex}-Commerce.Api.csproj

Supply any additional secrets required by the features being tested
using the corresponding environment variables documented in this guide.

Do not place real credentials in the command examples committed to
source control.

PowerShell session-scoped variables are inherited by processes started
from that session. For long-term local development, choose an
appropriate local secrets mechanism and follow its access-control
requirements.

------------------------------------------------------------------------

## 29. Verify the Setup

Use the following checklist.

### 29.1 Build Verification

-   `dotnet restore` completes successfully.
-   `dotnet build` completes successfully.
-   The API starts without configuration-validation errors.

### 29.2 Database Verification

-   SQL Server is running.
-   `DefaultConnection` is valid.
-   Automatic EF Core migrations execute successfully during startup.
-   The expected database and schema exist.
-   ReadModel DbUp migrations execute if configured for startup.
-   Expected stored procedures exist.
-   Configured seeders behave as expected.

### 29.3 Infrastructure Verification

-   Redis is reachable.
-   Hangfire starts if its configured dependencies are available.
-   File storage is writable.
-   Log files can be created.
-   Health checks return expected results.

### 29.4 API Verification

-   The HTTPS endpoint is reachable.
-   The development certificate is trusted.
-   Swagger is available in the configured environment.
-   API versioning routes resolve.
-   CORS works for the intended frontend origin.
-   Authentication and authorization behave as expected.
-   Rate limiting is applied to the intended endpoints.
-   Global exception handling returns the expected error contract.

### 29.5 External Integrations

-   Paymob sandbox credentials are configured before payment testing.
-   SMTP is configured before email-dependent workflows are tested.
-   Twilio credentials are configured before SMS-dependent workflows are
    tested.
-   Firebase credentials are configured before push notifications are
    tested.
-   Google OAuth credentials are configured before Google authentication
    is tested.

### 29.6 Configuration Security

-   No real secrets are hardcoded in source code.
-   No real credentials are committed to `appsettings.json`.
-   No real credentials are committed to `appsettings.Development.json`.
-   `launchSettings.json` contains no real secrets.
-   Required environment variables are present.
-   Invalid or missing required settings are handled clearly.
-   Sensitive configuration values are not written to logs.

------------------------------------------------------------------------

## 30. Troubleshooting

**SQL Server Connection Failure**

Check:

-   SQL Server is running.
-   The server name is correct.
-   SQL Server authentication is enabled if using SQL authentication.
-   The account and password are correct.
-   The intended connection-string environment variable is set.
-   Firewall and network access permit the connection.
-   The database exists or can be created by the startup migration
    implementation.

**Migration Failure**

Check:

-   Database:Migrations:Enabled is true.
-   The correct connection string is loaded.
-   The migration account has the required permissions.
-   The failing migration or SQL script is valid.
-   SQL Server is available.
-   Startup logs identify whether the failure is transient or
    non-transient.

Do not repeatedly retry a deterministic schema or permission error.

**ReadModel Query Failure**

Check:

-   DbUp migrations were executed.
-   The expected stored procedure exists.
-   The procedure is in the expected schema.
-   The runtime database identity has the required execution permission.
-   The query's parameter names and SQL types match the stored
    procedure.

**Redis Connection Failure**

Check:

-   Redis is running.
-   The endpoint and port are correct.
-   The application can reach Redis.
-   Startup logs identify the affected service.

**HTTPS Failure**

Check:

-   Kestrel is configured for HTTPS.
-   The development certificate is available and trusted.
-   Port 60449 is not already occupied.
-   The configured hostname matches the certificate.

**Swagger Failure**

Check:

-   Swagger services are registered.
-   Swagger UI is mapped at the expected route.
-   Swagger is enabled in the current environment.
-   The selected launch profile opens the expected URL.
-   The application has successfully started.
-   Kestrel is listening on the expected endpoint.

Remember that `launchUrl`: "swagger" does not enable Swagger; it only
specifies the URL opened by the development tooling.

**CORS Failure**

Check:

-   The frontend origin exactly matches an allowed origin.
-   The API is using the expected CORS policy.
-   Credential settings are compatible with the frontend request.
-   The middleware pipeline handles CORS before the endpoint response is
    produced.

**Email, SMS, or Payment Failure**

Check:

-   Placeholder credentials have been replaced.
-   The correct environment variables are present.
-   The provider account is active.
-   The endpoint and credentials match the intended environment.
-   Required sender IDs, integration IDs, and webhook secrets are
    correct.
-   The provider's callback and redirect URLs match the deployed
    environment.

Do not print secret values into logs while troubleshooting. Check
whether a value is present and valid without exposing its contents.

------------------------------------------------------------------------

**31. Production Deployment Considerations**

The supplied configuration is suitable as a starting point for local
development, not as a production configuration without further review.

**Secrets**

-   Replace sample or development JWT and hashing secrets.
-   Use dedicated database credentials.
-   Replace Paymob placeholder values.
-   Configure SMTP, Twilio, Firebase, and OAuth credentials as required.
-   Inject credentials through the deployment environment or an approved
    secrets-management service.
-   Restrict access to secrets.
-   Rotate credentials if exposed.
-   Ensure secrets do not appear in logs, source control, build
    artifacts, or diagnostic output.

**Production connection strings and secrets must never be hardcoded into
the application or committed in configuration files.**

**Database**

-   Use a dedicated application database account.
-   Separate runtime and schema-migration privileges where practical.
-   Confirm that automatic migration execution matches the deployment
    process.
-   Coordinate migrations if multiple application instances may start
    concurrently.
-   Configure database backups and test restoration.

**HTTPS and Proxies**

-   Configure valid certificates.
-   Configure trusted proxies if deployed behind a reverse proxy.
-   Validate forwarded-header behavior.
-   Confirm that only intended endpoints are exposed.

**Logging and Observability**

-   Review production log levels.
-   Confirm log retention and disk-space policies.
-   Avoid sensitive-data logging.
-   Verify health-check exposure and alerting.
-   Monitor database connectivity, Redis, background jobs, and outbox
    processing.

**Integrations**

-   Use production provider credentials only in the production
    environment.
-   Configure and verify payment webhooks.
-   Test outbox delivery, retries, idempotency, and failed-message
    monitoring.
-   Ensure storage, Redis, SQL Server, and background processing have
    appropriate monitoring.

**Data and Operations**

-   Configure backups and restoration procedures.
-   Establish a migration and rollback policy.
-   Verify file-storage persistence and backup requirements.
-   Test startup failure behavior.
-   Confirm the application can recover from temporary dependency
    outages.
-   Ensure that configuration validation identifies missing mandatory
    settings.

------------------------------------------------------------------------

## 32. Final Setup Checklist

Before considering the local setup complete, verify the following.

**Environment and Build**

-   .NET 8 SDK installed.
-   Solution restored and built.
-   API starts without configuration-validation errors.

**Database**

-   SQL Server running.
-   ConnectionStrings\_\_`DefaultConnection` configured.
-   ReadModel\_\_`MigrationConnectionString` configured.
-   Automatic migrations enabled.
-   EF Core migrations complete successfully during startup.
-   ReadModel DbUp migrations complete successfully during startup, if
    registered.
-   Expected schema and stored procedures exist.

**Infrastructure**

-   Redis running.
-   HTTPS development certificate configured.
-   Kestrel listening on the expected endpoint.
-   File storage writable.
-   Logs being written.
-   Health checks available as configured.

**API**

-   Swagger available in the configured environment.
-   `launchSettings.json` opens the expected Swagger URL.
-   CORS origins match the frontend.
-   API versioning routes resolve.
-   Authentication and authorization behave as expected.
-   Rate limiting is applied to the intended endpoints.
-   Exception handling returns the expected error contract.

**External Integrations**

-   Required Paymob sandbox credentials configured.
-   SMTP configured before testing email-dependent workflows.
-   Twilio credentials configured before testing SMS-dependent
    workflows.
-   Firebase credentials configured before testing push notifications.
-   Google OAuth credentials configured before testing Google
    authentication.

**Configuration Security**

-   No real connection strings or secrets are hardcoded in source code.
-   No real credentials are committed to `appsettings.json` or
    `appsettings.Development.json`.
-   No real credentials are stored in `launchSettings.json`.
-   Required environment variables are configured.
-   Sensitive files, including Firebase service-account credentials, are
    excluded from source control.
-   Secrets are not exposed in logs.

------------------------------------------------------------------------

## 33. Final Summary

The E-Commerce modular monolith is configured through
`appsettings.json`, external configuration providers, and the API
composition root.

The API project is the startup project. It registers Application,
Infrastructure, and ReadModel services and coordinates the configured
startup workflow.

**Database migrations run automatically during startup when migration
execution is enabled and the relevant migration runners are
registered.**

EF Core migrations manage the write-side Infrastructure schema. DbUp
manages the specialized ReadModel SQL scripts. These remain separate
migration systems, even when they target the same SQL Server database.

For local development, the essential dependencies are:

1.  .NET 8 SDK.
2.  SQL Server.
3.  Redis.
4.  A valid HTTPS development certificate.
5.  Correct local configuration.

External providers such as Paymob, SMTP, Twilio, Firebase, and Google
OAuth need valid credentials when their corresponding integrations are
required.

Swagger is opened automatically through the configured development
launch profile when the API is started using the appropriate tooling and
the Swagger endpoint is enabled.

The application can be started from the solution directory using:

dotnet run --project
.`\E`{=tex}-Commerce.Api`\E`{=tex}-Commerce.Api.csproj

**Final Security Rule**

**Connection strings and secrets must be supplied through environment
variables or an appropriate secrets-management service. Do not hardcode
real credentials into source code, `appsettings.json`,
`appsettings.Development.json`, or `launchSettings.json`, and never
commit them to source control.**

The goal of this setup is to make local startup straightforward while
preserving the architecture's separation of concerns, migration
discipline, security boundaries, and production-oriented failure
handling.
