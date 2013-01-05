using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper.Configuration
{
    public class YelpElement : ConfigurationElement
    {
        private const string ConsumerKeyXmlName = "consumerKey";
        private const string ConsumerSecretXmlName = "consumerSecret";
        private const string TokenXmlName = "token";
        private const string TokenSecretXmlName = "tokenSecret";

        [ConfigurationProperty(ConsumerKeyXmlName)]
        public string ConsumerKey
        {
            get { return (string)this[ConsumerKeyXmlName]; }
        }


        [ConfigurationProperty(ConsumerSecretXmlName)]
        public string ConsumerSecret
        {
            get { return (string)this[ConsumerSecretXmlName]; }
        }

        [ConfigurationProperty(TokenXmlName)]
        public string Token
        {
            get { return (string)this[TokenXmlName]; }
        }

        [ConfigurationProperty(TokenSecretXmlName)]
        public string TokenSecret
        {
            get { return (string)this[TokenSecretXmlName]; }
        }
    }
}
