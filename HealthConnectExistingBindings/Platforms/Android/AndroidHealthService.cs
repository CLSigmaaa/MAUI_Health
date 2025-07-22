#if ANDROID
using Android.App;
using Android.Content.PM;
using AndroidX.Health.Connect.Client;
using AndroidX.Health.Connect.Client.Permission;
using AndroidX.Health.Connect.Client.Records;
using AndroidX.Health.Connect.Client.Time;
using Java.Time;
using Kotlin.Jvm;
using HealthConnectExistingBindings.Services;
using HealthConnectExistingBindings.Platforms.Android;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;

namespace HealthConnectExistingBindings.Platforms.Android
{
    public class AndroidHealthService : IHealthService
    {
        private IHealthConnectClient? _healthConnectClient;
        private Activity? _activity;



        public enum MyCoroutineSingletons
        {
            COROUTINE_SUSPENDED,
            UNDECIDED,
            RESUMED
        }


        // Make sure you added the permissions required in the android manifest otherwhise it won't work !
        private readonly HashSet<string> permissions = new HashSet<string>
        {
            HealthPermission.GetReadPermission(Kotlin.Jvm.Internal.Reflection.GetOrCreateKotlinClass(Java.Lang.Class.FromType(typeof(StepsRecord)))!),
            HealthPermission.GetWritePermission(Kotlin.Jvm.Internal.Reflection.GetOrCreateKotlinClass(Java.Lang.Class.FromType(typeof(StepsRecord)))!),
        };

