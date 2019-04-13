using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides logic gates that act as a trigger and take two other triggers as inputs, or only one input if
/// the gate has the Not type
/// </summary>
public class LogicGateTrigger : TriggerObject {

    [Tooltip("The type of this logic gate")]
    public LogicGateType type;
    [Tooltip("The first input trigger")]
    public TriggerObject inputA;
    [Tooltip("The second input trigger")]
    public TriggerObject inputB;

    /// <summary>
    /// Returns the result of running the inputs through the logic gate
    /// </summary>
    /// <value>The result of the logic gate</value>
    public override bool Active {
        get {
            if (!inputA || (!inputB && type != LogicGateType.Not))
                return false;
            switch (type) {
                case LogicGateType.Not:
                    return !inputA.Active;
                case LogicGateType.And:
                    return inputA.Active && inputB.Active;
                case LogicGateType.Or:
                    return inputA.Active || inputB.Active;
                case LogicGateType.Xor:
                    return inputA.Active != inputB.Active;
                case LogicGateType.Nand:
                    return !(inputA.Active && inputB.Active);
                case LogicGateType.Nor:
                    return !(inputA.Active || inputB.Active);
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.color = Active? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    /// <summary>
    /// Types of logic gates
    /// </summary>
    public enum LogicGateType {
        Not,
        And,
        Or,
        Xor,
        Nand,
        Nor
    }
}