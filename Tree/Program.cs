using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Tree
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "(id,created,employee(id,firstname,employeeType(id), lastname),location)";
            Console.WriteLine(input);

            ItemSerializer ser = new ItemSerializer(ItemDelimiters.Comma, NestingPairs.Braces);
            Item<string> root = ser.Deserialize(input);
            Console.WriteLine(root.ToString());

            root.Sort();
            Console.WriteLine(root.ToString());

            Console.ReadLine();

        }


        #region classes

        public class Item<T> : IComparable<Item<T>>
        {
            public T Me { get; set; }
            public List<Item<T>> Children { get; set; }

            public Item()
            {
                Children = new List<Item<T>>();
            }

            #region for display the tree

            public override string ToString()
            {

                if (Children.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    
                    BuildTree(ref sb, 0);
                    return sb.ToString();
                }

                return Me.ToString().Trim();
            }

            private void BuildTree(ref StringBuilder sb, int level)
            {
                
                foreach (Item<T> child in Children)
                {
                    sb.AppendLine(string.Format("{0} {1}", new String('-', level), child.Me).Trim());
                    if (child.Children.Any())
                    {
                        level++;
                        child.BuildTree(ref sb, level);
                        level--;
                    }

                }


            }

            #endregion



            #region for sorting

            public int CompareTo(Item<T> other)
            {

                if (other == null) return 1;

                return Me.ToString().CompareTo(other.Me.ToString());
            }


            public void Sort()
            {
                Children.Sort();
                foreach (var child in Children)
                    child.Sort();

            }

            #endregion
        }


        public class ItemSerializer
        {
            private string _delimiter;
            private string _left;
            private string _right;
            private IEnumerable<string> _parts;
            private IEnumerator<string> _enumerator;

            public ItemSerializer(ItemDelimiters delimiter, NestingPairs pair)
            {

                if (delimiter != ItemDelimiters.Comma || pair != NestingPairs.Braces)
                    throw new NotSupportedException("Only comma delimiter and nesting braces are supported at this point!");
                _delimiter = ",";
                _left = "(";
                _right = ")";
            }

            public Item<string> Deserialize(string input)
            {
                if (string.IsNullOrEmpty(input))
                    throw new ArgumentNullException("input");

                Item<string> root = new Item<string>();
                _parts = Regex.Split(input, string.Format(@"({0}|\{1}|\{2})", _delimiter, _left, _right))
                    .Where(s => Regex.IsMatch(s, string.Format(@"(\w+|\{0}|\{1})", _left, _right)))
                    .Select(s => s.Trim());
 

                if (!_parts.Any() )
                    throw new ApplicationException("No items found in the input string!");

                _enumerator = _parts.GetEnumerator();

                AddChildren(ref root);
                return root;
            }

            private void AddChildren(ref Item<string> item)
            {
                   
              while (_enumerator.MoveNext())
                {
                    if (IsLeft(_enumerator.Current))
                    {
                        if (item.Children.Any())
                         {
                            var lastChild =  item.Children.Last();
                            AddChildren(ref lastChild);
                        }
                        continue;
                    }

                    if (IsRight(_enumerator.Current))
                        break;
                    
                    Item<string> child = new Item<string>() { Me = _enumerator.Current };
                    item.Children.Add(child);
                                        
                }

            }

            private bool IsLeft(string testString)
            {
                return testString == _left;
            }

            private bool IsRight(string testString)
            {
                return testString == _right;
            }

        }

        #endregion


        #region enums

        public enum ItemDelimiters
        {
            Comma,
            Pipe
        }

        public enum NestingPairs
        {
            Braces,
            Brackets
        }


        #endregion
    }
}
