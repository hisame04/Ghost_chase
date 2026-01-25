using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Rule_Scriptable")]
public class RuleData : ScriptableObject
{
    public int max_players;
    public float limitTime;
    public float waitStartTime;
}
