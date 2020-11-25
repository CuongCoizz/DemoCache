using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

public static class VietNamMapCommon
{
    public static ResponseModel LoadDataForVNMap(out JObject result)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\GeoData\\a0_diaphantinh_2019042412504646.xml");

        var coordinates = new Dictionary<string, MultiPolygon>();
        string currentName = string.Empty;
        Polygon currentPolygon = new Polygon();
        Coordinate currentCoordinate = new Coordinate();
        ElementType currentType = ElementType.None;

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.Async = false;
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "IsGroup:tenDoiTuong":
                                    currentType = ElementType.DoiTuong;
                                    break;
                                case "gml:Polygon":
                                    currentType = ElementType.Polygon;
                                    break;
                                case "gml:X":
                                    currentType = ElementType.X;
                                    break;
                                case "gml:Y":
                                    currentType = ElementType.Y;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                            switch (currentType)
                            {
                                case ElementType.DoiTuong:
                                    currentName = reader.Value;
                                    coordinates.Add(currentName, new MultiPolygon());
                                    break;
                                case ElementType.X:
                                    currentCoordinate.Longitude = reader.Value.ToDecimal() ?? 0;
                                    break;
                                case ElementType.Y:
                                    currentCoordinate.Latitude = reader.Value.ToDecimal() ?? 0;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            switch (reader.Name)
                            {
                                case "gml:Polygon":
                                    coordinates[currentName].Polygons.Add(currentPolygon);
                                    currentPolygon = new Polygon();
                                    break;
                                case "gml:Y":
                                    currentPolygon.Points.Add(currentCoordinate);
                                    currentCoordinate = new Coordinate();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        result = ToJSON(coordinates);

        return ResponseModel.Success();
    }
    public static decimal? ToDecimal(this string val)
    {
        decimal rt;
        if (decimal.TryParse(val, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out rt))
            return rt;

        return null;
    }
    public static ResponseModel GetInformationNhaMay()
    {

        return ResponseModel.Success();
    }
    private static JObject ToJSON(Dictionary<string, MultiPolygon> geoData)
    {
        JObject output = new JObject();
        output.Add("type", "FeatureCollection");
        JArray features = new JArray();
        int id = 0;
        foreach (var name in geoData.Keys)
        {
            MultiPolygon multiPolygon = geoData[name];

            JObject item = new JObject();
            item.Add("type", "Feature");
            item.Add("id", ++id);
            JObject properties = new JObject();
            properties.Add("name", name);
            item.Add("properties", properties);
            JObject geometry = new JObject();
            if (multiPolygon.Polygons.Count == 1)
            {
                geometry.Add("type", "Polygon");
            }
            else
            {
                geometry.Add("type", "MultiPolygon");
            }

            if (multiPolygon.Polygons.Count == 1)
            {
                var listOfPoints = multiPolygon.Polygons[0].Points
                    .Select(x => new JArray(x.Longitude, x.Latitude))
                    .ToList();
                var nestedJArray = new JArray();
                foreach (var p in listOfPoints)
                {
                    nestedJArray.Add(p);
                }
                JArray coordinates = new JArray();
                coordinates.Add(nestedJArray);
                geometry.Add("coordinates", coordinates);
            }
            else
            {
                JArray coordinates = new JArray();
                JArray polygons = new JArray();

                foreach (var poly in multiPolygon.Polygons)
                {
                    var listOfPoints = poly.Points
                        .Select(x => new JArray(x.Longitude, x.Latitude))
                        .ToList();
                    var nestedJArray = new JArray();
                    foreach (var p in listOfPoints)
                    {
                        nestedJArray.Add(p);
                    }

                    polygons.Add(nestedJArray);
                }

                coordinates.Add(polygons);
                geometry.Add("coordinates", coordinates);
            }
            item.Add("geometry", geometry);

            features.Add(item);
        }
        output.Add("features", features);

        return output;
    }
}
internal class Coordinate
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public decimal[] ToArray()
    {
        return new decimal[2] { Longitude, Latitude };
    }

    public Coordinate() { }
    public Coordinate(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

internal class Polygon
{
    public List<Coordinate> Points { get; }
    public Polygon()
    {
        Points = new List<Coordinate>();
    }

    public Polygon(List<Coordinate> coordinates)
    {
        Points = new List<Coordinate>();
        Points.AddRange(coordinates);
    }
}

internal class MultiPolygon
{
    public List<Polygon> Polygons { get; set; }

    public MultiPolygon()
    {
        Polygons = new List<Polygon>();
    }
}
public enum ElementType
{
    None = 0,
    DoiTuong = 1,
    Polygon = 2,
    X = 3,
    Y = 4
}
public class ResponseModel
{
    public ResponseType Type { get; set; }
    public string Message { get; set; }
    public string Description { get; set; }

    public ResponseModel() { }

    public ResponseModel(ResponseType type, string message)
    {
        Type = type;
        Message = message;
    }

    public ResponseModel(ResponseType type, string message, string description)
    {
        Type = type;
        Message = message;
        Description = description;
    }

    public static ResponseModel Success()
    {
        return new ResponseModel(ResponseType.Success, string.Empty);
    }
}

public enum ResponseType
{
    Success,
    Warning,
    UserError,
    DataError,
    ConnectionError,
    FunctionError
}