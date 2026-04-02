using System.Runtime.InteropServices;
using Extism;
using ZeroClaw.PluginSdk;

namespace HelloPlugin;

// Input/output records for the greet function
public record GreetInput(string Name);
public record GreetOutput(string Message);

// Input/output records for the add function
public record AddInput(int A, int B);
public record AddOutput(int Result);

public static class Plugin
{
    // Required entry point for WASI builds - actual exports use UnmanagedCallersOnly
    public static void Main() { }

    /// <summary>
    /// A simple greeting function that takes a name and returns a greeting message.
    /// </summary>
    [PluginFunction("greet")]
    [UnmanagedCallersOnly(EntryPoint = "greet")]
    public static int Greet()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
            new GreetOutput($"Hello, {input.Name}! Welcome to ZeroClaw."));
    }

    /// <summary>
    /// A simple add function that demonstrates numeric operations.
    /// </summary>
    [PluginFunction("add")]
    [UnmanagedCallersOnly(EntryPoint = "add")]
    public static int Add()
    {
        return PluginEntryPoint.Invoke<AddInput, AddOutput>(input =>
            new AddOutput(input.A + input.B));
    }

    /// <summary>
    /// Demonstrates using the Memory API to store and recall data.
    /// </summary>
    [PluginFunction("remember")]
    [UnmanagedCallersOnly(EntryPoint = "remember")]
    public static int Remember()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
        {
            // Store something in memory
            Memory.Store("last_visitor", input.Name);

            // Recall it back
            var recalled = Memory.Recall("last_visitor");

            return new GreetOutput($"Stored and recalled: {recalled}");
        });
    }

    /// <summary>
    /// Demonstrates using the Messaging API.
    /// </summary>
    [PluginFunction("notify")]
    [UnmanagedCallersOnly(EntryPoint = "notify")]
    public static int Notify()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
        {
            // Get available channels
            var channels = Messaging.GetChannels();

            if (channels.Count > 0)
            {
                // Send a message on the first available channel
                Messaging.Send(channels[0], "admin", $"User {input.Name} connected!");
            }

            return new GreetOutput($"Notification sent for {input.Name}");
        });
    }

    /// <summary>
    /// Demonstrates calling other tools from within a plugin.
    /// </summary>
    [PluginFunction("search_and_greet")]
    [UnmanagedCallersOnly(EntryPoint = "search_and_greet")]
    public static int SearchAndGreet()
    {
        return PluginEntryPoint.Invoke<GreetInput, GreetOutput>(input =>
        {
            // Call another tool
            var searchResult = Tools.ToolCall("search", new { query = input.Name });

            return new GreetOutput($"Found info about {input.Name}: {searchResult}");
        });
    }
}
