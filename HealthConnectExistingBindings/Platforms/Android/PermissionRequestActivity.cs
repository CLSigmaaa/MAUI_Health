using Android.App;
using Android.OS;
using Android.Util;
using AndroidX.Activity.Result;
using AndroidX.Health.Connect.Client;

namespace HealthConnectExistingBindings.Platforms.Android;


[Activity(Label = "PermissionRequestActivity")]
public class PermissionRequestActivity : Activity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var contract = PermissionController.CreateRequestPermissionResultContract();
        //var launcher = RegisterForActivityResult(contract, new PermissionResultCallback(this));

        //var javaPermissions = new HashSet();
        //javaPermissions.Add("android.permission.POST_NOTIFICATIONS"); // Exemple
        //launcher.Launch(javaPermissions);
    }

    // Finir l'Activity apr�s r�ception du r�sultat
    private class PermissionResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        private readonly Activity _activity;

        public PermissionResultCallback(Activity activity)
        {
            _activity = activity;
        }

        public void OnActivityResult(Java.Lang.Object result)
        {
            Log.Debug("Permissions", $"R�sultat re�u : {result}");
            _activity.Finish(); // Ferme l'activit�
        }
    }
}