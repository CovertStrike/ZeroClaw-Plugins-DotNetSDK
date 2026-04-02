namespace ZeroClaw.PluginSdk;

/// <summary>
/// Marks a static method as a ZeroClaw plugin function/tool.
/// The source generator discovers methods with this attribute and
/// auto-generates the plugin.toml manifest.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ZeroClawFunctionAttribute : Attribute
{
    /// <summary>
    /// The tool name exposed to ZeroClaw. Required.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Human-readable description of what the tool does. Required.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Risk level: "low", "medium", or "high". Defaults to "low".
    /// </summary>
    public string RiskLevel { get; set; } = "low";

    public ZeroClawFunctionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Specifies the JSON Schema for a tool's input parameters.
/// Apply to the input type (record/class) used by the tool.
/// If not specified, schema is auto-generated from the type's properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ZeroClawSchemaAttribute : Attribute
{
    /// <summary>
    /// Raw JSON Schema string. If provided, overrides auto-generation.
    /// </summary>
    public string? Schema { get; set; }
}

/// <summary>
/// Provides additional metadata for a property in the JSON Schema.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class ZeroClawPropertyAttribute : Attribute
{
    /// <summary>
    /// Description of this property for the JSON Schema.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this property is required. Defaults to true for non-nullable types.
    /// </summary>
    public bool Required { get; set; } = true;
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

// Keep the old attribute name as an alias for backwards compatibility
/// <summary>
/// Alias for ZeroClawFunctionAttribute. Prefer using ZeroClawFunctionAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PluginFunctionAttribute : Attribute
{
    public string? Name { get; }
    public string Description { get; set; } = "";
    public string RiskLevel { get; set; } = "low";
    public string ParametersSchema { get; set; } = """{"type":"object"}""";

    public PluginFunctionAttribute(string? name = null)
    {
        Name = name;
    }
}
