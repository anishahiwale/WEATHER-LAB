using System;
using System.Linq;


namespace WeatherLab
{
    class Program
    {
        static string dbfile = @".\data\climate.db";

        static void Main(string[] args)
        {
            var measurements = new WeatherSqliteContext(dbfile).Weather;

            var precipitaion_samples_2020 = from weatherDataItem in measurements
                                            where weatherDataItem.year == 2020
                                            select (weatherDataItem.precipitation);

            var total_2020_precipitation = precipitaion_samples_2020.Sum();
            Console.WriteLine($"Total precipitation in 2020: {total_2020_precipitation} mm\n");
            /*foreach(var measurement in measurements)
            {
                Console.WriteLine($"{measurement.day}");
            }*/
            //
            // Heating Degree days have a mean temp of < 18C
            //   see: https://en.wikipedia.org/wiki/Heating_degree_day
            //

            var hddItems = from weatherDataItem in measurements
                           where weatherDataItem.meantemp < 18
                           group weatherDataItem by weatherDataItem.year into weatherDataGroup
                           select new { year = weatherDataGroup.Key, count = weatherDataGroup.Count() };

            //
            // Cooling degree days have a mean temp of >=18C
            //

            var cddItems = from weatherDataItem in measurements
                           where weatherDataItem.meantemp >= 18
                           group weatherDataItem by weatherDataItem.year into weatherDataGroup
                           select new { year = weatherDataGroup.Key, count = weatherDataGroup.Count() };

            //
            // Most Variable days are the days with the biggest temperature
            // range. That is, the largest difference between the maximum and
            // minimum temperature
            //
            // Oh: and number formatting to zero pad.
            // 
            // For example, if you want:
            //      var x = 2;
            // To display as "0002" then:
            //      $"{x:d4}"
            //

            Console.WriteLine("Year\tHDD\tCDD");

            var result = from cddDataItem in cddItems
                         join hddDataItem in hddItems on cddDataItem.year equals hddDataItem.year
                         orderby hddDataItem.year
                         select new { year = hddDataItem.year, hdd = hddDataItem.count, cdd = cddDataItem.count };
            foreach (var row in result)
            {
                Console.WriteLine($"{row.year:d4}\t{row.hdd:d4}\t{row.cdd:d4}");
            }

            Console.WriteLine("\nTop 5 Most Variable Days");
            Console.WriteLine("YYYY-MM-DD\tDelta");

            var daysOrderedByVariation = from weatherDataItem in measurements
                                   orderby weatherDataItem.maxtemp - weatherDataItem.mintemp descending
                                   select new { year = weatherDataItem.year, month = weatherDataItem.month, day = weatherDataItem.day, delta = weatherDataItem.maxtemp - weatherDataItem.mintemp } ;
             foreach(var day in daysOrderedByVariation.Take(5))
            {
                Console.WriteLine($"{day.year:d4}-{day.month:d2}-{day.day:d2}\t{day.delta:0.00}");
            }                      
        }
    }
}
