#if IOS
using HealthConnectExistingBindings.Services;
using Foundation;
using HealthKit;

namespace HealthConnectExistingBindings.Platforms.iOS
{
    public class iOSHealthService : IHealthService
    {
        private HKHealthStore? _healthStore;

        public iOSHealthService()
        {
            // Initialiser le HealthStore iOS
            if (HKHealthStore.IsHealthDataAvailable)
            {
                _healthStore = new HKHealthStore();
            }
        }

        public Task<bool> InitializeAsync()
        {
            try
            {
                if (!HKHealthStore.IsHealthDataAvailable)
                {
                    Console.WriteLine("HealthKit is not available on this device");
                    return Task.FromResult(false);
                }

                _healthStore = new HKHealthStore();
                Console.WriteLine("HealthKit initialized");
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
            // Vérifier si HealthKit est disponible
            return HKHealthStore.IsHealthDataAvailable;
        }

        public Task<bool> HasAllPermissionsAsync()
        {
            try
            {
                if (_healthStore == null)
                {
                    Console.WriteLine("HealthStore not initialized");
                    return Task.FromResult(false);
                }

                // Vérifier les permissions pour les pas
                var stepType = HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount);
                if (stepType == null)
                {
                    Console.WriteLine("Could not create step count type");
                    return Task.FromResult(false);
                }

                var authStatus = _healthStore.GetAuthorizationStatus(stepType);
                
                Console.WriteLine($"Steps permission status: {authStatus}");
                return Task.FromResult(authStatus == HKAuthorizationStatus.SharingAuthorized);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in HasAllPermissionsAsync: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            try
            {
                if (_healthStore == null)
                {
                    Console.WriteLine("HealthStore not initialized");
                    return false;
                }

                var stepType = HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount);
                if (stepType == null)
                {
                    Console.WriteLine("Could not create step count type");
                    return false;
                }

                var typesToRead = new NSSet<HKObjectType>(stepType);
                var typesToWrite = new NSSet<HKSampleType>(stepType);

                var tcs = new TaskCompletionSource<bool>();
                
                _healthStore.RequestAuthorizationToShare(typesToWrite, typesToRead, (success, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine($"Error requesting permissions: {error.LocalizedDescription}");
                        tcs.SetResult(false);
                    }
                    else
                    {
                        Console.WriteLine($"Permission request completed. Success: {success}");
                        tcs.SetResult(success);
                    }
                });

                return await tcs.Task;
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
                if (_healthStore == null)
                {
                    Console.WriteLine("HealthStore not initialized");
                    return 0;
                }

                var stepType = HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount);
                if (stepType == null)
                {
                    Console.WriteLine("Could not create step count type");
                    return 0;
                }

                var calendar = NSCalendar.CurrentCalendar;
                var now = NSDate.Now;
                var startOfDay = calendar.StartOfDayForDate(now);
                
                var predicate = HKQuery.GetPredicateForSamples(startOfDay, now, HKQueryOptions.StrictStartDate);
                
                var tcs = new TaskCompletionSource<int>();
                
                var query = new HKStatisticsQuery(stepType, predicate, HKStatisticsOptions.CumulativeSum, (query, result, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine($"Error reading steps: {error.LocalizedDescription}");
                        tcs.SetResult(0);
                        return;
                    }

                    var steps = 0;
                    if (result?.SumQuantity() != null)
                    {
                        steps = (int)result.SumQuantity()!.GetDoubleValue(HKUnit.Count);
                        Console.WriteLine($"Steps for today: {steps}");
                    }
                    
                    tcs.SetResult(steps);
                });

                _healthStore.ExecuteQuery(query);
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ReadStepsTodayAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> AddStepsTodayAsync(int steps)
        {
            try
            {
                if (_healthStore == null)
                {
                    Console.WriteLine("HealthStore not initialized");
                    return false;
                }

                var stepType = HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount);
                if (stepType == null)
                {
                    Console.WriteLine("Could not create step count type");
                    return false;
                }

                var quantity = HKQuantity.FromQuantity(HKUnit.Count, steps);
                var now = NSDate.Now;
                var sample = HKQuantitySample.FromType(stepType, quantity, now, now);

                var tcs = new TaskCompletionSource<bool>();
                
                _healthStore.SaveObject(sample, (success, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine($"Error adding steps: {error.LocalizedDescription}");
                        tcs.SetResult(false);
                    }
                    else
                    {
                        Console.WriteLine($"Successfully added {steps} steps");
                        tcs.SetResult(success);
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddStepsTodayAsync: {ex.Message}");
                return false;
            }
        }
    }
}
#endif
