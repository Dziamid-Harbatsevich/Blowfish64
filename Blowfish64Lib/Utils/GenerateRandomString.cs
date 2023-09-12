using System;
using System.Text;

namespace Blowfish64Lib.Utils;

public class GenerateRandomString
{
    /**
     * Generates string token from decimal ASCII code range {32; 126}.
     */
    public static string Generate(int length = 40)
    {
        StringBuilder str_build = new StringBuilder();
        Random random = new Random();

        char letter;

        for (int i = 0; i < length; i++)
        {
            double flt = random.NextDouble();
            int shift = Convert.ToInt32(Math.Floor(79 * flt));
            letter = Convert.ToChar(shift + 32);
            str_build.Append(letter);
        }

        return str_build.ToString();
    }
}