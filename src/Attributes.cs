namespace ZeroClaw.PluginSdk;

/// <summary>
/// Marks a static method as a ZeroClaw plugin tool.
/// The build process discovers methods with this attribute and
/// generates the plugin.toml manifest automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PluginFunctionAttribute : Attribute
{
    /// <summary>
    /// The exported function name (entry point in WASM).
    /// If not specified, uses the method name in snake_case.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Human-readable description of what the tool does.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Risk level: "low", "medium", or "high".
    /// </summary>
    public string RiskLevel { get; set; } = "low";

    /// <summary>
    /// JSON Schema for the tool's input parameters.
    /// Defaults to { "type": "object" } if not specified.
    /// </summary>
    public string ParametersSchema { get; set; } = """{"type":"object"}""";

    public PluginFunctionAttribute(string? name = null)
    {
        Name = name;
    }
}

/// <summary>
/// Specifies plugin-level metadata. Apply to the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ZeroClawPluginAttribute : Attribute
{
    /// <summary>
    /// Plugin name. Defaults to assembly name if not specified.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Plugin version. Defaults to assembly version if not specified.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Plugin description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Comma-separated list of capabilities (e.g., "tool,skill").
    /// </summary>
    public string Capabilities { get; set; } = "tool";

    /// <summary>
    /// Comma-separated list of permissions (e.g., "http_client,file_read").
    /// </summary>
    public string Permissions { get; set; } = "";

    /// <summary>
    /// Timeout in milliseconds for tool execution.
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Comma-separated list of allowed HTTP hosts.
    /// </summary>
    public string AllowedHosts { get; set; } = "";

    /// <summary>
    /// Filesystem allowed paths as "virtual=real,virtual2=real2" pairs.
    /// </summary>
    public string AllowedPaths { get; set; } = "";
}
