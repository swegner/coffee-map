using Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SvgGenerator
{
    internal class Program
    {
        private const int Radius = 2;
        private const double DistanceMultiplier = 2500;
        private const string Color = "black";
        private const string FileName = "coffee-map.svg";

        public static void Main()
        {
            XNamespace svgNamespace = "http://www.w3.org/2000/svg";

            IEnumerable<XElement> elements;
            using (CoffeeEntities entities = new CoffeeEntities())
            {
                IEnumerable<CoffeeShop> coffeeShops = entities.CoffeeShops;
                    
                double maxLatitude = coffeeShops.Max(cs => cs.Location.Latitude);
                double minLongitude = coffeeShops.Min(cs => cs.Location.Longitude);

                var coordinates = coffeeShops
                    .OrderBy(cs => cs.Location.Latitude)
                    .ThenBy(cs => cs.Location.Longitude)
                    .Select(cs => new
                    {
                        X = (cs.Location.Longitude - minLongitude) * DistanceMultiplier,
                        Y = (cs.Location.Latitude - maxLatitude) * -1 * DistanceMultiplier,
                    })
                    .ToList();

                elements = coordinates
                    .Select(c => new XElement(svgNamespace + "circle",
                        new XAttribute("cx", c.X),
                        new XAttribute("cy", c.Y),
                        new XAttribute("r", Radius),
                        new XAttribute("fill", Color)));
            }

            XObject[] childObjects = new XObject[] { new XAttribute("version", "1.1") }
                .Concat(elements)
                .ToArray();

            XDocument xDoc = new XDocument(
                new XDeclaration("1.0", null, "no"),
                new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
                new XElement(svgNamespace + "svg", elements));

            using (XmlWriter writer = XmlWriter.Create(FileName, new XmlWriterSettings 
            {
                Indent = true,
            }))
            {
                xDoc.Save(writer);
            }
        }
    }
}
