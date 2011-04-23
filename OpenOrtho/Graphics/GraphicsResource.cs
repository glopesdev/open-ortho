using System;

namespace OpenOrtho.Graphics
{
    public abstract class GraphicsResource : IDisposable
    {
        bool disposed;

        ~GraphicsResource()
        {
            Dispose(false);
        }

        protected virtual void Unload()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Unload();
                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

