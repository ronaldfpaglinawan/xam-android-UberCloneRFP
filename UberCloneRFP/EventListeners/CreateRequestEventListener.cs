using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Java.Util;
using UberCloneRFP.DataModels;
using UberCloneRFP.Helpers;

namespace UberCloneRFP.EventListeners
{
    class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
    {
        NewTripDetails newTrip;
        FirebaseDatabase database;
        DatabaseReference newTripRef;
        public void OnCancelled(DatabaseError error)
        {
            throw new NotImplementedException();
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
        }

        public CreateRequestEventListener(NewTripDetails mNewTrip)
        {
            newTrip = mNewTrip;
            database = AppDataHelper.GetDatabase();
        }

        public void CreateRequest()
        {
            newTripRef = database.GetReference("riderequest").Push();

            HashMap location = new HashMap();
            location.Put("latitude", newTrip.PickupLat);
            location.Put("longitude", newTrip.PickupLng);

            HashMap destination = new HashMap();
            destination.Put("latitude", newTrip.PickupLat);
            destination.Put("longitude", newTrip.PickupLng);

            HashMap myTrip = new HashMap();

            newTrip.RideID = newTripRef.Key;
            myTrip.Put("location", location);
            myTrip.Put("destination", destination);
            myTrip.Put("destination_address", newTrip.DestinationAddress);
            myTrip.Put("pickup_address", newTrip.PickupAddress);
            myTrip.Put("rider_id", AppDataHelper.GetCurrentUser().Uid);
            myTrip.Put("payment_method", newTrip.PaymentMethod);
            myTrip.Put("created_at", newTrip.TimeStamp.ToString());
            myTrip.Put("driver_id", "waiting");
            myTrip.Put("rider_name", AppDataHelper.GetFullName());
            myTrip.Put("rider_phone", AppDataHelper.GetPhone());

            newTripRef.AddValueEventListener(this);
            newTripRef.SetValue(myTrip);
        }

        public void CancelRequest()
        {
            newTripRef.RemoveEventListener(this);
            newTripRef.RemoveValue();
        }
    }
}