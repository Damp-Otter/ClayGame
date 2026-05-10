using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Data
{

    [CreateAssetMenu(menuName = "Data/CharacterSelectionData", fileName = "CharacterSelectionData")]

    public class CharacterSelectionData : ScriptableObject
    {

        public List<CharacterInfo> characters;

    }
}

[Serializable]
public struct CharacterInfo
{
    public string name;
    public GameObject character;
}