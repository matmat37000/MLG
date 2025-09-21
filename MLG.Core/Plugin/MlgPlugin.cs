using Godot;

namespace MLG.Core;

[AttributeUsage(AttributeTargets.Class)]
public class MlgPlugin(string name, string id, string version) : Attribute
{
    public string Name { get; } = name;
    public string Id { get; } = id;
    public string Version { get; } = version;
}
