using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Player;
public class BuildScript
{
    [MenuItem("Herramientas/Compilar Todo")]
    public static void BuildAll()
    {
        BuildLinuxServer();
        BuildWindowsClient();
    }

    public static void BuildWindowsClient()
    {
        // Guardar los símbolos de compilación actuales
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

        // Remover el símbolo DEDICATED_SERVER
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, RemoveDefineSymbol(currentDefines, "DEDICATED_SERVER"));

        // Configurar opciones de compilación para el cliente de Windows
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = "Builds/latest/ParkourGame.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        // Compilar el cliente de Windows
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Restaurar los símbolos de compilación originales
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, currentDefines);
    }

    public static void BuildLinuxServer()
    {
        // Guardar las configuraciones actuales
        var currentTarget = EditorUserBuildSettings.activeBuildTarget;
        var currentGroup = BuildPipeline.GetBuildTargetGroup(currentTarget);
        var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup);
        var currentScriptingBackend = PlayerSettings.GetScriptingBackend(currentGroup);

        // Agregar el símbolo DEDICATED_SERVER
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, AddDefineSymbol(currentDefines, "DEDICATED_SERVER"));

        // Establecer el backend de scripting para Linux
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x); // O IL2CPP si lo prefieres

        // Configurar opciones de compilación para el servidor de Linux
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = "_ServerBuilds/serverBuildLatest/serverBuild.x86_64";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server; // Especificar que es un servidor
        buildPlayerOptions.options = BuildOptions.None; // No es necesario usar BuildOptions.EnableHeadlessMode

        // Compilar el servidor de Linux
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Restaurar las configuraciones originales
        PlayerSettings.SetScriptingDefineSymbolsForGroup(currentGroup, currentDefines);
        PlayerSettings.SetScriptingBackend(currentGroup, currentScriptingBackend);
    }


    private static string[] GetScenePaths()
    {
        // Obtener todas las escenas habilitadas en Build Settings
        return EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
    }

    // Métodos auxiliares para manejar símbolos de compilación
    private static string AddDefineSymbol(string defines, string symbol)
    {
        var symbols = defines.Split(';').ToList();
        if (!symbols.Contains(symbol))
        {
            symbols.Add(symbol);
        }
        return string.Join(";", symbols);
    }

    private static string RemoveDefineSymbol(string defines, string symbol)
    {
        var symbols = defines.Split(';').ToList();
        if (symbols.Contains(symbol))
        {
            symbols.Remove(symbol);
        }
        return string.Join(";", symbols);
    }
}
