using System;
using System.Collections.Generic;
using System.Text;

namespace CoreConsoleApp1
{
    internal class StructValue : IDictionary<string, object>
    {
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return m_values.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return m_values.GetEnumerator(); }
        public bool IsReadOnly { get { return true; } }
        public object this[string key]
        {
            get
            {
                foreach (var keyValue in m_values)
                {
                    if (key == keyValue.Key)
                    {
                        return keyValue.Value;
                    }
                }

                return null;
            }
            set { throw new NotImplementedException(); }
        }
        public bool TryGetValue(string key, out object value)
        {
            foreach (var keyValue in m_values)
            {
                if (key == keyValue.Key)
                {
                    value = keyValue.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }
        public void Add(string key, object value) { m_values.Add(new KeyValuePair<string, object>(key, value)); }
        public void Add(KeyValuePair<string, object> item) { m_values.Add(item); }
        public void Clear() { m_values.Clear(); }
        public int Count { get { return m_values.Count; } }
        public override string ToString()
        {
            return WriteAsJSon(new StringBuilder(), this).ToString();
        }

        private static StringBuilder WriteAsJSon(StringBuilder sb, object value)
        {
            var asStructValue = value as StructValue;
            if (asStructValue != null)
            {
                sb.Append("{ ");
                bool first = true;
                foreach (var keyvalue in asStructValue)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    sb.Append(keyvalue.Key).Append(":");
                    WriteAsJSon(sb, keyvalue.Value);
                }
                sb.Append(" }");
                return sb;
            }

            var asArray = value as System.Array;
            if (asArray != null && asArray.Rank == 1)
            {
                sb.Append("[ ");
                bool first = true;
                for (int i = 0; i < asArray.Length; i++)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    WriteAsJSon(sb, asArray.GetValue(i));
                }
                sb.Append(" ]");
                return sb;
            }

            if (value is int || value is bool || value is double || value is float)
            {
                sb.Append(value);
                return sb;
            }
            else if (value == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.Append("\"");
                Quote(sb, value.ToString());
                sb.Append("\"");
            }
            return sb;
        }

        #region private
        /// <summary>
        ///  Uses C style conventions to quote a string 'value' and append to the string builder 'sb'.
        ///  Thus all \ are turned into \\ and all " into \"
        /// </summary>
        private static void Quote(StringBuilder output, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '\\' || c == '"')
                {
                    output.Append('\\');
                }

                output.Append(c);
            }
        }

        public bool ContainsKey(string key)
        {
            object value;
            return TryGetValue(key, out value);
        }
        public ICollection<string> Keys { get { throw new NotImplementedException(); } }
        public bool Remove(string key) { throw new NotImplementedException(); }
        public ICollection<object> Values { get { throw new NotImplementedException(); } }

        public bool Contains(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { throw new NotImplementedException(); }
        public bool Remove(KeyValuePair<string, object> item) { throw new NotImplementedException(); }

        private List<KeyValuePair<string, object>> m_values = new List<KeyValuePair<string, object>>();
        #endregion
    }
}
