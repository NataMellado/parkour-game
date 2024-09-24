using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using System;
using Internal.ReadLine;

public class ServerConsoleInput : MonoBehaviour
{

    private Thread consoleThread;
    private bool isRunning;
    private ConcurrentQueue<string> commandQueue = new ConcurrentQueue<string>();

    private void Start()
    {
        if (Application.isBatchMode)
        {
            // Inicializar ReadLine
            ReadLine.HistoryEnabled = true;

            isRunning = true;
            consoleThread = new Thread(new ThreadStart(ConsoleInputThread));
            consoleThread.Start();
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (consoleThread != null && consoleThread.IsAlive)
            consoleThread.Abort();
    }

    private void ConsoleInputThread()
    {
        while (isRunning)
        {
            string input = ReadLine.Read("[Servidor] >> ");
            if (input != null)
            {
                commandQueue.Enqueue(input);
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    private void Update()
    {
        if (!commandQueue.IsEmpty)
        {
            while (commandQueue.TryDequeue(out string command))
            {
                ProcessCommand(command);
            }
        }
    }

    private void ProcessCommand(string command)
    {
        switch (command.ToLower())
        {
            case "cmd_equipos_alternar":
                ServerManager.Instance.AlternarEquipos();
                break;
            default:
                Debug.Log($"Comando desconocido: {command}");
                break;
        }
    }

}
