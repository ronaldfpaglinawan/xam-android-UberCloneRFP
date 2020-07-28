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
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace UberCloneRFP.Helpers
{
    public static class AppDataHelper
    {
        static ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        public static FirebaseDatabase GetDatabase()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseDatabase database;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uberclone-6596f")
                    .SetApiKey("AIzaSyAXqLpF_JVINHqrS74X_r6uMG8wW5EZlzs")
                    .SetDatabaseUrl("https://uberclone-6596f.firebaseio.com")
                    .SetStorageBucket("uberclone-6596f.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }

            return database;
        }

        public static FirebaseAuth GetFirebaseAuth()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uberclone-6596f")
                    .SetApiKey("AIzaSyAXqLpF_JVINHqrS74X_r6uMG8wW5EZlzs")
                    .SetDatabaseUrl("https://uberclone-6596f.firebaseio.com")
                    .SetStorageBucket("uberclone-6596f.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
            }

            return mAuth;
        }

        public static FirebaseUser GetCurrentUser()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;
            FirebaseUser mUser;

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uberclone-6596f")
                    .SetApiKey("AIzaSyAXqLpF_JVINHqrS74X_r6uMG8wW5EZlzs")
                    .SetDatabaseUrl("https://uberclone-6596f.firebaseio.com")
                    .SetStorageBucket("uberclone-6596f.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }

            return mUser;
        }

        public static string GetFullName()
        {
            string fullname = preferences.GetString("fullname", "");
            return fullname;
        }

        public static string GetEmail()
        {
            string email = preferences.GetString("email", "");
            return email;
        }

        public static string GetPhone()
        {
            string phone = preferences.GetString("phone", "");
            return phone;
        }
    }
}