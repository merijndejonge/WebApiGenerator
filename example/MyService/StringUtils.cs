namespace MyService
{
    public class StringUtils : IStringUtils
    {
        public string ToUpper(string input)
        {
            return input.ToUpper();
        }
    }
}