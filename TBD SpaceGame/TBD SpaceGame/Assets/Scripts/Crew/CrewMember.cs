using System.Collections.Generic;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Data container describing a crew member.
    /// </summary>
    [System.Serializable]
    public class CrewMember
    {
        public string role;
        public int age;
        public List<string> traits = new();
        public float health = 100f;
        public float morale = 100f;
        public List<string> skills = new();
    }
}
