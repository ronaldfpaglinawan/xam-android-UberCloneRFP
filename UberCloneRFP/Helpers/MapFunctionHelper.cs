using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;
using ufinix.Helpers;
using yucee.Helpers;

namespace UberCloneRFP.Helpers
{
    public class MapFunctionHelper
    {
        string mapkey;
        GoogleMap map;
        public double distance;
        public double duration;
        public string distanceString;
        public string durationString;

        public MapFunctionHelper(string mMapKey, GoogleMap mmap)
        {
            mapkey = mMapKey;
            map = mmap;
        }

        public string GetGeoCodeUrl(double lat, double lng)
        {
            string url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + mapkey;
            return url;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);
            return result;
        }

        public async Task<string> FindCoordinateAddress(LatLng position)
        {
            var currentCulture = CultureInfo.DefaultThreadCurrentCulture;
            CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = currentCulture;

            string url = GetGeoCodeUrl(position.Latitude, position.Longitude);
            string json = "";
            string placeAddress = "";

            //Check for internet connection
            json = await GetGeoJsonAsync(url);

            if(!string.IsNullOrEmpty(json))
            {
                var geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);
                if(!geoCodeData.status.Contains("ZERO"))
                {
                    if(geoCodeData.results[0] != null)
                    {
                        placeAddress = geoCodeData.results[0].formatted_address;
                    }
                }
            }

            return placeAddress;
        }

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
            //Origin of route
            string str_origin = "origin=" + location.Latitude + "," + location.Longitude;

            //Destination of route
            string str_destination = "destination=" + destination.Latitude + "," + destination.Longitude;

            //mode
            string mode = "mode=driving";

            //Building the parameters to the webservice
            string parameters = str_origin + "&" + str_destination + "&" + "&" + mode + "&key=";

            //Output format
            string output = "json";

            string key = mapkey;

            //Building the final url string
            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + parameters + key;

            string json = "";
            json = await GetGeoJsonAsync(url);

            return json;
        }

        public void DrawTripMap(string json)
        {
            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            //Decode Encoded Route
            var points = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(points);

            ArrayList routeList = new ArrayList();
            foreach (LatLng item in line)
            {
                routeList.Add(item);
            }

            //Draw Polylines on Map
            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(10)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            Android.Gms.Maps.Model.Polyline mPolyline = map.AddPolyline(polylineOptions);

            //Get the first point and last point
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[line.Count - 1];

            //Pickup marker options
            MarkerOptions pickupMarkerOptions = new MarkerOptions();
            pickupMarkerOptions.SetPosition(firstpoint);
            pickupMarkerOptions.SetTitle("Pickup Location");
            pickupMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

            //Destination marker options
            MarkerOptions destinationMarkerOptions = new MarkerOptions();
            destinationMarkerOptions.SetPosition(lastpoint);
            destinationMarkerOptions.SetTitle("Destination");
            destinationMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

            Marker pickupMarker = map.AddMarker(pickupMarkerOptions);
            Marker destinationMarker = map.AddMarker(destinationMarkerOptions);

            //Get Trip Bounds
            double southlng = directionData.routes[0].bounds.southwest.lng;
            double southlat = directionData.routes[0].bounds.southwest.lat;
            double northlng = directionData.routes[0].bounds.northeast.lng;
            double northlat = directionData.routes[0].bounds.northeast.lat;

            LatLng southwest = new LatLng(southlat, southlng);
            LatLng northeast = new LatLng(northlat, northlng);
            LatLngBounds tripBound = new LatLngBounds(southwest, northeast);

            //map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(tripBound, 470)); //causes crashing the app
            map.SetPadding(40, 70, 40, 70);
            pickupMarker.ShowInfoWindow();
            destinationMarker.ShowInfoWindow();

            duration = directionData.routes[0].legs[0].duration.value;
            distance = directionData.routes[0].legs[0].distance.value;
            durationString = directionData.routes[0].legs[0].duration.text;
            distanceString = directionData.routes[0].legs[0].distance.text;
        }

        public double EstimateFares()
        {
            double baseFare = 20; //USD
            double distanceFare = 5; //USD
            double timeFare = 3; //USD

            double kmFares = (distance / 1000) * distanceFare;
            double minsFares = (duration / 60) * timeFare;

            double amount = kmFares + minsFares + baseFare;
            double fares = Math.Floor(amount / 10) * 10;

            return fares;
        }
    }
}