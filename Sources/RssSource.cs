using System;
using System.Collections.Generic;
using System.Xml;

namespace SG.Core.Sources
{
    /// <summary>Reads an RSS 2.0 feed and maps each &lt;item&gt; into T.</summary>
    public sealed class RssSource<T> : ITableSource<T>
    {
        private readonly string _url;
        private readonly Func<RssItem, T> _map;

        public RssSource(string url, Func<RssItem, T> map)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("url required", "url");
            if (map == null) throw new ArgumentNullException("map");
            _url = url; _map = map;
        }

        public IList<T> Fetch()
        {
            var result = new List<T>();
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            var doc = new XmlDocument { XmlResolver = null };

            using (XmlReader reader = XmlReader.Create(_url, settings))
                doc.Load(reader);

            foreach (XmlNode node in doc.SelectNodes("//item"))
                result.Add(_map(new RssItem(node)));
            return result;
        }
    }

    /// <summary>Null-safe view over one feed item.</summary>
    public sealed class RssItem
    {
        private readonly XmlNode _node;
        internal RssItem(XmlNode node) { _node = node; }

        public string Child(string name)
        {
            XmlNode n = _node.SelectSingleNode(name);
            return n == null ? string.Empty : n.InnerText;
        }

        public string Title { get { return Child("title"); } }
        public string Description { get { return Child("description"); } }
        public string Link { get { return Child("link"); } }
    }
}
