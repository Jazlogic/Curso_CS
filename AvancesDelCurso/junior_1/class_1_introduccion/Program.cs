// Luego, declara variables para almacenar tu nombre, edad, estatura y profesion, e imprimelas en la consola.
// 1/9/2025 Jefry Astacio.
using System;

namespace Introduccion
{
    class Program
    {
        static void Main(string[] args)
        {
            // Crea un programa que imprima "Hola Mundo" en la consola.
            Console.WriteLine("Ejercicio 1: Hola Mundo");
            Console.WriteLine("!Hola Mi nombre es Jefry Astacio!\n\n\n");

           // Luego, declara variables para almacenar tu nombre, edad, estatura y profesion, e imprimelas en la consola.
            string nombre = "Jefry Astacio";
            int edad = 25;
            double estatura = 5.4;
            string profesion = "Desarrollador de software";
            Console.WriteLine("Hola Mundo desde C#!");
            Console.WriteLine($"Hola Mi nombre es {nombre}");

            Console.WriteLine($"Hola Mi nombre es {nombre}, tengo {edad}, años de edad, mi estatura es {estatura}, yo estoy especializado en el area de {profesion}");



        }
    }
}
