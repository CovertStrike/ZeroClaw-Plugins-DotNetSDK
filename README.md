# ZeroClaw.PluginSdk

A simple .NET SDK for building ZeroClaw WASM plugins. Just add the NuGet package, decorate your functions with `[ZeroClawFunction]`, and the SDK automatically generates `plugin.toml` manifests.

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

  <ItemGroup>
    <PackageReference Include="ZeroClaw.PluginSdk" Version="0.2.*" />
  </ItemGroup>
</Project>
```

### 5. Write Your Plugin

Just decorate your functions with `[ZeroClawFunction]` - tools are auto-discovered!

```csharp
using System.Runtime.InteropServices;
using Extism;
using ZeroClaw.PluginSdk;

public record GreetInput(string Name);
public record GreetOutput(string Message);

public static class MyPlugin
{
    public static void Main() { }

    [ZeroClawFunction("greet", "Greet a user by name")]
    [UnmanagedCallersOnly(EntryPoint = "greet")]
    public static int Greet()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
            new GreetOutput($"Hello, {input.Name}!"));
    }

    [ZeroClawFunction("add", "Add two numbers", RiskLevel = "low")]
    [UnmanagedCallersOnly(EntryPoint = "add")]
    public static int Add()
    {
        return PluginEntryPoint.Invoke<AddInput, AddOutput>(input =>
            new AddOutput(input.A + input.B));
    }
}
```

### 6. Build & Publish

```bash
# Build (generates plugin.toml with auto-discovered tools)
dotnet build -c Release

# Publish (generates .wasm file + copies plugin.toml)
dotnet publish -c Release
```

Your output will be in `bin/Release/net8.0/wasi-wasm/publish/`:
- `MyPlugin.wasm` - The compiled WASM module
- `plugin.toml` - The plugin manifest (auto-generated from attributes!)

## Auto-Discovery with [ZeroClawFunction]

The SDK automatically scans your source files for `[ZeroClawFunction]` attributes and generates the tools section of `plugin.toml`:

```csharp
// This attribute is all you need - no manual .csproj configuration required!
[ZeroClawFunction("tool_name", "Description of what the tool does")]
[UnmanagedCallersOnly(EntryPoint = "tool_name")]
public static int MyTool() { ... }

// With optional risk level
[ZeroClawFunction("dangerous_tool", "Does something risky", RiskLevel = "high")]
[UnmanagedCallersOnly(EntryPoint = "dangerous_tool")]
public static int DangerousTool() { ... }
```

**Attribute Parameters:**
- `name` (required): The tool name exposed to ZeroClaw
- `description` (required): Human-readable description
- `RiskLevel` (optional): `"low"` (default), `"medium"`, or `"high"`

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
| `ZeroClawWasmPath` | WASM filename in plugin.toml | `$(AssemblyName).wasm` |
| `ZeroClawGenerateManifest` | Enable/disable plugin.toml generation | `true` |

## SDK APIs

### Memory API

Store and recall data from the agent's memory:

```csharp
Memory.Store("my_key", "my_value");
string results = Memory.Recall("my_key");
Memory.Forget("my_key");
```

### Messaging API

Send messages through configured channels:

```csharp
List<string> channels = Messaging.GetChannels();
Messaging.Send("telegram", "user123", "Hello from my plugin!");
```

### Tools API

Call other registered tools:

```csharp
string result = Tools.ToolCall("search", new { query = "hello" });
```

### Entry Point Helpers

```csharp
// With input and output
PluginEntryPoint.Invoke<TInput, TOutput>(input => { ... });

// Input only
PluginEntryPoint.Invoke<TInput>(input => { ... });

// Output only
PluginEntryPoint.InvokeNoInput<TOutput>(() => { ... });
```

## Generated plugin.toml

The SDK generates a manifest like this automatically:

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
parameters_schema = { "type": "object" }

[[tools]]
name = "add"
description = "Add two numbers"
export = "add"
risk_level = "low"
parameters_schema = { "type": "object" }
```

## JSON Serialization

The SDK uses `snake_case` JSON naming:
- C# `MyProperty` becomes JSON `my_property`
- Records work great: `record Input(string UserName)` becomes `{"user_name": "..."}`

## Error Handling

Exceptions are automatically caught and returned as error responses:

```csharp
PluginEntryPoint.Invoke<Input, Output>(input =>
{
    if (string.IsNullOrEmpty(input.Name))
        throw new ArgumentException("Name is required");
    return new Output(...);
});
// Returns: {"error": "Name is required", "success": false}
```

## Building from Source

```bash
git clone https://github.com/Biztactix-Ryan/zeroclaw-dotnetsdk.git
cd zeroclaw-dotnetsdk

dotnet build
dotnet test

# Build sample plugin (requires wasi-experimental workload)
cd samples/HelloPlugin
dotnet build -c Release
```

## License

MIT License - see LICENSE file for details.
