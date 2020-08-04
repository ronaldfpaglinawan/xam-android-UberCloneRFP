using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace UberCloneRFP.Fragments
{
    public class RequestDriver : Android.Support.V4.App.DialogFragment
    {
        double mfares;
        Button cancelRequestButton;
        TextView faresText;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.request_driver, container, false);
            cancelRequestButton = (Button)view.FindViewById(Resource.Id.cancelrequestButton);
            faresText = (TextView)view.FindViewById(Resource.Id.faresText);
            faresText.Text = "$" + mfares.ToString();
            return view;
        }

        public RequestDriver(double fares)
        {
            mfares = fares;


        }
    }
}