using System;
using System.Collections.Generic;
using AndroidX.Activity.Result;
using Java.Util;    

namespace HealthConnectExistingBindings.Platforms.Android
{
    internal class PermissionResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        public void OnActivityResult(Java.Lang.Object result)
        {
            if (result is ISet grantedPermissions)
            {
                var granted = new HashSet<string>();
                var iterator = grantedPermissions.Iterator();
                while (iterator.HasNext)
                {
                    var permission = iterator.Next();
                    if (permission is Java.Lang.String jstr)
                        granted.Add(jstr.ToString());
                }
                Console.WriteLine($"Permissions accordées : {string.Join(", ", granted)}");
                // Ici, vous pouvez vérifier si toutes les permissions attendues sont présentes
            }
            else
            {
                Console.WriteLine($"Unexpected result type: {result?.GetType().FullName}");
            }
        }
    }
}
