using System;

namespace VariablesTipos
{
    class Program
    {
        static void Main(string[] args)
        {
            // ========= Ejercice 1. Declare cariables to store your name, age, height, and profession, and print them to the console.
            string name = "Jefry Astacio";
            int age = 26;
            double height = 5.4;
            bool isStudent = true;
            char grade = 'A';

            Console.WriteLine(" - Ejercice 1.");
            Console.WriteLine($"1. Hello,.");
            Console.WriteLine($"2. my name is {name}.");
            Console.WriteLine($"3. I am {age} years old.");
            Console.WriteLine($"4. My height is {height} feet.");
            Console.WriteLine($"5. I am a student: {isStudent}.");
            Console.WriteLine($"6. My grade is {grade}.     \n");
            


            // ========= Ejercice 2. Convert and print different data types.

            // Convert implicit types.
            int num1 = 10;
            double decimalNum = num1; // implicit conversion from int to double.

            // Convert explicity types.
            double price = 19.99;
            int priceInt = (int)price; // explicity conversion from double to int.

            // Convert with methods.
            string textNum = "123";
            int parsedNum = int.Parse(textNum); // converting string to int eith Convert class.

            Console.WriteLine(" - Ejercice 2.");
            Console.WriteLine($"1. Original Number: {num1}.");
            Console.WriteLine($"2. Convert to decimal: {decimalNum}.");
            Console.WriteLine($"3. Original price: {price}.");
            Console.WriteLine($"4. Integer price: {priceInt}.");
            Console.WriteLine($"5. Converted text: {parsedNum}.     \n");


            // ========= Ejercice 3. Calculate with different data types.
            int number1 = 10;
            double number2 = 5.5;
            decimal number3 = 3.5m;

            // Conversion automatique in calculations.
            double result1 = number1 + number2; // int + double = double
            decimal result2 = number3 * 2; // decimal * int = decimal

            Console.WriteLine(" - Ejercice 3.");
            Console.WriteLine($"1. Result 1 (int {number1} + double {number2}): {result1}");
            Console.WriteLine($"2. Result 2 (decimal {number3} * int 2): {result2}    \n");


            // ========= Ejercice 4. Use from var
            var number = 42; // C# infered as int
            var text = "Hello, World!"; // C# infered as string
            var decimalNumber = 3.14; // C# infered as double
            var isTrue = true; // C# infered as bool

            Console.WriteLine(" - Ejercice 4.");
            Console.WriteLine($"1. Type of number: {number.GetType()}");
            Console.WriteLine($"2. Type of text: {text.GetType()}");
            Console.WriteLine($"3. Type of decimalNumber: {decimalNumber.GetType()}");
            Console.WriteLine($"4. Type of isTrue: {isTrue.GetType()}   \n"); 



        }
    }
}
