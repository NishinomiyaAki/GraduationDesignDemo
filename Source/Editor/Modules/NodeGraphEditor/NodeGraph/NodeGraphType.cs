using System;
using EditorUI;
using System.Collections.Generic;
using System.IO;

namespace CrossEditor
{
    public enum NodeGraphType
    {
        BasicNodeGraph,

        // For Animation Start
        AnimGraph,
        StateMachineGraph,
        StateGraph,
        TransitionRuleGraph,
        ParamFormulaGraph,
        // For Animation End

        MaterialGraph,
    }
}