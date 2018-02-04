using System;
using System.Collections.Generic;
using System.Text;

namespace DataObjectNotation
{
    public static class DON
    {
        private const char ListDelimiter = ',';

        private const char PropertyDelimiter = ',';
        private const char PropertyOpen = '(';
        private const char PropertyClose = ')';
        private const char PropertyAssign = '=';

        private const char ChildrenOpen = '{';
        private const char ChildrenClose = '}';

        private const char EscapeCharacter = '|';

        public static DataObject Parse(string input)
        {
            var root = new DataObject("Root");
            var stack = new Stack<DataObject>();
            stack.Push(root);

            DataObject current = root;
            int index = 0;
            int state = 0;
            char character;
            StringBuilder sb = null;
            bool newLineProperties = false;

            string ReadUntil(params (Predicate<char> predicate,Action action)[] calls)
            {
                sb = new StringBuilder();
                while (index < input.Length)
                {
                    character = input[index++];
                    foreach (var (predicate, action) in calls)
                    {
                        if (predicate(character))
                        {
                            var result = sb.ToString();
                            action?.Invoke();
                            return result;
                        }
                    }

                    if (sb.Length == 0 && character == EscapeCharacter)
                    {
                        if (index < input.Length && input[index] == EscapeCharacter)
                        {
                            index++;
                            while (index < input.Length)
                            {
                                character = input[index++];
                                if (character == EscapeCharacter && index < input.Length && input[index] == EscapeCharacter)
                                {
                                    index++;
                                    break;
                                }
                                else
                                    sb.Append(character);
                            }
                            break;
                        }
                    }

                    if (!char.IsWhiteSpace(character) || (char.IsWhiteSpace(character) && sb.Length != 0))
                        sb.Append(character);
                }

                return sb.ToString();
            };

            while (index < input.Length)
            {
                switch (state)
                {
                    case 0:
                        var name = ReadUntil(
                            (c => c == ListDelimiter || c == '\n' || c == '\r', null),
                            (c => c == ChildrenOpen, () => state = 2),
                            (c => c == ChildrenClose, () => state = 3),
                            (c => c == PropertyOpen, () => state = 1));
                        if (!String.IsNullOrEmpty(name))
                        {
                            current = new DataObject(name);
                            stack.Peek().Children.Add(current);
                        }
                        break;

                    case 1:
                        string value = null;
                        name = ReadUntil(
                            (c => c == PropertyDelimiter && !newLineProperties, null),
                            (c => c == '\n' || c == '\r', () => newLineProperties=true),
                            (c => c == PropertyClose, () => state = 0),
                            (c => c == PropertyAssign, () => value = ReadUntil(
                                (c => c == PropertyDelimiter && !newLineProperties, null),
                                (c => c == '\n' || c == '\r', () => newLineProperties=true),
                                (c => c == PropertyClose, () => state = 0))
                            ));

                        if (!String.IsNullOrEmpty(name))
                        {
                            current.Properties.Add(name, value);
                        }

                        break;

                    case 2:
                        stack.Push(current);
                        state = 0;
                        break;

                    case 3:
                        stack.Pop();
                        state = 0;
                        break;
                }
            }

            return root;
        }
    }
}