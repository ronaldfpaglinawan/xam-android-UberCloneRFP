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
using Android.Support.Design.Widget;
using Firebase.Auth;
using UberCloneRFP.EventListeners;

namespace UberCloneRFP.Activities
{
    [Activity(Label = "@string/app_name", Theme ="@style/UberTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity
    {
        TextInputLayout emailText;
        TextInputLayout passwordText;
        Button loginButton;
        CoordinatorLayout rootView;
        TextView clickToRegisterText;
        FirebaseAuth mAuth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);

            emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            loginButton = (Button)FindViewById(Resource.Id.loginButton);
            clickToRegisterText = (TextView)FindViewById(Resource.Id.clickToRegister);

            clickToRegisterText.Click += ClickToRegisterText_Click;
            loginButton.Click += LoginButton_Click;
            InitialFirebase();
        }

        private void ClickToRegisterText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegistrationActivity));
            Finish();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string email, password;

            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if(!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please provide a valid email", Snackbar.LengthShort).Show();
                return;
            }
            else if(password.Length < 8)
            {
                Snackbar.Make(rootView, "Please provide a valid password", Snackbar.LengthShort).Show();
                return;
            }

            TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;
            
            mAuth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(taskCompletionListener)
                .AddOnFailureListener(taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Login Failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        void InitialFirebase()
        {
            var app = FirebaseApp.InitializeApp(this);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uberclone-6596f")
                    .SetApiKey("AIzaSyAXqLpF_JVINHqrS74X_r6uMG8wW5EZlzs")
                    .SetDatabaseUrl("https://uberclone-6596f.firebaseio.com")
                    .SetStorageBucket("uberclone-6596f.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(this, options);
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
            }
        }
    }
}