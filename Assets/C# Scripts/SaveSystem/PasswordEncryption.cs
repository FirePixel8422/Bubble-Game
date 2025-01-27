using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PasswordEncryption
{
    public static int lockIterations = 10000;


    // Method to generate a consistent encrypted password based on a password and a key
    public static string GenerateEncryptedPassword(string password, string key)
    {
        if (key != "C13_8422FPX")
        {
            return "Nope, try again >:D";
        }

        string result = "";

        Dictionary<char, int> charCounts = new Dictionary<char, int>();

        foreach (char c in password)
        {
            int calcNumber = CharToInt(c);
            calcNumber *= calcNumber + 13;


            if (charCounts.ContainsKey(c))
            {
                calcNumber += charCounts[c] * charCounts[c];

                charCounts[c] += 1;
            }
            else
            {
                charCounts.Add(c, 1);
            }

            char charFromInt = IntToChar(calcNumber);

            result += charFromInt;
        }

        return result + "C@1";
    }




    public static int CharToInt(char c)
    {
        // Convert the character to lowercase to make it case-insensitive
        char lowerChar = Char.ToLower(c);

        Dictionary<char, int> customMappings = new Dictionary<char, int>
        {
            { '1', 27 }, { '2', 28 }, { '3', 29 }, { '4', 30 }, { '5', 31 }, { '6', 32 }, { '7', 33 }, { '8', 34 }, { '9', 35 }, { '0', 36 },
            { '!', 37 }, { '@', 38 }, { '#', 39 }, { '$', 40 }, { '%', 41 }, { '^', 42 }, { '&', 43 }, { '*', 44 }, { '(', 45 }, { ')', 46 },
            { '_', 47 }, { '=', 48 }, { ' ', 49 }, { '{', 50 }, { '[', 51 }, { '}', 52 }, { ']', 53 }, { '|', 54 }, { ';', 55 }, { ':', 56 },
            { '\\', 57 }, { '"', 58 }, { '\'', 59 }, { '<', 60 }, { ',', 61 }, { '>', 62 }, { '.', 63 }, { '?', 64 }, { '/', 65 }
        };

        // Ensure the character is a letter and within the valid range
        if (lowerChar >= 'a' && lowerChar <= 'z')
        {
            // Convert 'a' to 1, 'b' to 2, etc.
            return lowerChar - 'a' + 1;
        }
        // Check if the character exists in the custom mapping
        else if (customMappings.ContainsKey(c))
        {
            return customMappings[c];
        }
        else
        {
            return -1;
        }
    }




    public static char IntToChar(int n)
    {
        Dictionary<int, char> customMappings = new Dictionary<int, char>
        {
            { 27, '1' }, { 28, '2' }, { 29, '3' }, { 30, '4' }, { 31, '5' }, { 32, '6' }, { 33, '7' }, { 34, '8' }, { 35, '9' }, { 36, '0' },
            { 37, '!' }, { 38, '@' }, { 39, '#' }, { 40, '$' }
        };

        for (int i = 0; i < 1000; i++)
        {
            // Ensure the integer is within the valid range (1 to 26)
            if (n >= 1 && n <= 26)
            {
                char targetChar = (char)('a' + n - 1);

                return targetChar;
            }
            else if (customMappings.ContainsKey(n))
            {
                return customMappings[n];
            }
            else
            {
                n -= 26;
            }
        }

        return '#';
    }
}