namespace Smartstore.Extensions
{
    public class RandomExtensions
    {
        public static string GenerateOrganizationKey()
        {
            // Generate random 6-digit number
            string x = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 6)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            // Generate random 4-letter string
            string y = new string(Enumerable.Repeat("0123456789", 4)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            return $"{x}-bmda{y}";
        }
    }
}
