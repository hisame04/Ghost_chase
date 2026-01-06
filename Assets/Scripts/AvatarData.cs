using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Avatar_Scriptable")]

public class AvatarData : ScriptableObject
{
    public GameObject[] baseAvatars;
    public GameObject[] playerAvatars;
    public GameObject[] enemyAvatars;
    public Color[] baseColors;
    public Color[] emissionColors;
    public Color normalColor;
    public Color tagColor;
    public float[] emissionIntensity;
}
