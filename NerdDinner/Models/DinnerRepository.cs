using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;
using System.Data;

namespace NerdDinner.Models
{

    public class DinnerRepository : NerdDinner.Models.IDinnerRepository
    {

        NerdDinnerEntities db = new NerdDinnerEntities();

        //
        // Query Methods

        public IQueryable<Dinner> FindDinnersByText(string q)
        {
        	var dinners = db.Dinners;
        	var textDinners = new List<Dinner>();

        	foreach (var dinner in dinners)
        	{
        		if (dinner.Title.Contains(q) || dinner.Description.Contains(q) || dinner.HostedBy.Contains(q))
        		{
        			textDinners.Add(dinner);
        		}
        	}

        	return textDinners.AsQueryable();

			/* Original */
			//return db.Dinners.Where(d => d.Title.Contains(q)
			//                || d.Description.Contains(q)
			//                || d.HostedBy.Contains(q));
        }

        public IQueryable<Dinner> FindAllDinners()
        {
            return db.Dinners;
        }

        public IQueryable<Dinner> FindUpcomingDinners()
        {
        	var dinners = FindAllDinners();
        	var upcomingDinners = new List<Dinner>();

        	foreach (var dinner in dinners)
        	{
        		if (dinner.EventDate >= DateTime.Now)
					upcomingDinners.Add(dinner);
        	}

        	return upcomingDinners.OrderBy(d => d.EventDate).AsQueryable();

			/* Original */
			//return from dinner in FindAllDinners()
			//       where dinner.EventDate >= DateTime.Now
			//       orderby dinner.EventDate
			//       select dinner;
        }

        public IQueryable<Dinner> FindByLocation(float latitude, float longitude)
        {
        	var dinners = FindUpcomingDinners();
        	var localDinners = new List<Dinner>();

        	foreach (var dinner in dinners)
        	{
        		localDinners.AddRange(NearestDinners(dinner.Latitude, dinner.Longitude).ToList());
        	}

			return localDinners.AsQueryable();

			//var dinners = from dinner in FindUpcomingDinners()
			//              join i in NearestDinners(latitude, longitude)
			//              on dinner.DinnerID equals i.DinnerID
			//              select dinner;
        }

        public Dinner GetDinner(int id)
        {
            return db.Dinners.SingleOrDefault(d => d.DinnerID == id);
        }

        //
        // Insert/Delete Methods

        public void Add(Dinner dinner)
        {
            db.Dinners.AddObject(dinner);
        }

        public void Delete(Dinner dinner)
        {
            foreach (RSVP rsvp in dinner.RSVPs.ToList())
                db.RSVPs.DeleteObject(rsvp);
            db.Dinners.DeleteObject(dinner);
        }

        //
        // Persistence 

        public void Save()
        {
            db.SaveChanges();
        }


        // Helper Methods

        [EdmFunction("NerdDinnerModel.Store", "DistanceBetween")]
        public static double DistanceBetween(double lat1, double long1, double lat2, double long2)
        {
            throw new NotImplementedException("Only call through LINQ expression");
        }

        public IQueryable<Dinner> NearestDinners(double latitude, double longitude)
        {
        	var dinners = db.Dinners;
        	var nearestDinners = new List<Dinner>();
        	var thePoint = new GeoCoordinate(latitude, longitude);

        	foreach (var dinner in dinners)
        	{
				if (DistanceBetweenPoints(thePoint, new GeoCoordinate(dinner.Latitude, dinner.Longitude)) < 100)
					nearestDinners.Add(dinner);
        	}

        	return nearestDinners.AsQueryable();

			/* Original */
        	//return from d in db.Dinners
        	//       where DistanceBetween(latitude, longitude, d.Latitude, d.Longitude) < 100
        	//       select d;
        }

		private Double DistanceBetweenPoints(GeoCoordinate source, GeoCoordinate target)
		{
			var distanceInMeters = source.GetDistanceTo(target);
			var distanceInMiles = (distanceInMeters / 1000) * 0.621371192;

			return distanceInMiles;
		}
    }
}
