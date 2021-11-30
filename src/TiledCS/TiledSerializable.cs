using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO.Compression;
using System.Globalization;

namespace TiledCS
{
    /// <summary>
    /// Represents the base class of any object that can be serialized to XML
    /// </summary>
    public abstract class TiledSerializable
    {
        /// <summary>
        /// Initializes an array of type <typeparamref name="T"/> from XML.
        /// </summary>
        /// <typeparam name="T">The type to initialize.</typeparam>
        /// <param name="nodeList">The xml source.</param>
        /// <returns>The initialized array.</returns>
        protected static T[] TryCreateFrom<T>(XmlNodeList nodeList)
            where T : TiledSerializable, new()
        {
            if (nodeList == null) return null; // it could be replaced by Array.Empty<T>();

            return nodeList
                .Cast<XmlNode>()
                .Select(TryCreateFrom<T>)
                .ToArray();
        }

        /// <summary>
        /// Initializes an array of type <typeparamref name="T"/> from XML.
        /// </summary>
        /// <typeparam name="T">The type to initialize.</typeparam>
        /// <param name="nodeList">The xml source.</param>
        /// <param name="activator">function that creates the item.</param>
        /// <returns>The initialized array.</returns>
        protected static T[] TryCreateFrom<T>(XmlNodeList nodeList, Func<XmlNode,T> activator)
            where T : TiledSerializable
        {
            if (nodeList == null) return null; // it could be replaced by Array.Empty<T>();

            return nodeList
                .Cast<XmlNode>()
                .Select(activator)
                .ToArray();
        }

        /// <summary>
        /// Initializes an instance of type <typeparamref name="T"/> from XML.
        /// </summary>
        /// <typeparam name="T">The type to initialize.</typeparam>
        /// <param name="node">The xml source.</param>
        /// <returns>The initialized object.</returns>
        protected static T TryCreateFrom<T>(XmlNode node)
            where T:TiledSerializable, new()
        {
            if (node == null) return null;
            var item = new T();
            item.ParseXml(node);
            return item;
        }

        /// <summary>
        /// Reads the contents of the current object from xml.
        /// </summary>
        /// <param name="node">The xml element from where to read the content.</param>
        protected abstract void ParseXml(XmlNode node);
    }

    

}
