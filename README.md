# ZeroClaw.PluginSdk

A simple .NET SDK for building ZeroClaw WASM plugins. Just add the NuGet package and start writing plugins - the SDK handles WASM compilation and automatically generates `plugin.toml` manifests.

## Quick Start

### 1. Install Prerequisites

```bash
# Install .NET 8 SDK from https://dotnet.microsoft.com/download

# Install the WASI workload (required for WASM compilation)
dotnet workload install wasi-experimental
```

### 2. Create a New Plugin Project

```bash
dotnet new console -n MyPlugin
cd MyPlugin
```

### 3. Add the SDK

```bash
dotnet add package ZeroClaw.PluginSdk
```

### 4. Configure Your Project

Edit your `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>wasi-wasm</RuntimeIdentifier>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <PublishTrimmed>true</PublishTrimmed>
  </PropertyGroup>

  <!-- Plugin metadata -->
  <PropertyGroup>
    <ZeroClawPluginName>my-plugin</ZeroClawPluginName>
    <ZeroClawPluginVersion>1.0.0</ZeroClawPluginVersion>
    <ZeroClawPluginDescription>My awesome plugin</ZeroClawPluginDescription>
    <ZeroClawCapabilities>tool</ZeroClawCapabilities>
    <ZeroClawPermissions>http_client</ZeroClawPermissions>
    <ZeroClawTimeoutMs>5000</ZeroClawTimeoutMs>
    <ZeroClawAllowedHosts>api.example.com</ZeroClawAllowedHosts>
  </PropertyGroup>

  <!-- Define your tools -->
  <ItemGroup>
    <ZeroClawTool Include="greet"
                  Description="Greet a user by name"
                  Export="greet"
                  RiskLevel="low"
                  ParametersSchema='{ "type": "object", "properties": { "name": { "type": "string" } }, "required": ["name"] }' />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="ZeroClaw.PluginSdk" Version="0.1.*" />
  </ItemGroup>
</Project>
```

### 5. Write Your Plugin

```csharp
using System.Runtime.InteropServices;
using Extism;
using ZeroClaw.PluginSdk;

public record GreetInput(string Name);
public record GreetOutput(string Message);

public static class MyPlugin
{
    // Required entry point for WASI
    public static void Main() { }

    [PluginFunction("greet", Description = "Greet a user")]
    [UnmanagedCallersOnly(EntryPoint = "greet")]
    public static int Greet()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
            new GreetOutput($"Hello, {input.Name}!"));
    }
}
```

### 6. Build & Publish

```bash
# Build (generates plugin.toml)
dotnet build -c Release

# Publish (generates .wasm file + copies plugin.toml)
dotnet publish -c Release
```

Your output will be in `bin/Release/net8.0/wasi-wasm/publish/`:
- `MyPlugin.wasm` - The compiled WASM module
- `plugin.toml` - The plugin manifest (auto-generated)

## Plugin Configuration

### Plugin Properties

Set these in your `.csproj` `<PropertyGroup>`:

| Property | Description | Default |
|----------|-------------|---------|
| `ZeroClawPluginName` | Plugin name | Assembly name |
| `ZeroClawPluginVersion` | Plugin version | `1.0.0` |
| `ZeroClawPluginDescription` | Description | `"A ZeroClaw plugin"` |
| `ZeroClawCapabilities` | Comma-separated capabilities | `"tool"` |
| `ZeroClawPermissions` | Comma-separated permissions | (none) |
| `ZeroClawTimeoutMs` | Execution timeout in ms | `30000` |
| `ZeroClawAllowedHosts` | Comma-separated allowed HTTP hosts | (none) |
| `ZeroClawAllowedPaths` | Filesystem paths as `virtual=real` pairs | (none) |
| `ZeroClawWasmPath` | WASM filename in plugin.toml | `$(AssemblyName).wasm` |
| `ZeroClawGenerateManifest` | Enable/disable plugin.toml generation | `true` |

### Tool Definitions

Define tools with `<ZeroClawTool>` items:

