using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI
{
    class CommandLineChooser
    {
        List<CommandLineChoice> choices = new List<CommandLineChoice>();

        public CommandLineChooser(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Add(CommandLineChoice choice)
        {
            choices.Add(choice);
        }

        public CommandLineChoice? Choose()
        {
            CommandLineChoice choice = null;
            while (choice is null)
            {

                Console.WriteLine($"You need to choose a {Name}:");
                // write out choices
                var count = 1;
                foreach (CommandLineChoice options in choices)
                {
                    Console.WriteLine($"{count}. {options.Name}");
                    count++;
                }
                int selected = GetNumberFromUser(choices.Count);
                if (selected == -1 || selected == 0)
                {
                    throw new InvalidSelectionException();
                }
                choice = choices[selected-1];

            }
            return choice;
        }

        private int GetNumberFromUser(int max)
        {
            string output = string.Empty;
            ConsoleKeyInfo cki;
            Console.Write("> ");
            do
            {
                cki = Console.ReadKey(false);

                if (Char.IsNumber(cki.KeyChar))
                { 
                    Int32 number;
                    if (Int32.TryParse(cki.KeyChar.ToString(), out number))
                    {
                        output += number.ToString();
                    }
                } else if (cki.Key != ConsoleKey.Escape && cki.Key != ConsoleKey.Enter)
                {
                    Console.WriteLine("Invalid key, only numbers are allowed.");
                    return -1;
                }
                Int32 outtest;
                Int32.TryParse(output, out outtest);
                if (outtest > max)
                {
                    Console.WriteLine("Number exeded maximum choice.");
                    return -1;
                }    
            } while (cki.Key != ConsoleKey.Escape && cki.Key != ConsoleKey.Enter);
            Int32 returnable;
            Int32.TryParse(output, out returnable);
            return returnable;
        }

    }

    class InvalidSelectionException : Exception { }


    class CommandLineChoice
    {
        public CommandLineChoice(dynamic name, dynamic id)
        {
            Name = name;
            Id = id;

        }

        public dynamic Name { get; }
        public dynamic Id { get; }
    }
}
