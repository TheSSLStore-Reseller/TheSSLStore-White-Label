using System;

namespace WBSSLStore.Data.Infrastructure
{
    public class Disposable : IDisposable
    {
        private bool isDisposed;

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
                Disposer();
            isDisposed = true;
        }

        protected virtual void Disposer()
        {
            //To be overriden in the child class
        }
    }
}
