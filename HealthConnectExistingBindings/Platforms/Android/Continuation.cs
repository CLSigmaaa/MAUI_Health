using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Runtime;
using Kotlin.Coroutines;

namespace HealthConnectExistingBindings.Platforms.Android
{
      internal class Continuation : Java.Lang.Object, IContinuation
    {
        public ICoroutineContext Context => EmptyCoroutineContext.Instance;

        private readonly TaskCompletionSource<Java.Lang.Object> taskCompletionSource;



        public Continuation(TaskCompletionSource<Java.Lang.Object> taskCompletionSource, CancellationToken cancellationToken)
        {
            this.taskCompletionSource = taskCompletionSource;
            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());
        }


        public void ResumeWith(Java.Lang.Object result)
        {
            var exceptionField = result.Class.GetDeclaredFields().FirstOrDefault(x => x.Name.Contains("exception", StringComparison.OrdinalIgnoreCase));
            if (exceptionField != null)
            {
                var exception = exceptionField.Get(result).JavaCast<Java.Lang.Throwable>();
                taskCompletionSource.TrySetException(new System.Exception(exception is not null ? exception.Message : "No known exception"));
                return;
            }
            else
            {
                taskCompletionSource.TrySetResult(result);
            }
        }
    }
}
