using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper.Configuration
{
    public class ApiKeySection : ConfigurationSection
    {
        public const string SectionName = "apikeys";

        private const string YelpXmlName = "yelp";

        [ConfigurationProperty(YelpXmlName)]
        public YelpElement Yelp
        {
            get { return (YelpElement)this[YelpXmlName]; }
        }
    }
}
