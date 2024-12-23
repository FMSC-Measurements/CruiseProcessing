using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CruiseProcessing.OutputModels
{
    // used to just be stored in an array, but created a class to make it more readable
    // but implemented IReadOnlyList to trasition
    public class HeaderFieldData : IReadOnlyList<string>
    {
        public string Date { get; set; }
        public string Version { get; set; }
        public string DllVersion { get; set; }
        public string CruiseName { get; set; }
        public string SaleName { get; set; }

        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => Date,
                    1 => Version,
                    2 => DllVersion,
                    3 => CruiseName,
                    4 => SaleName,
                    _ => throw new IndexOutOfRangeException(nameof(index)),
                };
            }
        }

        public int Count => 5;

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var i in Enumerable.Range(0, Count))
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}