using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidX.Activity.Result;
using Android.App;
using AndroidX.Health.Connect.Client;

namespace HealthConnectExistingBindings.Platforms.Android
{
    internal class PermissionResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        public void OnActivityResult(Java.Lang.Object result)
        {
            var activityResult = (ActivityResult)result;
            if (activityResult.ResultCode == (int)Result.Ok)
            {
                Console.WriteLine("Permissions granted.");
            }
            else
            {
                Console.WriteLine("Permissions denied.");
            }
        }
    }
}
