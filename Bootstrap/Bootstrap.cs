using Godot;

[assembly: AssemblyHasScripts(new[] { typeof(Bootstrap.Bootstrap) })]

namespace Bootstrap;

// ReSharper disable once PartialTypeWithSinglePart
public partial class Bootstrap : Node
{
    public override void _EnterTree()
    {
        base._EnterTree();

        GD.Print("Godot is ready. Now it's safe to use Engine API.");

        var mainLoop = Engine.GetMainLoop();
        GD.Print("Main loop: " + mainLoop);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        GD.Print("Main loop finished.");
    }

    public SceneTree GetSceneTree()
    {
        return GetTree();
    }
}