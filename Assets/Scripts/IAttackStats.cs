using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackCount
{
    int strength { get; }
    int speed { get; }
    int attack { get; }
    int defense { get; }
    int knowledge { get; }
    int charisma { get; }
}
