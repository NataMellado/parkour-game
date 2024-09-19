using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase para definir las propiedades de los equipos
namespace Tbvl.GameManager.Gameplay
{
    public enum Team
    {
        SinEquipo,
        Policias,
        Ladrones
    }
    public class TeamProperties
    {
        public string TeamName { get; private set; }
        public Color TeamColor { get; private set; }
        public string TeamPrefix { get; private set; }

        public TeamProperties(string teamName, Color teamColor, string teamPrefix)
        {
            TeamName = teamName;
            TeamColor = teamColor;
            TeamPrefix = teamPrefix;
        }
    }
}
