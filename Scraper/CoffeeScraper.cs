using System.Collections.Generic;
using Entities;
using YelpSharp.Data;
using System.Data.Entity;
using System;

namespace Scraper
{
    public class CoffeeScraper
    {
        private const double MinLatitude = 47.40;
        private const double MaxLatitude = 47.85;

        private const double MinLongitude = -122.50;
        private const double MaxLongitude = -122.20;

        private readonly ISearcher _coffeeSearcher;

        public CoffeeScraper()
        {
             this._coffeeSearcher = new SplitRegionSearcher(new CoffeeSearcher());
        }

        public void Run()
        {
            CoffeeEntities entities = new CoffeeEntities();
            DbSet<CoffeeShop> coffeeShops = entities.CoffeeShops;
            this.ClearDatabase(coffeeShops);
            entities.SaveChanges();

            SearchArea searchArea = new SearchArea
            {
                SouthwestCorner = new Coordinate
                {
                    latitude = (double)MinLatitude,
                    longitude = (double)MinLongitude,
                },
                NortheastCorner = new Coordinate
                {
                    latitude = (double)MaxLatitude,
                    longitude = (double)MaxLongitude,
                }
            };

            SearchResult result = this._coffeeSearcher.Search(searchArea).Result;
            if (result.Error != null)
            {
                throw new Exception(result.Error.Description);
            }
            else
            {
                foreach (CoffeeShop shop in result.Results)
                {
                    coffeeShops.Add(shop);
                }

                entities.SaveChanges();
            }
        }

        private void ClearDatabase(DbSet<CoffeeShop> coffeeShops)
        {
            foreach (var shop in coffeeShops)
            {
                coffeeShops.Remove(shop);
            }
        }
    }
}
