namespace Tools.StringManipulator
{
    public class StringParser
    {
        private static readonly bool[] _lookup;

        static StringParser()
        {
            _lookup = new bool[65545];

            for (char c = '0'; c <= '9'; c++) _lookup[c] = true;
            for (char c = 'A'; c <= 'Z'; c++) _lookup[c] = true;
            for (char c = 'a'; c <= 'z'; c++) _lookup[c] = true;

            _lookup['.'] = true;
            _lookup['_'] = true;
            _lookup[' '] = true;
            _lookup['-'] = true;
            _lookup[','] = true;
            _lookup['['] = true;
            _lookup[']'] = true;
            _lookup['"'] = true;
            _lookup['&'] = true;
            _lookup['('] = true;
            _lookup[')'] = true;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            char[] buffer = new char[str.Length];
            int index = 0;

            foreach (char c in str)
            {
                if (_lookup[c])
                {
                    buffer[index] = c;
                    index++;
                }
            }

            return new string(buffer, 0, index);
        }
    }
}
