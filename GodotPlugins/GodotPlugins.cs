using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MLG.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

// ReSharper disable once CheckNamespace
namespace GodotPlugins.Game;

/// <summary>
///     Godot generated class to load the game assembly.
///     Used here as an entry point for patching
/// </summary>
public static class Main
{
    private static SceneTree _sceneTree = null!;

    /// <summary>
    ///     Function called by the engine to load all the scripts and connect the Godot C# Bridge
    /// </summary>
    /// <param name="godotDllHandle">This seems to be the DllHandle given by the native C++ code</param>
    /// <param name="outManagedCallbacks"></param>
    /// <param name="unmanagedCallbacks"></param>
    /// <param name="unmanagedCallbacksSize"></param>
    [UnmanagedCallersOnly(EntryPoint = "godotsharp_game_main_init")]
    private static godot_bool InitializeFromGameProject(
        IntPtr godotDllHandle,
        IntPtr outManagedCallbacks,
        IntPtr unmanagedCallbacks,
        int unmanagedCallbacksSize
    )
    {
        try
        {
            // Get where the program is executed to get assembly around it
            var exeDir = AppContext.BaseDirectory;
            Console.WriteLine($"Living in {exeDir}");

            // Get DLL file name
            var dllName = Path.GetFileNameWithoutExtension(
                Assembly.GetExecutingAssembly().Location
            );
            // Apply the correct path
            var dllPath = Path.Combine(exeDir, $"{dllName}_original.dll"); // The original assembly
            var patchedDllPath = Path.Combine(exeDir, $"{dllName}_patched.dll"); // The patched one for reflection

            // Load MLG.Core.dll manually because Godot Engine can't do it for us
            AppDomain.CurrentDomain.AssemblyResolve += LoadDependencies;
            // var mlgCoreDllPath = Path.Combine(exeDir, "MLG", "Core", "MLG.Core.dll");
            // if (Path.Exists(mlgCoreDllPath))
            //     AssemblyLoadContext.Default.LoadFromAssemblyPath(mlgCoreDllPath);
            // else
            // {
            //     Console.WriteLine($"Failed to find MLG.Core.dll");
            //     throw new Exception("Could not find MLG.Core.dll");
            // }

            // var dllManager = new DllManager();
            // Console.WriteLine(DllManager.WorkingDirectory);
            // AppDomain.CurrentDomain.AssemblyResolve -= LoadDependencies;

            PatchInitializeFromGameProject(dllPath, patchedDllPath);

            // var context = new NoFallbackLoadContext(patchedDllPath);
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(patchedDllPath);

            // Console.WriteLine($"Using {self.FullName} from {self.Location}");
            Console.WriteLine($"Loaded {assembly.FullName} from {assembly.Location}");

            // Search the original GodotPlugin.Game::Main function
            var type = assembly.GetType("GodotPlugins.Game.Main");

            if (type == null)
            {
                Console.WriteLine("Type 'GodotPlugin.Game.Main' not found.");
                return godot_bool.False;
            }

            foreach (var m in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                Console.WriteLine($"Method '{m.Name}' of '{m.DeclaringType?.FullName}'.");

            // Search the original InitializeFromGameProject
            var method = type.GetMethod(
                "InitializeFromGameProject",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            if (method == null)
            {
                Console.WriteLine("Method 'InitializeFromGameProject' not found.");
                return godot_bool.False;
            }

            Console.WriteLine("Calling original Godot init...");
            // The assembly should be patched before the real InitializeFromGameProject
            // Call the original InitializeFromGameProject via reflection

            var returnedValue = (godot_bool)
                method.Invoke(
                    null,
                    [
                        godotDllHandle,
                        outManagedCallbacks,
                        unmanagedCallbacks,
                        unmanagedCallbacksSize,
                    ]
                )!;

            Console.WriteLine("GodotSharp Game Initialized");

            // PrintInitializationOfDotnet();

            return returnedValue;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Failed to initialize GodotSharp Game");
            Console.WriteLine(e);
            return godot_bool.False;
        }
    }

    private static void InjectNode()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        var root = tree.Root;

        var box = new CsgBox3D { Position = new Vector3(0, 0, 0) };

        root.CallDeferred("add_child", box);
    }

    private static void InstanceOnTreeEntered(Node node)
    {
        Console.WriteLine("MY INSTANCE IS ON SCENE TREE ??");
        Console.WriteLine(_sceneTree.Root.GetChildren());
        Console.WriteLine(node);
    }

    private static Assembly? LoadDependencies(object? _, ResolveEventArgs args)
    {
        var requestedAssembly = new AssemblyName(args.Name);
        Console.WriteLine($"[Loader] Resolving assembly {requestedAssembly.Name}");
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "MLG",
            "core",
            requestedAssembly.Name + ".dll"
        );

        if (File.Exists(path))
        {
            Console.WriteLine($"[Loader] Loading from {path}");
            return Assembly.LoadFrom(path);
        }

        return null;
    }