```xml
<ItemGroup>
  <ZeroClawTool Include="my_tool"
                Description="What this tool does"
                Export="my_tool"
                RiskLevel="low"
                ParametersSchema='{ "type": "object" }' />
</ItemGroup>
```

| Attribute | Description |
|-----------|-------------|
| `Include` | Tool name |
| `Description` | Human-readable description |
| `Export` | WASM export name (entry point) |
| `RiskLevel` | `"low"`, `"medium"`, or `"high"` |
| `ParametersSchema` | JSON Schema for input parameters |

## SDK APIs

### Memory API

Store and recall data from the agent's memory:

```csharp
// Store a value
Memory.Store("my_key", "my_value");

// Recall memories matching a query
string results = Memory.Recall("my_key");

// Delete a memory
Memory.Forget("my_key");
```

### Messaging API

Send messages through configured channels:

```csharp
// Get available channels
List<string> channels = Messaging.GetChannels();

// Send a message
Messaging.Send("telegram", "user123", "Hello from my plugin!");
```

### Tools API

Call other registered tools:

```csharp
// Call another tool
string result = Tools.ToolCall("search", new { query = "hello" });
```

### Entry Point Helpers

The SDK handles JSON serialization automatically:

```csharp
// For functions with input and output
PluginEntryPoint.Invoke<TInput, TOutput>(input => { ... });

// For functions with input only
PluginEntryPoint.Invoke<TInput>(input => { ... });

// For functions with output only
PluginEntryPoint.InvokeNoInput<TOutput>(() => { ... });
```

## Complete Example

```csharp
using System.Runtime.InteropServices;
using Extism;
using ZeroClaw.PluginSdk;

public record SearchInput(string Query, int MaxResults);
public record SearchOutput(List<string> Results, int Total);

public static class SearchPlugin
{
    public static void Main() { }

    [PluginFunction("search")]
    [UnmanagedCallersOnly(EntryPoint = "search")]
    public static int Search()
    {
        return PluginEntryPoint.Invoke<SearchInput, SearchOutput>(input =>
        {
            // Your search logic here
            var results = new List<string> { "result1", "result2" };
            
            // Optionally store in memory
            Memory.Store("last_query", input.Query);
            
            // Optionally notify
            Messaging.Send("slack", "search-channel", 
                $"Search performed: {input.Query}");
            
            return new SearchOutput(results, results.Count);
        });
    }
}
```

## Generated plugin.toml

The SDK automatically generates a `plugin.toml` like this:

```toml
[plugin]
name = "my-plugin"
version = "1.0.0"
description = "My awesome plugin"
wasm_path = "MyPlugin.wasm"
capabilities = ["tool"]
permissions = ["http_client"]
timeout_ms = 5000
allowed_hosts = ["api.example.com"]

[[tools]]
name = "greet"
description = "Greet a user by name"
export = "greet"
risk_level = "low"
parameters_schema = { "type": "object", "properties": { "name": { "type": "string" } }, "required": ["name"] }
```

## JSON Serialization

The SDK uses `snake_case` JSON naming to match ZeroClaw's wire format:

- C# `MyProperty` becomes JSON `my_property`
- C# records work great: `record Input(string UserName)` becomes `{"user_name": "..."}`

## Error Handling

Exceptions thrown in your handler are automatically caught and returned as error responses:

```csharp
PluginEntryPoint.Invoke<Input, Output>(input =>
{
    if (string.IsNullOrEmpty(input.Name))
        throw new ArgumentException("Name is required");
    
    return new Output(...);
});
// Returns: {"error": "Name is required", "success": false}
```

For explicit error handling with host functions:

```csharp
try
{
    Memory.Store("key", "value");
}
catch (PluginException ex)
{
    // Handle host function errors
}
```

## Building from Source

```bash
# Clone the repo
git clone https://github.com/Biztactix-Ryan/zeroclaw-dotnetsdk.git
cd zeroclaw-dotnetsdk

# Build
dotnet build

# Run tests
dotnet test

# Build the sample plugin (requires wasi-experimental workload)
cd samples/HelloPlugin
dotnet publish -c Release
```

## License

MIT License - see LICENSE file for details.
