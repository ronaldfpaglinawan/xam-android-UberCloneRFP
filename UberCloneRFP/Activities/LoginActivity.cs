using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Firebase.Database;
using Firebase;

namespace UberCloneRFP.Activities
{
    [Activity(Label = "@string/app_name", Theme ="@style/UberTheme", MainLauncher =true)]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);
        }
    }
}