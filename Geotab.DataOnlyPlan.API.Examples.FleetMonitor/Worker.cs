using System.Threading.Tasks;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Worker base class.
    /// </summary>
    abstract class Worker
    {
        bool stop;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        internal Worker()
        {

        }

        /// <summary>
        /// Do the work.
        /// </summary>
        /// <param name="continuous">Indicates whether WorkActionAsync should be executed continuously or stop after a single execution.</param>
        /// <returns></returns>
        public async Task DoWorkAsync(bool continuous)
        {
            do
            {
                await WorkActionAsync();
            }
            while (continuous && !stop);
        }

        /// <summary>
        /// Requests that processing be stopped.
        /// </summary>
        public void RequestStop()
        {
            stop = true;
        }

        /// <summary>
        /// The work action.
        /// </summary>
        public abstract Task WorkActionAsync();
    }
}