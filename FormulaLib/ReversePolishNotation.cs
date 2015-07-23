using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaLib
{
    internal class Functions
    {
        public List<Functions> FunctionList = new List<Functions>();
        public List<List<Lexem>> operands = new List<List<Lexem>>();
        public Functions(Queue<Lexem> queue)
        {
            Queue<Lexem> lexems = new Queue<Lexem>();
            Lexem currLextm = queue.Dequeue();
            int brackets = 1;
            while (queue.Count > 0 && brackets > 0)
            {
                currLextm = queue.Dequeue();
                if (currLextm.Type == LexemType.Identifier)
                {
                    currLextm.Value = currLextm.Value + "{" + FunctionList.Count.ToString() + "}";
                    lexems.Enqueue(currLextm);
                    FunctionList.Add(new Functions(queue));
                }
                else
                {
                    if (currLextm.Type == LexemType.LBracket)
                    {
                        brackets++;
                    }
                    if (currLextm.Type == LexemType.RBracket)
                    {
                        brackets--;
                    }
                    lexems.Enqueue(currLextm);
                }
            }
            List<Lexem> operand = new List<Lexem>();
            while (lexems.Count > 0)
            {
                currLextm = lexems.Dequeue();
                if (currLextm.Type == LexemType.LBracket) { brackets++; }
                if (currLextm.Type == LexemType.RBracket) { brackets--; }
                if ((currLextm.Type == LexemType.Semicolon)&&(brackets==0))
                {
                    operands.Add(operand);
                    operand = new List<Lexem>();
                }
                else
                {
                    operand.Add(currLextm);
                }
            }
            operand.Remove(operand.Last());
            operands.Add(operand);
        }
    }
    public class PostfixNotationExpression
    {
        private List<Functions> FunctionList = new List<Functions>();
        private List<List<Lexem>> operands = new List<List<Lexem>>();

        private List<LexemType> operators = new List<LexemType>()
        { 
            LexemType.Multiply, LexemType.Divide, LexemType.Plus, LexemType.Minus, LexemType.UnarMinus, 
            LexemType.And, LexemType.Or, LexemType.Equals, LexemType.NotEquals, LexemType.Greater, LexemType.Lower, LexemType.GreaterEquals, LexemType.LowerEquals,
            LexemType.LBracket, LexemType.RBracket
        };
        
        public PostfixNotationExpression(String formula, Dictionary<String, Double> variables)
        {
            variables.Add("PI", Math.PI);
            variables.Add("E", Math.E);
            Queue<Lexem> queue = new Queue<Lexem>(new Lexer(formula, variables).Lexems);
            Lexem currLextm = null;
            Queue<Lexem> lexems = new Queue<Lexem>();
            while(queue.Count>0)
            {
                currLextm = queue.Dequeue();
                if (currLextm.Type == LexemType.Identifier)
                {
                    currLextm.Value = currLextm.Value + "{" + FunctionList.Count.ToString() + "}";
                    lexems.Enqueue(currLextm);
                    FunctionList.Add(new Functions(queue));
                }
                else
                {
                    lexems.Enqueue(currLextm);
                }
            }
            List<Lexem> operand = new List<Lexem>();
            Int32 brackets = 0;
            while (lexems.Count > 0)
            {
                currLextm = lexems.Dequeue();
                if (currLextm.Type == LexemType.LBracket) { brackets++; }
                if (currLextm.Type == LexemType.RBracket) { brackets--; }
                if ((currLextm.Type == LexemType.Semicolon) && (brackets == 0))
                {
                    operands.Add(operand);
                    operand = new List<Lexem>();
                }
                else
                {
                    operand.Add(currLextm);
                }
            }
            operands.Add(operand);
        }

        public PostfixNotationExpression()
        {
            // TODO: Complete member initialization
        }

        public String Calc()
        {
            return Calc(FunctionList, operands).First();
        }
        private List<String> Calc(List<Functions> fList, List<List<Lexem>> lOps)
        {
            List<String> res = new List<String>();
            if (fList.Count==0)
            {
                foreach (List<Lexem> op in lOps)
                {
                    String resOp = getResult(op);
                    res.Add(resOp);
                }
            }
            else
            {
                foreach (List<Lexem> op in lOps)
                {
                    for (int i=0;i<op.Count;i++)
                    {
                        if (op[i].Type == LexemType.Identifier)
                        {
                            String fName = op[i].Value.Remove(op[i].Value.IndexOf("{"));
                            String fNumStr = op[i].Value.Remove(0, op[i].Value.IndexOf("{") + 1);
                            fNumStr = fNumStr.Remove(fNumStr.IndexOf("}"));
                            Int32 fNum = Int32.Parse(fNumStr);
                            Functions func = fList[fNum];
                            List<String> funcRes = Calc(func.FunctionList, func.operands);
                            List<Double> rList = new List<double>();
                            for (int k = 0; k < funcRes.Count; k++)
                            {
                                rList.Add(Double.Parse(funcRes[k]));
                            }
                            switch (fName)
                            {
                                case "Если": case "If": 
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (rList[0]==1?rList[1]:rList[2]).ToString() };
                                        break;
                                    }
                                case "Log":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (rList.Count==1?(Math.Log(rList[0])):(Math.Log(rList[0], rList[1]))).ToString() };
                                        break;
                                    }
                                case "Sqr":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (rList[0] * rList[0]).ToString() };
                                        break;
                                    }
                                case "Sin":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Sin(rList[0])).ToString() };
                                        break;
                                    }
                                case "Cos":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Cos(rList[0])).ToString() };
                                        break;
                                    }
                                case "Tan":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Tan(rList[0])).ToString() };
                                        break;
                                    }
                                case "Asin":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Asin(rList[0])).ToString() };
                                        break;
                                    }
                                case "Acos":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Acos(rList[0])).ToString() };
                                        break;
                                    }
                                case "Atan":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Atan(rList[0])).ToString() };
                                        break;
                                    }
                                case "Pow":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Pow(rList[0], rList[1])).ToString() };
                                        break;
                                    }
                                case "Exp":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Exp(rList[0])).ToString() };
                                        break;
                                    }
                                case "Sqrt":
                                    {
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = (Math.Sqrt(rList[0])).ToString() };
                                        break;
                                    }
                                case "Min":
                                    {
                                        Double Min = Double.MaxValue;
                                        for (int k = 0; k < rList.Count;k++)
                                        {
                                            if (rList[k] < Min) { Min = rList[k]; }
                                        }
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = Min.ToString() };
                                        break;
                                    }
                                case "Max":
                                    {
                                        Double Max = Double.MinValue;
                                        for (int k = 0; k < rList.Count; k++)
                                        {
                                            if (rList[k] > Max) { Max = rList[k]; }
                                        }
                                        op[i] = new Lexem() { Type = LexemType.Number, Value = Max.ToString() };
                                        break;
                                    }
                            }
                        }
                    }
                    String resOp = getResult(op);
                    res.Add(resOp);
                }
            }
            return res;
        }

        private object sin(double p)
        {
            throw new NotImplementedException();
        }

        private byte GetPriority(LexemType s)
        {
            switch (s)
            {
                case LexemType.RBracket:
                case LexemType.LBracket:
                    return 0;
                case LexemType.And:
                case LexemType.Or:
                case LexemType.Equals:
                case LexemType.NotEquals:
                case LexemType.Greater:
                case LexemType.Lower:
                case LexemType.GreaterEquals:
                case LexemType.LowerEquals:
                    return 2;
                case LexemType.Plus:
                case LexemType.Minus:
                    return 3;
                case LexemType.Multiply:
                case LexemType.Divide:
                    return 4;
                case LexemType.UnarMinus:
                    return 5;
                default:
                    return 6;
            }
        }

        private Lexem[] ConvertToPostfixNotation(List<Lexem> _lexems)
        {
            List<Lexem> outputSeparated = new List<Lexem>();
            Stack<Lexem> stack = new Stack<Lexem>();
            foreach (Lexem c in _lexems)
            {
                if (operators.Contains(c.Type))
                {
                    if ((stack.Count > 0) && (c.Type!=LexemType.LBracket))
                    {
                        if (c.Type == LexemType.RBracket)
                        {
                            Lexem s = stack.Pop();
                            while (s.Type != LexemType.LBracket)
                            {
                                outputSeparated.Add(s);
                                s = stack.Pop();
                            }
                        }
                        else
                        {
                            if (GetPriority(c.Type) > GetPriority(stack.Peek().Type))
                            {
                                stack.Push(c);
                            }
                            else
                            {
                                while (stack.Count > 0 && GetPriority(c.Type) <= GetPriority(stack.Peek().Type)) outputSeparated.Add(stack.Pop());
                                stack.Push(c);
                            }
                        }
                    }
                    else
                        stack.Push(c);
                }
                else
                    outputSeparated.Add(c);
            }
            if (stack.Count > 0)
                foreach (Lexem c in stack)
                    outputSeparated.Add(c);
            return outputSeparated.ToArray();
        }

        private String getResult(List<Lexem> _lexems)
        {
            Stack<Lexem> stack = new Stack<Lexem>();
            Lexem[] postfix = ConvertToPostfixNotation(_lexems);
            Queue<Lexem> queue = new Queue<Lexem>(postfix);
            Lexem str = queue.Dequeue();
            while (queue.Count >= 0)
            {
                if (operators.Contains(str.Type))
                {
                    Lexem result = new Lexem();
                    result.Type = LexemType.Number;
                    try
                    {

                        switch (str.Type)
                        {
                            case LexemType.UnarMinus:
                                {
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    result.Value = (-a).ToString();
                                    break;
                                }
                            case LexemType.Plus:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    result.Value = (a + b).ToString();
                                    break;
                                }
                            case LexemType.Minus:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    result.Value = (a - b).ToString();
                                    break;
                                }
                            case LexemType.Multiply:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    result.Value = (a * b).ToString();
                                    break;
                                }
                            case LexemType.Divide:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    result.Value = (a / b).ToString();
                                    break;
                                }
                            case LexemType.Equals:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a == b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.NotEquals:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a != b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.Greater:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a > b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.GreaterEquals:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a >= b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.Lower:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a < b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.LowerEquals:
                                {
                                    Double b = Convert.ToDouble(stack.Pop().Value);
                                    Double a = Convert.ToDouble(stack.Pop().Value);
                                    Boolean r = (a <= b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.Or:
                                {
                                    Boolean b = Convert.ToBoolean(Convert.ToDouble(stack.Pop().Value) > 0 ? 1 : 0);
                                    Boolean a = Convert.ToBoolean(Convert.ToDouble(stack.Pop().Value) > 0 ? 1 : 0);
                                    Boolean r = (a || b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                            case LexemType.And:
                                {
                                    Boolean b = Convert.ToBoolean(Convert.ToDouble(stack.Pop().Value) > 0 ? 1 : 0);
                                    Boolean a = Convert.ToBoolean(Convert.ToDouble(stack.Pop().Value) > 0 ? 1 : 0);
                                    Boolean r = (a && b);
                                    result.Value = (r ? 1 : 0).ToString();
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        new InvalidOperationException(ex.Message);
                    }
                    stack.Push(result);
                    if (queue.Count > 0) str = queue.Dequeue(); else break;
                }
                else
                {
                    stack.Push(str);
                    if (queue.Count > 0) { str = queue.Dequeue(); } else break;
                }
            }
            String rValue = stack.Pop().Value;
            return rValue;
        }
    }
}
