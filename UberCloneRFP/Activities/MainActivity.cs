﻿using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Firebase.Database;
using Firebase;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Gms.Location;
using UberCloneRFP.Helpers;
using System;
using Google.Places;
using Xamarin.Essentials;
using Android.Graphics;
using System.Collections.Generic;
using Java.IO;
using Java.Lang.Reflect;
using Android.Content;
using Android.Support.Design.Widget;
using UberCloneRFP.EventListeners;
using UberCloneRFP.Fragments;
using UberCloneRFP.DataModels;

namespace UberCloneRFP
{
    [Activity(Label = "@string/app_name", Theme = "@style/UberTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        //Firebase
        UserProfileEventListener profileEventListener = new UserProfileEventListener();
        CreateRequestEventListener requestListener;

        //Views
        Android.Support.V7.Widget.Toolbar mainToolbar;
        Android.Support.V4.Widget.DrawerLayout drawerLayout;

        //TextViews
        TextView pickupLocationText;
        TextView destinationText;

        //Buttons
        Button favouritePlacesButton;
        Button locationSetButton;
        Button requestDriverButton;
        RadioButton pickupRadio;
        RadioButton destinationRadio;

        //ImageViews
        ImageView centerMarker;

        //Layouts
        RelativeLayout layoutPickup;
        RelativeLayout layoutDestination;

        //Bottomsheets
        BottomSheetBehavior tripDetailsBottomSheetBehavior;

        GoogleMap mainMap;

        readonly string[] permissionGroupLocation = { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };
        const int requestLocationId = 0;

        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationClient;
        Android.Locations.Location mLastLocation;
        LocationCallbackHelper mLocationCallback;

        static int UPDATE_INTERVAL = 5; //5 SECONDS
        static int FASTEST_INTERVAL = 5;
        static int DISPLACEMENT = 3; //meters

        //Helpers
        MapFunctionHelper mapHelper;

        //TripDetails
        LatLng pickupLocationLatLng;
        LatLng destinationLatLng;
        string pickupAddress;
        string destinationAddress;

        //Flags
        int addressRequest = 1;
        bool takeAddressFromSearch;

        //Fragments
        RequestDriver requestDriverFragment;

        //DataModels
        NewTripDetails newTripDetails;

        void ConnectControl()
        {
            //DrawerLayout
            drawerLayout = (Android.Support.V4.Widget.DrawerLayout)FindViewById(Resource.Id.drawerLayout);

            //Toolbar
            mainToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);

            //TextView
            pickupLocationText = (TextView)FindViewById(Resource.Id.pickupLocationText);
            destinationText = (TextView)FindViewById(Resource.Id.destinationText);

            //Buttons
            favouritePlacesButton = (Button)FindViewById(Resource.Id.favouritePlacesButton);
            locationSetButton = (Button)FindViewById(Resource.Id.locationSetButton);
            requestDriverButton = (Button)FindViewById(Resource.Id.requestDriverButton);
            pickupRadio = (RadioButton)FindViewById(Resource.Id.pickupRadio);
            destinationRadio = (RadioButton)FindViewById(Resource.Id.destinationRadio);
            favouritePlacesButton.Click += FavouritePlacesButton_Click;
            locationSetButton.Click += LocationSetButton_Click;
            requestDriverButton.Click += RequestDriverButton_Click;
            pickupRadio.Click += PickupRadio_Click;
            destinationRadio.Click += DestinationRadio_Click;

            //Layouts
            layoutPickup = (RelativeLayout)FindViewById(Resource.Id.layoutPickup);
            layoutDestination = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);

            layoutPickup.Click += LayoutPickup_Click;
            layoutDestination.Click += LayoutDestination_Click;

            //ImageViews
            centerMarker = (ImageView)FindViewById(Resource.Id.centerMarker);

            //Bottomsheets
            FrameLayout tripDetailsView = (FrameLayout)FindViewById(Resource.Id.tripdetails_bottomsheet);
            tripDetailsBottomSheetBehavior = BottomSheetBehavior.From(tripDetailsView);
        }


