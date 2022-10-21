using System.Collections;
using System.Collections.Generic;

namespace SaveTools
{
    public class FilePath : IEnumerable
    {
        private List<string> hierarchy = new();
        private List<string> partialPaths = new();

        public string this[int i] => hierarchy[i];
        public int Length => hierarchy.Count;
        public string End => hierarchy[^1];

        public FilePath(string path)
        {
            string[] pathElements = path.Split("/");
            string partialPath = "";

            for (int i = 0; i < pathElements.Length; i++)
            {
                partialPath += pathElements[i];

                hierarchy.Add(pathElements[i]);
                partialPaths.Add(partialPath);

                if (i < pathElements.Length - 1)
                    partialPath += "/";
            }
        }

        public FilePath CreateExtendedPath(string addedElement)
        {
            FilePath other = new(ToString());
            other.Append(addedElement);

            return other;
        }

        public string GetPartialPath(int i) => partialPaths[i];

        public void Append(string elementName)
        {
            hierarchy.Add(elementName);
            partialPaths.Add(partialPaths[^1] + "/" + elementName);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)hierarchy).GetEnumerator();
        }

        public override string ToString() => partialPaths[^1];
    }
}