    /// <summary>
    ///     Remove the UnmanagedCallersOnly attribute of the original dll
    ///     to allow us to call it via reflexion
    /// </summary>
    /// <param name="originalDllPath">The original game assembly path</param>
    /// <param name="patchedDllPath">Where to save the patched assembly</param>
    private static void PatchInitializeFromGameProject(
        string originalDllPath,
        string patchedDllPath
    )
    {
        Console.WriteLine("Patching game project...");
        var assembly = AssemblyDefinition.ReadAssembly(originalDllPath);
        Console.WriteLine($"{assembly.FullName} found [{originalDllPath}]");

        var type = assembly.MainModule.GetType("GodotPlugins.Game.Main");
        var method = type.Methods.First(m => m.Name == "InitializeFromGameProject");

        // Update name
        assembly.Name.Name = $"{assembly.Name.Name}.Patched";

        // Remove UnmanagedCallersOnlyAttribute
        var attrToRemove = method.CustomAttributes.FirstOrDefault(a =>
            a.AttributeType.FullName
            == "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute"
        );

        // Reference to the hook method
        var hookAssembly = AssemblyDefinition.ReadAssembly(
            Path.Combine(
                AppContext.BaseDirectory,
                Path.GetFileName(Assembly.GetExecutingAssembly().Location) // Get the dll name
            )
        );
        var hookModule = hookAssembly.MainModule;
        var hookType = hookModule.Types.First(t => t.Name == "Main");
        var hookMethod = hookType.Methods.First(m => m.Name == "WaitForSceneTree");

        // Find existing call to ScriptManagerBridge.LookupScriptsInAssembly in original dll
        Instruction? targetInstruction = null;
        foreach (var instr in method.Body.Instructions)
            if (
                (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt)
                && instr.Operand is MethodReference { Name: "LookupScriptsInAssembly" }
            )
            {
                targetInstruction = instr;
                break;
            }

        if (targetInstruction == null)
        {
            Console.WriteLine("Call to LookupScriptsInAssembly not found!");
            return;
        }

        // Import references needed:
        _ = assembly.MainModule.ImportReference(typeof(Type));
        var getTypeFromHandle = assembly.MainModule.ImportReference(
            typeof(Type).GetMethod("GetTypeFromHandle", [typeof(RuntimeTypeHandle)])
        );
        _ = assembly.MainModule.ImportReference(typeof(Assembly));
        var getAssemblyMethod = assembly.MainModule.ImportReference(
            typeof(Type).GetProperty("Assembly")?.GetGetMethod()
        );

        var bootstrapType = assembly.MainModule.ImportReference(typeof(MLG.Bootstrap.Bootstrap));

        // The method ref for ScriptManagerBridge.LookupScriptsInAssembly
        // We assume it's the same method as in the targetInstruction
        var lookupMethod = (MethodReference)targetInstruction.Operand;

        var il = method.Body.GetILProcessor();
        // Create instructions
        var instructionsToInsert = new[]
        {
            il.Create(OpCodes.Ldtoken, bootstrapType), // ldtoken MLG.Bootstrap.MLG.Bootstrap
            il.Create(OpCodes.Call, getTypeFromHandle), // call Type.GetTypeFromHandle
            il.Create(OpCodes.Callvirt, getAssemblyMethod), // callvirt get_Assembly
            il.Create(OpCodes.Call, lookupMethod), // call ScriptManagerBridge.LookupScriptsInAssembly
        };

        // Insert instructions after the existing call
        var insertAfter = targetInstruction;
        foreach (var newInstr in instructionsToInsert)
        {
            il.InsertAfter(insertAfter, newInstr);
            insertAfter = newInstr;
        }

        // Import the method reference into the target module
        var hookMethodRef = assembly.MainModule.ImportReference(hookMethod);

        var retInstructions = method
            .Body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret)
            .ToList();

        // Inject call before every return
        foreach (var ret in retInstructions)
        {
            il.InsertBefore(ret, il.Create(OpCodes.Ldarg_0)); // IntPtr godotDllHandle
            il.InsertBefore(ret, il.Create(OpCodes.Ldarg_1)); // IntPtr outManagedCallbacks
            il.InsertBefore(ret, il.Create(OpCodes.Ldarg_2)); // IntPtr unmanagedCallbacks
            il.InsertBefore(ret, il.Create(OpCodes.Ldarg_3)); // int unmanagedCallbacksSize
            il.InsertBefore(ret, il.Create(OpCodes.Call, hookMethodRef));
        }