        #region CLICK EVENT HANDLERS
        private void RequestDriverButton_Click(object sender, EventArgs e)
        {
            requestDriverFragment = new RequestDriver(mapHelper.EstimateFares());
            requestDriverFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            requestDriverFragment.Show(trans, "Request");
            requestDriverFragment.CancelRequest += RequestDriverFragment_CancelRequest;

            newTripDetails = new NewTripDetails();
            newTripDetails.DestinationAddress = destinationAddress;
            newTripDetails.PickupAddress = pickupAddress;
            newTripDetails.DestinationLat = destinationLatLng.Latitude;
            newTripDetails.DestinationLng = destinationLatLng.Longitude;
            newTripDetails.DistanceString = mapHelper.distanceString;
            newTripDetails.DistanceValue = mapHelper.distance;
            newTripDetails.DurationString = mapHelper.durationString;
            newTripDetails.DurationValue = mapHelper.duration;
            newTripDetails.EstimateFare = mapHelper.EstimateFares();
            newTripDetails.PaymentMethod = "cash";
            newTripDetails.PickupLat = pickupLocationLatLng.Latitude;
            newTripDetails.PickupLng = pickupLocationLatLng.Longitude;
            newTripDetails.TimeStamp = DateTime.Now;

            requestListener = new CreateRequestEventListener(newTripDetails);
            requestListener.CreateRequest();

        }

        private void RequestDriverFragment_CancelRequest(object sender, EventArgs e)
        {
            //User cancels request before driver accepts it
            if(requestDriverFragment != null && requestListener != null)
            {
                requestListener.CancelRequest();
                requestListener = null;
                requestDriverFragment.Dismiss();
                requestDriverFragment = null;
            }
        }

        async void LocationSetButton_Click(object sender, EventArgs e)
        {
            locationSetButton.Text = "Please wait...";
            locationSetButton.Enabled = false;

            string json;
            json = await mapHelper.GetDirectionJsonAsync(pickupLocationLatLng, destinationLatLng);

            if (!string.IsNullOrEmpty(json))
            {
                TextView txtFare = (TextView)FindViewById(Resource.Id.tripEstimateFareText);
                TextView txtTime = (TextView)FindViewById(Resource.Id.newTripTimeText);

                mapHelper.DrawTripMap(json);

                // Set Estimate Fares and Time
                txtFare.Text = "$" + mapHelper.EstimateFares().ToString() + "_" + (mapHelper.EstimateFares() + 20).ToString();
                txtTime.Text = mapHelper.durationString;

                // Display BottomSheet
                tripDetailsBottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
                
                //Disable Views
                TripDrawnOnMap();
            }

            locationSetButton.Text = "Done";
            locationSetButton.Enabled = true;
        }

