﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinate()
        {

        }

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}