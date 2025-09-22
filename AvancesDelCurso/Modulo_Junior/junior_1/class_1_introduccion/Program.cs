// Luego, declara variables para almacenar tu nombre, edad, estatura y profesion, e imprimelas en la consola.
// 1/9/2025 Jefry Astacio.
using System;

namespace Introduccion
{
    class Program
    {
        static void Main(string[] args)
        {
            //Ejercicio 1: Hello World Personalizado
            Console.WriteLine("Ejercicio 1.");
            Console.WriteLine("!Hola Mi nombre es Jefry Astacio!\n\n\n");

           //Ejercicio 2: Múltiples Líneas
            string nombre = "Jefry Astacio";
            int edad = 25;
            double estatura = 5.4;
            string profesion = "Desarrollador de software";
            Console.WriteLine("Hola Mundo desde C#!");
            Console.WriteLine($"Hola Mi nombre es {nombre}");
            Console.WriteLine($"Hola Mi edad es {edad}");
            Console.WriteLine($"Hola Mi estatura es {estatura} \n\n\n");

            //Ejercicio 3: Información Personal
            Console.WriteLine($"Hola Mi nombre es {nombre}, tengo {edad}, años de edad, mi estatura es {estatura}, yo estoy especializado en el area de {profesion}");
            Console.WriteLine("\n\n\n");

            //Ejercicio 4: Cálculo Simple
            int num1 = 10;
            int num2 = 15;
            int suma = num1 + num2;
            Console.WriteLine($"La suma de {num1} y {num2} es: {suma}");
            Console.WriteLine("\n\n\n");



        }
    }
}
