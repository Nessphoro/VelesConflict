using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace VelesConflict.Parsing
{
    public static class Colors
    {
        private static readonly Dictionary<string, Color> dictionary =
            typeof(Color).GetProperties(BindingFlags.Public |
                                        BindingFlags.Static)
                         .Where(prop => prop.PropertyType == typeof(Color))
                         .ToDictionary(prop => prop.Name,
                                       prop => (Color)prop.GetValue(null, null));

        public static Color FromName(string name)
        {
            if (!dictionary.ContainsKey(name))
            {
                return Color.White;
            }
            return dictionary[name];
        }
    }
    class TextManager
    {
        public TextManagerSettings Settings { get; set; }
        public string Text { get; set; }
        public float Height { get; private set; }
        private List<TextManagerOperation> Operations = new List<TextManagerOperation>();

        public TextManager()
        {
            Settings = new TextManagerSettings();
        }
        public TextManager(string text)
        {
            Text = text;
        }
        public TextManager(string text, TextManagerSettings settings)
        {
            this.Settings = settings;
            this.Text = text;
        }

        public void Parse()
        {

            Operations.Clear();
            Settings.Font.LineSpacing = 14;
            int Index = 0;
            int IndexOffset;
            int MaxSpaceLength = (int)Settings.Font.MeasureString("X").X;
            if (Settings.Width < MaxSpaceLength)
                Settings.Width = MaxSpaceLength + 1;
            StringBuilder Fragment = new StringBuilder("");
            StringBuilder WordFragment = new StringBuilder();
            char[] String = Text.ToCharArray();
            Vector2 Position = new Vector2();
            Color color = Color.White;
            TextManagerOperation operation = new TextManagerOperation();

            while (Index < String.Length)
            {
                int Width = (int)(Position.X + Settings.Font.MeasureString(Fragment).X);
                if (Width >= Settings.Width - MaxSpaceLength)
                {
                    operation.TextFragment = Fragment.ToString();
                    operation.Font = Settings.Font;
                    operation.Position = new Vector2(Position.X, Position.Y);
                    operation.Color = color;
                    Operations.Add(operation);


                    Fragment.Clear();
                    operation = new TextManagerOperation();
                    Position.X = 0;
                    Position.Y += Settings.Font.LineSpacing;
                }
                else
                {
                    char Peek = String[Index];
                    if (Peek == ' ' && Fragment.Length == 0 && Position.X == 0)
                    {
                        Index++;
                        continue;
                    }
                    switch (Peek)
                    {
                        case '\n':
                            operation.TextFragment = Fragment.ToString();
                            operation.Font = Settings.Font;
                            operation.Position = new Vector2(Position.X, Position.Y);
                            operation.Color = color;
                            Operations.Add(operation);


                            Fragment.Clear();
                            operation = new TextManagerOperation();
                            Position.X = 0;
                            Position.Y += Settings.Font.LineSpacing;
                            Index++;
                            break;
                        case '\t':
                            IndexOffset = 1;
                            WordFragment.Clear();
                            for (; Index + IndexOffset < String.Length; IndexOffset++)
                            {
                                Peek = String[Index + IndexOffset];
                                if (Peek == '\t')
                                {
                                    IndexOffset++;
                                    break;
                                }
                                else if (!(Peek == '\n' || Peek == '?' || Peek == '!' || Peek == '.' || Peek == ' '))
                                {
                                    WordFragment.Append(Peek);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(Fragment.ToString()))
                            {
                                operation.TextFragment = Fragment.ToString();
                                operation.Font = Settings.Font;
                                operation.Position = new Vector2(Position.X, Position.Y);
                                operation.Color = color;
                                Operations.Add(operation);
                                Position.X += Settings.Font.MeasureString(Fragment).X;
                                Fragment.Clear();
                                operation = new TextManagerOperation();
                            }
                            try
                            {
                                color = Colors.FromName(WordFragment.ToString());
                            }
                            catch
                            {
                                color = Color.White;
                            }
                            Index += IndexOffset;
                            break;
                        case ' ':
                            //Probably a start of a new word
                            //Lets see if we can fit it
                            IndexOffset = 1;
                            WordFragment.Clear();
                            WordFragment.Append(' ');
                            for (; Index + IndexOffset < String.Length; IndexOffset++)
                            {
                                Peek = String[Index + IndexOffset];
                                if (!(Peek == '\n' || Peek == '\t' || Peek == '?' || Peek == '!' || Peek == '.' || Peek == ' '))
                                {
                                    WordFragment.Append(Peek);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            Width += (int)(Settings.Font.MeasureString(WordFragment).X);
                            if (Width >= Settings.Width - MaxSpaceLength)
                            {
                                operation.TextFragment = Fragment.ToString();
                                operation.Font = Settings.Font;
                                operation.Position = new Vector2(Position.X, Position.Y);
                                operation.Color = color;
                                Operations.Add(operation);


                                Fragment.Clear();
                                operation = new TextManagerOperation();
                                Position.X = 0;
                                Position.Y += Settings.Font.LineSpacing;

                                Index++;
                            }
                            else
                            {
                                Fragment.Append(WordFragment);
                                Index += IndexOffset;
                            }
                            break;
                        default:
                            Fragment.Append(Peek);
                            Index++;
                            break;
                    }
                }
            }
            operation.TextFragment = Fragment.ToString();
            operation.Font = Settings.Font;
            operation.Position = new Vector2(Position.X, Position.Y);
            operation.Color = color;
            Operations.Add(operation);
            Height = Position.Y + Settings.Font.LineSpacing;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (TextManagerOperation operation in Operations)
            {
                spriteBatch.DrawString(operation.Font, operation.TextFragment, Settings.Offset + operation.Position + Settings.Origin, operation.Color, 0f, Settings.Origin, Settings.Scale, SpriteEffects.None, Settings.Depth);
            }
        }
    }
}
