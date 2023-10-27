using System;
using System.Data;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Calculator
{
    internal class Program
    {
        /*Megvizsgálja, hogy a mműveletben szerepel-e műveleti jel, és ha igen, akkor a precedencia szabályát alkalmazva melyik a soron következő műveleti jel.
        Egyenrangú műveletek esetén a balról-jobbra elvet követi.*/
        static string IdentifyOperator(string operation)
        {
            string operatorType = "";
            int indexOfOperator1;
            int indexOfOperator2;

            if (operation.IndexOf("√") != -1 || operation.IndexOf("\\") != -1)
            {
                operatorType = "√";
            }
            else if (operation.IndexOf("^") != -1)
            {
                operatorType = "^";
            }
            else if (operation.IndexOf("*") != -1 || operation.IndexOf("/") != -1)
            {
                indexOfOperator1 = operation.IndexOf("*");
                indexOfOperator2 = operation.IndexOf("/");

                if (indexOfOperator1 != -1 && indexOfOperator2 != -1)
                {
                    if (indexOfOperator1 < indexOfOperator2)
                    {
                        operatorType = "*";
                    }
                    else { operatorType = "/"; }
                }
                else
                {
                    if (indexOfOperator1 != -1)
                    {
                        operatorType = "*";
                    }
                    else { operatorType = "/"; }
                }
            }
            else if (operation.IndexOf("+") != -1 || operation.IndexOf("-") != -1)
            {
                if (operation.StartsWith("+"))
                {
                    indexOfOperator1 = operation.IndexOf("+", 1);
                }
                else { indexOfOperator1 = operation.IndexOf("+"); }

                if (operation.StartsWith("-"))
                {
                    indexOfOperator2 = operation.IndexOf("-", 1);
                }
                else { indexOfOperator2 = operation.IndexOf("-"); }

                if (indexOfOperator1 != -1 && indexOfOperator2 != -1)
                {
                    if (indexOfOperator1 < indexOfOperator2)
                    {
                        operatorType = "+";
                    }
                    else { operatorType = "-"; }
                }
                else
                {
                    if (indexOfOperator1 != -1)
                    {
                        operatorType = "+";
                    }
                    else if (indexOfOperator2 != -1)
                    {
                        operatorType = "-";
                    }
                }
            }

            return operatorType;
        }

        /*Ellenőrzi, hogy az osztó nem nulla-e.*/
        static decimal DvisionByZero(double x, double y)
        {
            decimal result = 0;

            try
            {
                result = (decimal)(x / y);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"A művelet nullával való osztást tartalmaz - \"{x}/{y}\" nem számolható ki! ({e.Message})");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return result;
        }

        /*Kiszámolja a részműveleteket a megadott értékek és műveleti jel alapján.*/
        static decimal BasicCalculator(double x, double y, string operatorType)
        {
            decimal result;
            switch (operatorType)
            {
                case "+": result = (decimal)(x + y); break;
                case "-": result = (decimal)(x - y); break;
                case "*": result = (decimal)(x * y); break;
                case "/": result = DvisionByZero(x, y); break;
                case "^": result = (decimal)(Math.Pow(x, y)); break;
                case "√": result = (decimal)(Math.Pow(y, 1/x)); break;
                default: result = 0; break;
            }
            return result;

        }

        /*A műveleti jelek mentén feldarabolja és tömbként adja vissza a műveletet. Amikor talál egy operátort, előbb az előző operátortól, illetve - a sztring első
        operátora esetén - a sztring elejétől a megtalált operátorig tartó rész-sztringet teszi a tömb soronkövetkező elemébe, majd a rákövetkező elembe teszi az
        operátort. Ha a sztring 1. karaktere, illetve egy operátor után következő karakter operátor, az csak a '-' jel lehet, tehát nem teszi be külön elemként a tömbbe, 
        (mivel nem tesz be egymás után 2 operátort) hanem a következő operátorig tartó operandust reprezentáló rész-sztringel együtt kerül be a tömb következő elemébe.
        Végül az utolsó operátor utáni rész-sztringet - mint utolsó operandust - elhelyezi a tömb utolsó elemébe.*/
        static string[] SplitOperation(string operation, string[] splittedOperation)
        {
            string[] operators = { "\\", "√", "^", "*", "/", "+", "-" };
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

                            splittedOperation[indexOfSplittedOperation] = (operators[j] == "\\") ? "√" : operators[j];

                            //splittedOperation[indexOfSplittedOperation] = operators[j];
                            indexOfSplittedOperation++;
                        }
                    }
                }
            }
            operandString = operation.Substring(indexOfOperand, operation.Length - indexOfOperand);
            splittedOperation[indexOfSplittedOperation] = operandString;

            return splittedOperation;
        }

        /*A műveleti jelek mentén a SplitOperation() metódussal feldaraboltatja a felhasználó által szöveges formátumban megadott és a zárójelektől megszabadított
        műveletet, betölti egy tömbbe, az IdentifyOperator() visszatérési értékét alapul véve a precedencia szabályai szerint lépésről-lépésre elvégzteti a 
        műveleteket a BasicCalculator() metódussal, majd visszatér az eredménnyel.*/
        static string ManipulateOperation(string operation)
        {
            string[] splittedOperation = new string[operation.Length];
            splittedOperation = SplitOperation(operation, splittedOperation);
            string operatorType = IdentifyOperator(operation);
            decimal result;
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
                    resultString = Math.Round(result, 6).ToString();
                    newOperation = operation.Replace(operationToCalculate, resultString);
                }
            }


            return newOperation;
        }

        /*Megkeresi az első jobboldali zárójel párját, ezáltal meghatároza az első legbelső zárójelpár indexeit és a zárójeles kifejezés hosszát. Levágja 
        róla a zárójeleket, a ManipulateOperation() metódussal elvégezteti a műveletet, majd az adott zárójeles kifejezést kicseréli annak eredményére a 
        műveletben, és ezzel a művelettel tér vissza.*/
        static string ResolveBracketedOperation(string operation, int rightParIndex)
        {
            string parOperation = operation;
            string trimmedOperation;
            int leftParIndex = operation.IndexOf('(');
            int temporaryLeftParIndex = leftParIndex;

            do
            {
                int parOpLength = rightParIndex - leftParIndex + 1;
                parOperation = parOperation.Substring(temporaryLeftParIndex, parOpLength);
                trimmedOperation = parOperation.Remove(parOperation.IndexOf('('), 1);

                leftParIndex = trimmedOperation.IndexOf('(');
                rightParIndex = trimmedOperation.IndexOf(')');
                temporaryLeftParIndex = leftParIndex+1;
            } 
            while (leftParIndex != -1);

            trimmedOperation = trimmedOperation.Trim(')');

            trimmedOperation = ManipulateOperation(trimmedOperation);

            if (IdentifyOperator(trimmedOperation) != "")
            {
                trimmedOperation = '(' + trimmedOperation + ')';
            }

            string newOperation = operation.Replace(parOperation, trimmedOperation);

            return newOperation;
        }

        /*Karakterenként beolvassa a műveletet egy változóba, majd karakterláncba fűzi. A numerikus billentyűket, valamint - a hatványozás és gyök jelének
         beolvasásához - a fel és le nyilakat rendeli hozzá.*/
        static string inputOperation()
        {
            string input;
            string newOperation = "";
            //string escInput = "";
            ConsoleKeyInfo keyInfo;
            //int i = Console.CursorLeft;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8; 

            do
            {
                keyInfo = Console.ReadKey(true);
               
                switch (keyInfo.Key)
                {
                    case ConsoleKey.DownArrow: 
                        input = "√";
                        newOperation += input;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(newOperation);
                        break;
                    case ConsoleKey.UpArrow: 
                        input = "^";
                        newOperation += input;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(newOperation);
                        break;
                    case ConsoleKey.Add:
                    case ConsoleKey.Divide:
                    case ConsoleKey.Multiply:
                    case ConsoleKey.Subtract:
                    case ConsoleKey.Decimal:
                    case ConsoleKey.NumPad0:
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.NumPad5:
                    case ConsoleKey.NumPad6:
                    case ConsoleKey.NumPad7:
                    case ConsoleKey.NumPad8:
                    case ConsoleKey.NumPad9:
                    case ConsoleKey.D8:
                    case ConsoleKey.D9:
                        //input = keyInfo.KeyChar.ToString();
                        newOperation += keyInfo.KeyChar;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(newOperation);
                        break;

                    case ConsoleKey.Backspace:
                        //input = keyInfo.KeyChar.ToString();
                        newOperation = newOperation.Remove(newOperation.Length-1);
                        Console.SetCursorPosition(Console.CursorLeft-1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(newOperation);
                        break;

                    case ConsoleKey.Insert:
                        newOperation = Console.ReadLine();
                        
                        Console.SetCursorPosition(Console.CursorLeft+newOperation.Length, Console.CursorTop-1);
                        break;

                    default:; break;
                 }

            }

            while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return newOperation;
        }

        static void Main(string[] args)

        {
            /*Kiírja egy üzenetben a használati útmutatót, majd bekér egy összetett műveletet a következő részműveletek (+, -, *, /, ^, n√) és akár egymásba ágyazott 
            zárójelek használatával. */
            Console.Title = "Számológép";
            Console.WriteLine($"Adj meg egy összetett műveletet a következő részműveletek (+, -, *, /, ^, n√) " +
                $"és akár egymásba ágyazott zárójelek használatával!");
            Console.WriteLine("(y^x) - Hatványozáshoz használd a ('↑') billentyűt!");
            Console.WriteLine("(n√x) - Gyökvonáshoz használd a ('↓') billentyűt, az 'n' helyére írd be, hogy " +
                "hányadik gyököt szeretnéd megkapni!");

            /*Bekéri a műveletet, és egy változóban elmanti.*/
            string operation = inputOperation();

            /*A beolvasott műveletre meghívja a ResolveBracketedOperation metódust, ami balról jobbra haladva feloldja a zárójeleket, majd egy új változóban folyamatosan
            frissíti a műveletet a kiszámolt értékekkel, amíg a zárójelek el nem fogynak.*/
            string resolvedOperation = operation; 
            int rightParIndex = resolvedOperation.IndexOf(')');

            while (rightParIndex != -1)
            {
                resolvedOperation = ResolveBracketedOperation(resolvedOperation, rightParIndex);
                Console.WriteLine(resolvedOperation);
                rightParIndex = resolvedOperation.IndexOf(')');
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