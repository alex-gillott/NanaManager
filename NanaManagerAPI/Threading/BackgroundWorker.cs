using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NanaManagerAPI.Threading
{
    /// <summary>
    /// A managed worker for background, non-GUI threads
    /// </summary>
    public class BackgroundWorker
    {
        private readonly ConcurrentDictionary<Guid, Thread> threadPool = new ConcurrentDictionary<Guid, Thread>();
        private bool accessing = false;

        /// <summary>
        /// Creates a new thread that self destructs on completion
        /// </summary>
        /// <param name="Method">The method to initialise the thread with</param>
        public Guid DelegateThread( Action Method ) { //TODO - Improve safety and research thread safety
            Guid iD = Guid.NewGuid();
            void thread() {
                Method();
                bool tryRemove() {
                    accessing = true;
                    bool success = threadPool.TryRemove( iD, out Thread _ );
                    accessing = false;
                    return success;
                }
                while ( !tryRemove() ) SpinWait.SpinUntil( () => !accessing );
            }
            bool tryAdd() {
                accessing = true;
                bool success = threadPool.TryAdd( iD, new Thread( thread ) );
                accessing = false;
                return success;
            }
            while ( !tryAdd() )
                SpinWait.SpinUntil( () => !accessing );
            threadPool[iD].Start();
            return iD;
        }

        /// <summary>
        /// Aborts the specified thread and removes it
        /// </summary>
        /// <param name="ID">The ID of the thread to be aborted</param>
        public void AbortThread( Guid ID ) {
            threadPool[ID].Abort();
            bool tryRemove() {
                accessing = true;
                bool success = threadPool.TryRemove( ID, out Thread _ );
                accessing = false;
                return success;
            }
            while ( !tryRemove() ) SpinWait.SpinUntil( () => !accessing );
        }
    }
}