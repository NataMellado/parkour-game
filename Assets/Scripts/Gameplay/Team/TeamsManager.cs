using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tbvl.GameManager.Gameplay;

// Clase para manejar los equipos del juego
public class TeamsManager : MonoBehaviour
{
    public static TeamsManager Instance { get; private set; }

    private Dictionary<Team, TeamProperties> teams;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarEquipos();
        }else
        {
            Destroy(gameObject);
        }
    }

    private void InicializarEquipos()
    {
        teams = new Dictionary<Team, TeamProperties>
        {
            {Team.SinEquipo, new TeamProperties("Sin Equipo", Color.white, "[?]")},
            {Team.Policias, new TeamProperties("Policias", Color.blue, "[P]")},
            {Team.Ladrones, new TeamProperties("Ladrones", Color.red, "[L]")}
        };
    }

    public TeamProperties GetTeamProperties(Team team)
    {
        return teams[team];
    }
}
