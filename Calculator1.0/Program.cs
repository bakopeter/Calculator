using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Calculator
{
    internal class Program
    {
        /*Megvizsgálja, hogy a mműveletben szerepel-e műveleti jel, és ha igen, akkor
         a precedencia szabályát alkalmazva melyik a soron következő műveleti jel.*/
        static string IdentifyOperator(string operation)
        {
            string operatorType = "";
            if (operation.IndexOf("*") != -1)
            {
                operatorType = "*";
            }
            else if (operation.IndexOf("/") != -1)
            {
                operatorType = "/";
            }
            else if (operation.IndexOf("+") != -1)
            {
                operatorType = "+";
            }
            else if (operation.IndexOf("-") != -1)
            {
                operatorType = "-";
            }

            return operatorType;
        }

        /*Kiszámolja a részműveleteket a megadott értékek és műveleti jel alapján.*/
        static double BasicCalculator(double x, double y, string operatorType)
        {
            double result;
            switch (operatorType)
            {
                case "+":
                    result = x + y;
                    break;
                case "-":
                    result = x - y;
                    break;
                case "*":
                    result = x * y;
                    break;
                case "/":
                    result = x / y;
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }
        static string[] SplitOperation(string operation, string[] splittedOperation)
        {
            string[] operators = { "*", "/", "+", "-" };
            int indexOfSplittedOperation = 0;
            string operandString;
            int indexOfOperand = 0;

            for (int i = 0; i < operation.Length; i++)
            {
                for (int j = 0; j < operators.Length; j++)
                {
                    if (operators[j] == Convert.ToString(operation[i]))
                    {
                        if (indexOfOperand < i)
                        {
                            operandString = operation.Substring(indexOfOperand, i - indexOfOperand);
                            splittedOperation[indexOfSplittedOperation] = operandString;
                            indexOfSplittedOperation++;
                            indexOfOperand = i + 1;

                            splittedOperation[indexOfSplittedOperation] = operators[j];
                            indexOfSplittedOperation++;
                        }
                    }
                }
            }
            operandString = operation.Substring(indexOfOperand, operation.Length - indexOfOperand);
            splittedOperation[indexOfSplittedOperation] = operandString;

            return splittedOperation;
        }

        static string ManipulateOperation(string operation)
        {
            //A műveleti jelek mentén feldarabolja a felhasználó által szövges formátumban megadott
            //műveletet, betölti egy tömbbe, a precedencia szabályai szerint lépésről-lépésre elvégzi
            //a műveleteket, majd visszatér az eredménnyel.

            string[] splittedOperation = new string[operation.Length];
            splittedOperation = SplitOperation(operation, splittedOperation);
            string operatorType = IdentifyOperator(operation);
            double result;
            double operand1;
            double operand2;
            string newOperation = "";
            string operationToCalculate;
            string resultString;

            for (int i = 0; i < splittedOperation.Length; i++)
            {
                if (splittedOperation[i] == operatorType)
                {
                    operand1 = Convert.ToDouble(splittedOperation[i - 1]);
                    operand2 = Convert.ToDouble(splittedOperation[i + 1]);
                    operationToCalculate = $"{operand1}{operatorType}{operand2}";
                    result = BasicCalculator(operand1, operand2, operatorType);
                    resultString = Math.Round(result, 2).ToString();
                    newOperation = operation.Replace(operationToCalculate, resultString);
                }
            }


            return newOperation;
        }

        static string ResolveBracketedOperation(string operation)
        {
            /*Meghatározza az első zárójelpár indexeit és a zárójeles kifejezés hosszát.*/
            int leftParIndex = operation.IndexOf('(');
            int rightParIndex = operation.IndexOf(')');
            int parOpLength = rightParIndex - leftParIndex + 1;

            //Szöveges formában visszaadja a műveletet, majd levágja róla a zárójeleket.
            string parOperation = operation.Substring(leftParIndex, parOpLength);
            char[] charsToTrim = { '(', ')' };
            string trimmedOperation = parOperation.Trim(charsToTrim);

            /* Az identifyOperator metódustól lekéri az aktuális műveleti jelet, két részre 
            darabolja a műveletet a megadott operátor mentén, majd a szöveges tömbben visszakapott 
            két operandust double tipusúvá alakítja.*/
            string operatorType = IdentifyOperator(trimmedOperation);
            string[] parameters = trimmedOperation.Split(operatorType);
            double parameter1 = Convert.ToDouble(parameters[0]);
            double parameter2 = Convert.ToDouble(parameters[1]);

            /*Meghívja a BasicCalculator metódust, ami kiszámolja a kifejezés eredményét
            a megadott számok és műveleti jel alapján.*/
            double result = BasicCalculator(parameter1, parameter2, operatorType);


            /*A felhasználó által megadott műveletben kicseréli a zárójeles műveleteket
            az eredményükre két tizedesre kerekítve.*/
            string resultString = Math.Round(result, 2).ToString();
            string newOperation = operation.Replace(parOperation, resultString);

            return newOperation;
        }

        static void Main(string[] args)

        {
            /*Kiír egy üzenetet, majd bekér egy műveletet, ami pillanatnyilag az
            alapműveleteket (+, -, *, /) és nem egymásba ágyazott zárójelezést kezel.*/

            Console.WriteLine("Számológép");
            string operation = Console.ReadLine();

            /*A beolvasott műveletre meghívja a resolvedOperation metódust, ami balról
            jobbra haladva feloldja a zárójeleket, majd egy új változóban folyamatosan
            frissíti a műveletet a kiszámolt értékekkel, amíg a zárójelek el nem fogynak.*/

            string resolvedOperation = operation;

            while (resolvedOperation.IndexOf('(') != -1)
            {
                resolvedOperation = ResolveBracketedOperation(resolvedOperation);
                Console.WriteLine(resolvedOperation);
            }

            /*A zárójelektől megtisztított részműveleteket a precedencia szabálya szerint 
             balról jobbra halava kiszámolja és a változóban mindig frissíti a művelet 
             aktuális állapotát mindaddig, amíg a műveleti jelek el nem fogynak.*/

            while (IdentifyOperator(resolvedOperation) != "")
            {
                resolvedOperation = ManipulateOperation(resolvedOperation);
                Console.WriteLine(resolvedOperation);
            }

        }
    }
}