using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tbvl.GameManager.Gameplay;

// Clase que se encarga de gestionar el equipo del jugador
public class PlayerTeam : MonoBehaviour
{
    public Teams.Team team;
    public Teams.Team enemyTeam;

    private void Start()
    {
        if (team == Teams.Team.Policias)
        {
            enemyTeam = Teams.Team.Ladrones;
        }
        else
        {
            enemyTeam = Teams.Team.Policias;
        }
    }
    public void SwitchToTeam(Teams.Team team)
    {
        if (this.team != team)
        {
            team = this.team;
            enemyTeam = (team == Teams.Team.Policias) ? Teams.Team.Ladrones : Teams.Team.Policias;
        }
    }

}
