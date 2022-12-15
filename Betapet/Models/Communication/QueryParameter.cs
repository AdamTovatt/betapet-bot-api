using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication
{
    public class QueryParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the paramter</param>
        public QueryParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