        if (attrToRemove != null)
            method.CustomAttributes.Remove(attrToRemove);

        assembly.Write(patchedDllPath);
        Console.WriteLine($"Done patching game project..., wrote to {patchedDllPath}");
    }

    private static async Task PrintInitializationOfDotnet()
    {
        var nativeFuncs = Type.GetType("Godot.NativeInterop.NativeFuncs, GodotSharp");
        var dotnetModuleIsInitializedMethod = nativeFuncs?.GetMethod(
            "godotsharp_dotnet_module_is_initialized",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var isInitialized = (godot_bool)(
            dotnetModuleIsInitializedMethod?.Invoke(null, null)
            ?? throw new NullReferenceException()
        );
        Console.WriteLine($"The dotnet state is: {isInitialized}");

        while (isInitialized.CompareTo(godot_bool.True) != 0)
        {
            isInitialized = (godot_bool)(
                dotnetModuleIsInitializedMethod.Invoke(null, null)
                ?? throw new NullReferenceException()
            );
            Console.WriteLine($"The dotnet state is: {isInitialized}");
        }

        while (true)
        {
            var mainLoop = Engine.GetMainLoop();
            Console.WriteLine($"Main loop: {mainLoop}");

            // Only continue if SceneTree is active (not just a Window)
            if (mainLoop is SceneTree { Root: not null } tree)
            {
                Console.WriteLine("SceneTree ready, injecting autoload...");
                _sceneTree = tree;
                // InjectAutoload(tree);

                var treeRoot = tree.Root;

                var box = new CsgBox3D { Position = new Vector3(5, 2, 0) };
                // var boot = new MLG.Bootstrap();
                // treeRoot.AddChild(box);
                treeRoot.CallDeferred("add_child", box);
                Console.WriteLine("Getting the bootstrapper");

                var success = ProjectSettings.LoadResourcePack("user://PluginLoader.pck");
                Console.WriteLine(
                    success
                        ? "[Modot] Plugin loaded successfully !"
                        : "[Modot] Plugin loading failed !"
                );

                // var dir = ResourceLoader.ListDirectory("res://");
                // foreach (var s in dir)
                //     Console.WriteLine(s);

                Console.WriteLine("[Modot] Calling ResourceLoader...");
                var userDir = OS.GetUserDataDir();
                Console.WriteLine($"[Modot] User directory: {userDir}");
                try
                {
                    var pluginLoaderAssembly = Assembly.LoadFile(
                        Path.Combine(userDir, "plugin-loader.dll")
                    );
                    Console.WriteLine(
                        $"[Modot] Plugin loader assembly: {pluginLoaderAssembly.FullName}"
                    );
                    ScriptManagerBridge.LookupScriptsInAssembly(pluginLoaderAssembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                var pluginLoaderScene = ResourceLoader
                    .Load<PackedScene>("res://plugin_loader.tscn")
                    .Instantiate();
                Console.WriteLine($"[Modot] Loading scene: {pluginLoaderScene}");
                // treeRoot.CallDeferred("add_child", pluginLoaderScene);
                treeRoot.AddChild(pluginLoaderScene);
                Console.WriteLine("[Modot] Loaded the scene !");

                Console.WriteLine($"Getting the instance: {pluginLoaderScene}");
                pluginLoaderScene.TreeEntered += () => InstanceOnTreeEntered(pluginLoaderScene);
                Console.WriteLine("Adding child to scene");
                break;
            }

            await Task.Delay(100);
        }

        // GD.Print("Hello from godot");
        // Console.WriteLine(box.GetInstanceId());
        //
        // // Load the external assembly (must be already loaded in AppDomain or load it manually)
        // // var assembly = Assembly.LoadFrom("res://MyExternalAssembly.dll"); // or absolute path
        //
        // // Get the type of the script you want to instantiate
        //

        //
        // // Create instance of the type
        //
        // // Add to scene
        // var myTree = (SceneTree)Engine.GetMainLoop();
        // // myTree.AddChild(instance);
        // GD.Print(treeRoot.GetChildren());
        // GD.Print(myTree.GetRoot().GetTree());
        // GD.Print("MLG.Bootstrap is ok");
    }

    public static void WaitForSceneTree(
        IntPtr dllHandle,
        IntPtr outCallbacks,
        IntPtr unmanagedCallbacks,
        int size
    )
    {
        Console.WriteLine("Waiting for scene tree...");
        GD.Print("test");
        Task.Run(PrintInitializationOfDotnet);
    }
}
