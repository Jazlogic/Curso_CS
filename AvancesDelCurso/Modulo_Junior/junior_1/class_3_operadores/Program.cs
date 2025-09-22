using System;

namespace Class_3_operadores
{
    class Program
    {
        static void Main(string[] args)
        {
            /* 🧮 1. Gestión de inventario técnico.
            Descripción: Simula un sistema que lleva el conteo de dispositivos disponibles (como laptops o routers). Cada vez que se entrega uno, el inventario disminuye; cuando se devuelve, aumenta. Objetivo: Aplicar operadores unarios (++, --) y binarios (+=, -=) para modificar valores numéricos.*/ 

            int inventory = 20; // initial inventory
            Console.WriteLine($"total inventory: {inventory}");

            inventory--; // device delivered
            Console.WriteLine($"total inventory after delivery: {inventory}");

            inventory++; // delivery returned
            Console.WriteLine($"Total inventory after returned delivery: {inventory}");

            inventory += 5; // news devices arrived
            Console.WriteLine($"Total inventory after new devices arrived: {inventory}");

            inventory -= 10; // devices delivered
            Console.WriteLine($"Total inventory after devices delivered: {inventory}");

            /*💰 2. Cálculo de salario con horas extra.
            Descripción: Crea una lógica que calcule el salario de un empleado. Si trabaja más de 40 horas, se le paga un bono por cada hora extra. Objetivo: Usar operadores binarios para cálculos matemáticos y el operador ternario (? :) para aplicar condiciones.*/

            int hoursWorked = 45;
            int hourlyRate = 20;
            int salary = hoursWorked <= 40 ? hoursWorked * hourlyRate : (40 * hourlyRate) + ((hoursWorked - 40 ) * (hourlyRate * 2));
            Console.WriteLine($"Total salary: {salary}");

            /*🔐 3. Validación de acceso a un sistema.
            Descripción: Diseña una lógica que determine si un usuario puede acceder a una plataforma interna. Si no tiene permisos, se le niega el acceso. Objetivo: Practicar el operador unario ! para invertir valores booleanos y condicionar el flujo del programa.*/

            

            /*📊 4. Comparación de rendimiento entre dos aplicaciones.
            Descripción: Simula una comparación entre dos sistemas internos midiendo su tiempo de respuesta. El sistema más rápido se recomienda para uso prioritario. Objetivo: Usar operadores binarios de comparación (<, >, ==) y el operador ternario para tomar decisiones automáticas.*/

            /*📦 5. Simulación de carga de tickets de soporte.
            Descripción: Imagina que estás gestionando los tickets diarios de soporte. Cada vez que llega uno, se suma al conteo; cuando se resuelve, se resta. Objetivo: Aplicar operadores unarios y binarios para simular el flujo de trabajo y mantener el estado actualizado.*/
        }
    }
}