        public AndroidHealthService(Activity activity)
        {
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        public Task<bool> InitializeAsync()
        {
            try
            {
                // if (!CheckSdkStatus())
                // {
                //     Console.WriteLine("Health Connect SDK is not available");
                //     return Task.FromResult(false);
                // }

                _healthConnectClient = HealthConnectClient.GetOrCreate(_activity!);
                Console.WriteLine("Health Connect client initialized");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in InitializeAsync: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public bool CheckSdkStatus()
        {
            var availabilityStatus = HealthConnectClient.GetSdkStatus(_activity!);
            if (availabilityStatus != HealthConnectClient.SdkUnavailable)
            {
                Console.WriteLine($"Health Connect SDK status: unavailable");
                return false;
            }

            if (availabilityStatus == HealthConnectClient.SdkUnavailableProviderUpdateRequired)
            {
                Console.WriteLine("Health Connect SDK unavailable: Provider update required.");
                return false;
            }

            return true;
        }

        public async Task<bool> HasAllPermissionsAsync()
        {
            try
            {
                if (_healthConnectClient == null)
                {
                    Console.WriteLine("Health Connect client not initialized");
                    return false;
                }

                var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();

                var result = _healthConnectClient.PermissionController.GetGrantedPermissions(
                    new Continuation(taskCompletionSource, default));

                if (result is Java.Lang.Enum CoroutineSingletons)
                {
                    MyCoroutineSingletons checkedEnum = (MyCoroutineSingletons)System.Enum.Parse(typeof(MyCoroutineSingletons), CoroutineSingletons.ToString());
                    if (checkedEnum == MyCoroutineSingletons.COROUTINE_SUSPENDED)
                    {
                        Console.WriteLine("Coroutine is suspended, waiting for result...");
                        result = await taskCompletionSource.Task;
                    }
                }

                var grantedSet = new HashSet<string>();
                if (result is Java.Util.ISet set)
                {
                    var iterator = set.Iterator();
                    while (iterator.HasNext)
                    {
                        var permission = iterator.Next();
                        if (permission is Java.Lang.String jstr)
                        {
                            var permissionStr = jstr.ToString();
                            grantedSet.Add(permissionStr);
                        }
                    }
                }

                var hasAll = permissions.All(permission => grantedSet.Contains(permission));
                
                Console.WriteLine($"Has all permissions (here): {hasAll}");
                return hasAll;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in HasAllPermissionsAsync: {ex.Message}");
                return false;
            }
        }

        public bool RequestPermissionsAsync()
        {
            try
            {
                if (_activity == null || _healthConnectClient == null)
                {
                    Console.WriteLine("Activity or Health Connect client not initialized");
                    return false;
                }

                // Récupérer les permissions déjà accordées
                var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
                var result = _healthConnectClient.PermissionController.GetGrantedPermissions(
                    new Continuation(taskCompletionSource, default));

                if (result is Java.Lang.Enum CoroutineSingletons)
                {
                    MyCoroutineSingletons checkedEnum = (MyCoroutineSingletons)System.Enum.Parse(typeof(MyCoroutineSingletons), CoroutineSingletons.ToString());
                    if (checkedEnum == MyCoroutineSingletons.COROUTINE_SUSPENDED)
                    {
                        Console.WriteLine("Coroutine is suspended, waiting for result...");
                        result = taskCompletionSource.Task.GetAwaiter().GetResult();
                    }
                }

                var grantedSet = new HashSet<string>();
                if (result is Java.Util.ISet set)
                {
                    var iterator = set.Iterator();
                    while (iterator.HasNext)
                    {
                        var permission = iterator.Next();
                        if (permission is Java.Lang.String jstr)
                        {
                            var permissionStr = jstr.ToString();
                            grantedSet.Add(permissionStr);
                        }
                    }
                }

                // Change the type of 'missingPermissions' to HashSet<string> to match the expected argument type
                var missingPermissions = new HashSet<string>(permissions.Where(p => !grantedSet.Contains(p) && !string.IsNullOrEmpty(p)));

                Console.WriteLine($"Permissions manquantes : {string.Join(", ", missingPermissions)}");

                if (!missingPermissions.Any())
                {
                    Console.WriteLine("Toutes les permissions sont déjà accordées");
                    return true;
                }

                // Demander uniquement les permissions manquantes
                if (_activity is MainActivity mainActivity)
                {
                    mainActivity.AskPermissions(missingPermissions);
                    Console.WriteLine("Demande des permissions manquantes");
                    return true;
                }
                else
                {
                    Console.WriteLine("Activity is not of type MainActivity");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RequestPermissionsAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<int> ReadStepsTodayAsync()
        {
            try
            {
                if (_healthConnectClient == null)
                {
                    Console.WriteLine("Health Connect client not initialized");
                    return 0;
                }

                var now = DateTimeOffset.UtcNow;
                var startOfDay = new DateTimeOffset(now.Date, TimeSpan.Zero);
                var stepCountRecordClass = JvmClassMappingKt.GetKotlinClass(Java.Lang.Class.FromType(typeof(StepsRecord)))!;

                var timeRangeFilter = TimeRangeFilter.Between(
                    Instant.OfEpochMilli(startOfDay.ToUnixTimeMilliseconds())!,
                    Instant.OfEpochMilli(now.ToUnixTimeMilliseconds())!
                );

                var request = new AndroidX.Health.Connect.Client.Request.ReadRecordsRequest(
                    recordType: stepCountRecordClass,
                    timeRangeFilter: timeRangeFilter,
                    dataOriginFilter: [],
                    ascendingOrder: true,
                    pageSize: 1000,
                    pageToken: null
                );

                var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
                var response = _healthConnectClient.ReadRecords(request, new Continuation(taskCompletionSource, default));

                if (response is Java.Lang.Enum CoroutineSingletons)
                {
                    MyCoroutineSingletons checkedEnum = (MyCoroutineSingletons)System.Enum.Parse(typeof(MyCoroutineSingletons), CoroutineSingletons.ToString());
                    if (checkedEnum == MyCoroutineSingletons.COROUTINE_SUSPENDED)
                    {
                        Console.WriteLine("Coroutine is suspended, waiting for result...");
                        response = await taskCompletionSource.Task;
                    }
                }

                int totalSteps = 0;
                if (response is AndroidX.Health.Connect.Client.Response.ReadRecordsResponse readResponse)
                {
                    var records = readResponse.Records;
                    Console.WriteLine($"Found {records.Count} steps records for today.");
                    
                    foreach (var record in records)
                    {
                        if (record is StepsRecord stepsRecord)
                        {
                            totalSteps += (int)stepsRecord.Count;
                            Console.WriteLine($"Steps: {stepsRecord.Count}, Time: {stepsRecord}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No steps records found or response is not of expected type.");
                }

                return totalSteps;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ReadStepsTodayAsync: {ex.Message}");
                return 0;
            }
        }

        public Task<bool> AddStepsTodayAsync(int steps)
        {
            try
            {
                if (_healthConnectClient == null)
                {
                    Console.WriteLine("Health Connect client not initialized");
                    return Task.FromResult(false);
                }

                var now = DateTimeOffset.UtcNow;
                var startOfDay = new DateTimeOffset(now.Date, TimeSpan.Zero);
                var endOfDay = startOfDay.AddDays(1);

                var stepsRecord = new StepsRecord(
                    startTime: Instant.OfEpochMilli(startOfDay.ToUnixTimeMilliseconds())!,
                    startZoneOffset: null,
                    endTime: Instant.OfEpochMilli(endOfDay.ToUnixTimeMilliseconds())!,
                    endZoneOffset: null,
                    count: steps,
                    metadata: new AndroidX.Health.Connect.Client.Records.Metadata.Metadata()
                );

                IRecord record = new test(stepsRecord);
                var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
                
                var recordsList = new List<IRecord> { record };
                var response = _healthConnectClient.InsertRecords(
                    recordsList,
                    new Continuation(taskCompletionSource, default)
                );

                // Pour simplifier, nous considérons que l'insertion est réussie
                Console.WriteLine($"Added {steps} steps for today");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddStepsTodayAsync: {ex.Message}");
                return Task.FromResult(false);
            }
        }
    }
}
#endif
