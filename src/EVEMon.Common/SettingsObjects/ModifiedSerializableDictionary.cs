using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.SettingsObjects
{
    [Serializable]
    public sealed class ModifiedSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EVEMon.Common.SettingsObjects.SerializableDictionary{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        /// <remarks>Implemented to satisfy rule CA2229</remarks>
        private ModifiedSerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModifiedSerializableDictionary()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source"></param>
        public ModifiedSerializableDictionary(IDictionary<TKey, TValue> source)
            : base(source)
        {
        }

        /// <summary>
        /// This method is reserved and should not be used.
        /// When implementing the IXmlSerializable interface,
        /// you should return null (Nothing in Visual Basic) from this method,
        /// and instead, if specifying a custom schema is required,
        /// apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object
        /// that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method
        /// and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema() => null;

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        /// <exception cref="System.ArgumentNullException">reader</exception>
        public void ReadXml(XmlReader reader)
        {
            reader.ThrowIfNull(nameof(reader));

            if (reader.IsEmptyElement)
                return;

            reader.Read();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var value = default(TValue);

                // Does the element have attributes?
                if (reader.HasAttributes)
                {
                    var valueType = typeof(TValue);

                    // Does type have properties ?
                    if (valueType.GetProperties().Length == 0)
                    {
                        var converter = TypeDescriptor.GetConverter(valueType);

                        var attribute = reader.GetAttribute(0);

                        if (string.IsNullOrEmpty(attribute))
                            break;

                        // Assign the value
                        value = (TValue)converter.ConvertFromInvariantString(attribute);
                    }
                    else
                    {
                        // Assign the value
                        value = Activator.CreateInstance<TValue>();

                        // Assign values to the properties
                        foreach (var property in valueType.GetProperties().Where(
                            property => !Attribute.IsDefined(property, typeof(XmlIgnoreAttribute))))
                        {
                            var propertyName =
                                property.GetCustomAttributesData().First().ConstructorArguments.First().Value.ToString();

                            var converter = TypeDescriptor.GetConverter(property.PropertyType);

                            var attribute = reader.GetAttribute(propertyName);

                            if (string.IsNullOrEmpty(attribute))
                                break;

                            var propertyValue = converter.ConvertFromInvariantString(attribute);

                            property.SetValue(value, propertyValue, null);
                        }
                    }
                }

                reader.Read();

                var typeConverter = TypeDescriptor.GetConverter(typeof(TKey));

                var key = default(TKey);

                // Assign the key
                var keyValue = typeConverter.ConvertFrom(reader.Value);
                if (keyValue != null)
                    key = (TKey)keyValue;

                Add(key, value);

                reader.Read();
                reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.</param>
        /// <exception cref="System.ArgumentNullException">writer</exception>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public void WriteXml(XmlWriter writer)
        {
            writer.ThrowIfNull(nameof(writer));

            var keyType = typeof(TKey);
            var valueType = typeof(TValue);

            // Check that each type we use is serializable
            if (!keyType.IsSerializable)
                throw new ArgumentException($"{keyType} is not serializable", keyType.ToString());

            if (!valueType.IsSerializable)
                throw new ArgumentException($"{valueType} is not serializable", valueType.ToString());

            // Serialize each dictionary element as Xml
            foreach (var key in Keys)
            {
                var rootAttribute =
                    valueType.GetCustomAttributes(typeof(XmlRootAttribute), false).Cast<XmlRootAttribute>().FirstOrDefault();

                // Get the name specified in XmlRootAttribute or use the type name
                var elementName = rootAttribute != null && !string.IsNullOrWhiteSpace(rootAttribute.ElementName)
                    ? rootAttribute.ElementName
                    : TypeName;

                // Write as XmlElement
                writer.WriteStartElement(elementName);

                // Do we have properties ?
                if (valueType.GetProperties().Length == 0)
                {
                    var attributeName = TypeName;

                    // Write the value as XmlAttribute
                    writer.WriteAttributeString(attributeName, this[key].ToString());
                }
                else
                {
                    // Note: Best practice is to serialize properties instead of fields;
                    // however if you have to serialize fields use the 'GetFields()' method
                    // and adjust the code accordingly using the 'NonSerializedAttribute' 

                    // Write each property value as XmlAttribute excluding those that have the 'XmlIgnoreAttribute'
                    foreach (var property in valueType.GetProperties().Where(
                        property => !Attribute.IsDefined(property, typeof(XmlIgnoreAttribute))))
                    {
                        var attribute = property.GetCustomAttributes(false).Where(
                            x => x is XmlElementAttribute || x is XmlAttributeAttribute).Cast<Attribute>().FirstOrDefault();

                        var xmlElement = attribute as XmlElementAttribute;
                        var xmlAttribute = attribute as XmlAttributeAttribute;

                        // Get the name specified in XmlElement/XmlAttribute or use the property name
                        string attributeName;
                        if (xmlElement != null && !string.IsNullOrWhiteSpace(xmlElement.ElementName))
                            attributeName = xmlElement.ElementName;
                        else if (xmlAttribute != null && !string.IsNullOrWhiteSpace(xmlAttribute.AttributeName))
                            attributeName = xmlAttribute.AttributeName;
                        else
                            attributeName = property.Name;

                        var propertyValue = property.GetValue(this[key], null);

                        // Special condition for Boolean type (because "Boolean.ToString()" returns "True"/"False")
                        var propertyValueString = propertyValue is bool
                            ? XmlConvert.ToString((bool)propertyValue)
                            : propertyValue.ToString();

                        writer.WriteAttributeString(attributeName, propertyValueString);
                    }
                }

                // Write the key as XmlText
                writer.WriteValue(key.ToString());

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        private static string TypeName
        {
            get
            {
                var type = typeof(TValue);
                if (type.Namespace != "System")
                    return type.Name.ConvertUpperToLowerCamelCase();

                switch (type.Name)
                {
                    case "Int16":
                        return "short";
                    case "Int32":
                        return "int";
                    case "Int64":
                        return "long";
                    case "UInt16":
                        return "unsignedShort";
                    case "UInt32":
                        return "unsignedInt";
                    case "UInt64":
                        return "unsignedLong";
                    case "Single":
                        return "float";
                    case "DateTime":
                        return "dateTime";
                }
                return type.Name.ToLower(CultureConstants.InvariantCulture);
            }
        }
    }
}