        private void FavouritePlacesButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }

        }

        void PickupRadio_Click(object sender, System.EventArgs e)
        {
            addressRequest = 1;
            pickupRadio.Checked = true;
            destinationRadio.Checked = false;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.DarkGreen);
        }

        void DestinationRadio_Click(object sender, System.EventArgs e)
        {
            addressRequest = 2;
            destinationRadio.Checked = true;
            pickupRadio.Checked = false;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.Red);
        }

        void LayoutPickup_Click(object sender, System.EventArgs e)
        {
            //AutocompleteFilter filter = new AutocompleteFilter.Builder()
            //    .SetCountry("NZ")
            //    .Build();

            //Intent intent = new PlaceAutoComplete.IntentBuilder(PlaceAutoComplete.ModeOverlay)
            //    .SetFilter(filter)
            //    .Build(this);

            //StartActivityForResult(intent, 1);


            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("NZ")
                .Build(this);

            StartActivityForResult(intent, 1);
        }

        void LayoutDestination_Click(object sender, System.EventArgs e)
        {
            //AutocompleteFilter filter = new AutocompleteFilter.Builder()
            //    .SetCountry("NZ")
            //    .Build();

            //Intent intent = new PlaceAutoComplete.IntentBuilder(PlaceAutoComplete.ModeOverlay)
            //    .SetFilter(filter)
            //    .Build(this);

            //StartActivityForResult(intent, 2);


            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("NZ")
                .Build(this);

            StartActivityForResult(intent, 2);
        }
        #endregion


        #region MAP AND LOCATION SERVICES
        void InitializePlaces()
        {
            string mapKey = Resources.GetString(Resource.String.mapkey);
            if (PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, mapKey);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                // to set a customize map style
                //bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.silvermapstyle));
            }
            catch
            {

            }

            mainMap = googleMap;
            mainMap.CameraIdle += MainMap_CameraIdle;
            string mapkey = Resources.GetString(Resource.String.mapkey);
            mapHelper = new MapFunctionHelper(mapkey, mainMap);
        }

        private async void MainMap_CameraIdle(object sender, EventArgs e)
        {
            if (!takeAddressFromSearch)
            {
                if (addressRequest == 1)
                {
                    pickupLocationLatLng = mainMap.CameraPosition.Target;
                    pickupAddress = await mapHelper.FindCoordinateAddress(pickupLocationLatLng);
                    pickupLocationText.Text = pickupAddress;
                }
                else if (addressRequest == 2)
                {
                    destinationLatLng = mainMap.CameraPosition.Target;
                    destinationAddress = await mapHelper.FindCoordinateAddress(destinationLatLng);
                    destinationText.Text = destinationAddress;
                    TripLocationsSet();
                }
            }
        }

        bool CheckLocationPermission()
        {
            bool permissionGranted = false;

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = LocationServices.GetFusedLocationProviderClient(this);
            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
        }

        void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17));
        }

        void StartLocationUpdates()
        {
            if (CheckLocationPermission())
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
            }
        }

        void StopLocationUpdates()
        {
            if (locationClient != null && mLocationCallback != null)
            {
                locationClient.RemoveLocationUpdates(mLocationCallback);
            }
        }

        async void GetMyLocation()
        {
            if (!CheckLocationPermission())
            {
                return;
            }

            mLastLocation = await locationClient.GetLastLocationAsync();

            if (mLastLocation != null)
            {
                LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17));
            }
        }
        #endregion


        #region TRIP CONFIGURATION
        void TripLocationsSet()
        {
            favouritePlacesButton.Visibility = ViewStates.Invisible;
            locationSetButton.Visibility = ViewStates.Visible;
        }

        void TripDrawnOnMap()
        {
            layoutDestination.Clickable = false;
            layoutPickup.Clickable = false;
            pickupRadio.Enabled = false;
            destinationRadio.Enabled = false;
            takeAddressFromSearch = true;
            centerMarker.Visibility = ViewStates.Invisible;
        }
        #endregion


        #region OVERRIDE METHODS
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ConnectControl();

            SupportMapFragment mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();
            InitializePlaces();
            profileEventListener.Create();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults.Length < 1)
            {
                return;
            }

            if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                StartLocationUpdates();
            }
            else
            {
                Toast.MakeText(this, "Permission was denied", ToastLength.Short).Show();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    System.Console.WriteLine($"requestCode is {requestCode}");
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    pickupLocationText.Text = place.Name.ToString();
                    pickupAddress = place.Name.ToString();
                    pickupLocationLatLng = place.LatLng;
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.DarkGreen);
                }
            }

            if (requestCode == 2)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    System.Console.WriteLine($"requestCode is {requestCode}");
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    destinationText.Text = place.Name.ToString();
                    destinationAddress = place.Name.ToString();
                    destinationLatLng = place.LatLng;
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.Red);
                    TripLocationsSet();
                }
            }
        }
        #endregion
    }
}