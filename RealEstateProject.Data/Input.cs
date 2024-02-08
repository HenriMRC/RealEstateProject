using System;
using System.Xml.Serialization;

namespace RealEstateProject.Data;

[XmlRoot("inputs", IsNullable = false)]
public class Input
{
    [XmlElement("item")]
    public Item[] Items = Array.Empty<Item>();
}

public class Item
{
    [XmlAttribute("neighborhood")]
    public string Neighborhood = string.Empty;

    [XmlAttribute("zone")]
    public string Zone = string.Empty;

    [XmlAttribute("city")]
    public string City = string.Empty;

    [XmlAttribute("state")]
    public string State = string.Empty;

    [XmlAttribute("location_id")]
    public string LocationID = string.Empty;

    [XmlAttribute("latitude")]
    public string Latitude = string.Empty;

    [XmlAttribute("longitude")]
    public string Longitude = string.Empty;

    [XmlAttribute("levels")]
    public string Levels = string.Empty;

    [XmlElement("business")]
    public Business[] Business = Array.Empty<Business>();
}

public class Business
{
    [XmlAttribute("kind")]
    public UrlKind UrlKind;
}