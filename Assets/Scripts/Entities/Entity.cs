using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : GravityBody
{
    [Header("Movement")]
    [SerializeField] protected float groundMovementSpeed = 5f;
    [SerializeField] protected float jumpHeight = 1f;

    [Header("Stats")]
    [SerializeField] private int oxygen = 100;
    [SerializeField] private int health = 100;
    [SerializeField] private int hunger = 100;
    [SerializeField] private int thirst = 100;

    protected Vector3 forwardDir = Vector3.forward;
    protected Vector3 rightDir = Vector3.right;
}
