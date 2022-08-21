using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    public static class UnityHelper
    {
        public static void NextJumpAlways(this List<CodeInstruction> code, ref int index)
        {
            for (; index < code.Count; index++)
            {
                var line = code[index];

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                UnityEngine.Debug.Log($"Transpiler NextJumpAlways {line.opcode} @{index}");

                if (line.opcode.OperandType == OperandType.InlineBrTarget)
                    code.Insert(++index, new CodeInstruction(OpCodes.Br, line.operand));

                else if (line.opcode.OperandType == OperandType.ShortInlineBrTarget)
                    code.Insert(++index, new CodeInstruction(OpCodes.Br_S, line.operand));

                else
                    throw new Exception("Did not expect this OpCode");

                return;
            }
        }

        public static void NextJumpNever(this List<CodeInstruction> code, ref int index)
        {
            for (; index < code.Count; index++)
            {
                var line = code[index];

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                UnityEngine.Debug.Log($"Transpiler NextJumpNever {line.opcode} @{index}");

                if (line.opcode.StackBehaviourPush != StackBehaviour.Push0)
                    throw new Exception("Cond_Branch should not push onto stack");

                var num = line.opcode.StackBehaviourPop.GetStackChange();
                if (num == 0)
                {
                    line.opcode = OpCodes.Nop;
                    line.operand = null;
                }
                else if (num == -1)
                {
                    line.opcode = OpCodes.Pop;
                    line.operand = null;
                }
                else if (num == -2)
                {
                    line.opcode = OpCodes.Pop;
                    line.operand = null;
                    code.Insert(index++, new CodeInstruction(OpCodes.Pop));
                }
                else
                    throw new Exception("Cond_Branch should not pop more than 2");

                return;
            }
        }

        public static int GetStackChange(this StackBehaviour stack)
        {
            switch (stack)
            {
                case StackBehaviour.Pop0:
                case StackBehaviour.Push0:
                    return 0;
                case StackBehaviour.Pop1:
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Varpop:
                    return -1;
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                case StackBehaviour.Varpush:
                    return 1;
                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return -2;
                case StackBehaviour.Push1_push1:
                    return 2;
                case StackBehaviour.Popref_popi_pop1:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                case StackBehaviour.Popi_popi_popi:
                    return -3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
