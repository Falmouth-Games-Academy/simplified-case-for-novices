using System;

/*
You do not need to successfully complete the tasks given, just have an attempt at
what you can. It's better to attempt each of the sections and revisit areas where you get
might get stuck later.

There is no pressure to write good code or to finish any of this, feel free to ask questions
about the tasks themselves but avoid asking for solutions.
 
Your solution won't be traced back to you or used to "grade" you, it won't be shown to anyone
outside of this testing environment

Feel free to add any libraries and/or new files

You are encouraged to use the tool you've been provided with in these exercises!
*/

namespace ER_CompBase
{
    /// <summary>
    /// Base Class given to write in for the Educational Refactoring Testing
    /// </summary>
    public static class StringManipulation
    {
        static void Main()
        {
            throw new NotImplementedException();
        }

        // Complete Task 1 here:
        public static string RepeatString(int repeatCount, string input)
        {
            string output = "";
            for(int i = 0; i < repeatCount; i++)
            {
                output += input;
            }
            return output;
        }

        // Complete Task 2 here:
        public static string ReverseString(string input)
        {
            string[] words = input.Split(" ");
            string output = "";
            for(int i = words.Length - 1; i > 0; i--)
            {
                output += words[i] + " ";
            }
            output += words[0];
            return output;
        }

        // Complete Task 3 here:
        public static string InvertCasing(string input)
        {
            string output = "";
            foreach(char c in input.ToCharArray())
            {
                char o = c;
                if (char.IsLower(c))
                {
                    o = char.ToUpper(c);
                }
                else
                {
                    o = char.ToLower(c);
                }
                output += o.ToString();
            }
            return output;
        }

        // Complete Task 4 here:
        public static string RemoveChars(string input, char[] charsToRemove)
        {
            foreach(char c in charsToRemove)
            {
                input = input.Replace(c.ToString(), "");
            }
            return input;
        }
    }
}
