# MSSQLToSQLite - SQL Server to SQLite Database Converter

**Last Updated**: December 11, 2025  
**.NET Version**: 9.0  
**Platform**: Cross-platform (CLI) / Windows (GUI)

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Platform Support](#platform-support)
3. [Database Security](#database-security)
4. [Configuration Architecture](#configuration-architecture)
5. [Troubleshooting](#troubleshooting)
6. [Development Guide](#development-guide)

---

## Quick Start

### macOS / Linux (CLI Tool)

```bash
# Navigate to project
cd /Users/robertrunge/code/_robert/Macos-MSSQLToSQLite

# Build
dotnet build ClassLibrary/ClassLibrary.csproj

# SQL Server Authentication
dotnet run --project ClassLibrary/ClassLibrary.csproj -- \
  --conn-type sql \
  --conn-server "server.database.windows.net" \
  --conn-db "database-name" \
  --conn-user "username" \
  --conn-pass "password" \
  --sqlite-path "~/output.sqlite"

# Azure SQL with Azure CLI Token (recommended for Azure)
# First: az login
dotnet run --project ClassLibrary/ClassLibrary.csproj -- \
  --conn-type sql \
  --conn-server "server.database.windows.net" \
  --conn-db "database-name" \
  --conn-user "azure-token" \
  --conn-pass "$(az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv)" \
  --sqlite-path "~/output.sqlite"

# Or use the convenience script
./run-converter.sh --help
```

### Windows (GUI or CLI)

- **GUI**: Run `ApplicationForm` or `Converter` projects (Windows Forms)
- **CLI**: Same as macOS commands above

---

## Platform Support

| Project | Type | Windows | macOS | Linux |
|---------|------|---------|-------|-------|
| **ClassLibrary** | CLI Tool | ✅ | ✅ | ✅ |
| **DbAccess** | Library | ✅ | ✅ | ✅ |
| **ApplicationForm** | GUI (WinForms) | ✅ | ❌ | ❌ |
| **Converter** | GUI (WinForms) | ✅ | ❌ | ❌ |

**Important**: On macOS/Linux, use the ClassLibrary CLI tool only. Windows Forms apps require the Windows-only `Microsoft.WindowsDesktop.App` framework.

---

## Database Security

### Source Database (SQL Server) - READ-ONLY

All connections to SQL Server include `ApplicationIntent=ReadOnly` in the connection string, guaranteeing read-only access:

```csharp
// ClassLibrary/Program.cs
";ApplicationIntent=ReadOnly"
```

- ✅ Source database cannot be modified
- ✅ Enforced at connection level
- ✅ SQL Server rejects any write attempts

### Target Database (SQLite) - READ-WRITE

The SQLite database is created with default read-write permissions to receive the converted data.

---


## Configuration Architecture

### Centralized Settings

All projects use **Directory.Build.props** and **Directory.Packages.props** for consistent configuration:

**Directory.Build.props** - Common build properties:
- Target Framework: `net9.0` (default)
- Language Version: `latest`
- Implicit Usings: `enable`
- Nullable: `enable`
- Windows Forms projects override to `net9.0-windows`

**Directory.Packages.props** - Package versions:

| Package | Version | Used By |
|---------|---------|---------|
| Microsoft.Data.SqlClient | 5.2.2 | ClassLibrary (secure, cross-platform) |
| Microsoft.Data.Sqlite | 9.0.0 | ClassLibrary (cross-platform) |
| log4net | 2.0.15 | All projects |
| System.CommandLine | 2.0.0-beta4.22272.1 | ClassLibrary |

### Project Targets

| Project | TargetFramework | Platform |
|---------|----------------|----------|
| ClassLibrary | `net9.0` | Cross-platform |
| DbAccess | `net9.0` | Cross-platform |
| ApplicationForm | `net9.0-windows` | Windows only |
| Converter | `net9.0-windows` | Windows only |

### To Update Package Versions

Edit `Directory.Packages.props`:
```xml
<PackageVersion Include="log4net" Version="2.0.17" />
```

### To Change .NET Version

1. Edit `Directory.Build.props` (line 4):
   ```xml
   <TargetFramework Condition="'$(TargetFramework)' == ''">net10.0</TargetFramework>
   ```

2. Edit Windows Forms projects (`ApplicationForm.csproj`, `Converter.csproj`):
   ```xml
   <TargetFramework>net10.0-windows</TargetFramework>
   ```

---


## Troubleshooting

### Error: "Microsoft.WindowsDesktop.App not found"

**Cause**: Trying to run Windows Forms app on macOS.

**Fix**: Use ClassLibrary CLI tool:
```bash
./run-converter.sh --help
```

### Error: "PackageReference items cannot define a value for Version"

**Cause**: Version specified in PackageReference (should be in Directory.Packages.props).

**Fix**: Remove Version attribute from project file, add to Directory.Packages.props.

### Build Warnings: NU1507

**Message**: "There are 2 package sources defined..."

**Status**: Informational only, doesn't prevent building.

---

## Development Guide


### Building the Solution

```bash
dotnet build SqlConverter.sln
```

Note: Windows Forms projects build on macOS (cross-compilation) but cannot run.

### Platform-Specific Development

**Windows**:
- ✅ GUI: `ApplicationForm` or `Converter`
- ✅ CLI: `ClassLibrary`

**macOS / Linux**:
- ✅ CLI: `ClassLibrary`
- ❌ GUI: Not available (Windows Forms)

### Best Practices

1. ✅ Edit Directory.Build.props for solution-wide settings
2. ✅ Edit Directory.Packages.props for package versions
3. ✅ Keep project files minimal (only project-specific settings)
4. ❌ Don't add Version to PackageReference in project files
5. ❌ Don't repeat common properties in project files
6. ❌ Don't use different .NET versions across projects

### To Upgrade .NET Version

1. Edit `Directory.Build.props` (line 4):
   ```xml
   <TargetFramework Condition="'$(TargetFramework)' == ''">net10.0</TargetFramework>
   ```

2. Edit Windows Forms projects:
   - `ApplicationForm/ApplicationForm.csproj` (line 5)
   - `Converter/Converter.csproj` (line 5)
   ```xml
   <TargetFramework>net10.0-windows</TargetFramework>
   ```

3. Test: `dotnet build`

### Key Technical Details

**Packages**:
- `Microsoft.Data.SqlClient` 5.2.2 - SQL Server (secure, cross-platform)
- `Microsoft.Data.Sqlite` 9.0.0 - SQLite (cross-platform)
- `log4net` 2.0.15 - Logging
- `System.CommandLine` 2.0.0-beta4 - CLI parsing

**Security**:
- Source DB: Read-only via `ApplicationIntent=ReadOnly`
- Azure AD tokens: Auto-detected (>128 chars) and uses `SqlConnection.AccessToken`

**SQLite Fixes**:
- NULL handling: Uses `DBNull.Value` (not C# null)
- Foreign keys: Disabled during import via `PRAGMA foreign_keys = OFF`
- Path expansion: Tilde (~) expanded to home directory

---

## Summary

✅ **Cross-platform CLI tool** works on macOS/Linux/Windows  
✅ **Read-only source** database access guaranteed  
✅ **Centralized configuration** in Directory.Build.props  
✅ **Azure AD authentication** support via access tokens  
✅ **Modern packages** (Microsoft.Data.SqlClient, Microsoft.Data.Sqlite)  

### Quick Reference

```bash
# Run on macOS
./run-converter.sh --help

# Build solution
dotnet build

# Update package version
# Edit Directory.Packages.props

# Change .NET version
# Edit Directory.Build.props + 2 Windows Forms .csproj files
```

---

🎉 **Ready to use!** The solution is upgraded to .NET 9.0 with centralized configuration and works on macOS.

