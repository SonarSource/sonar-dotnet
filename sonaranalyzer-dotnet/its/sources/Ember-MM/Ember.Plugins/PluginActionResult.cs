using System;

namespace Ember.Plugins
{
    /// <summary>
    /// The result of a plug-in action.
    /// </summary>
    public class PluginActionResult
    {

        #region Fields

        private bool cancelled;
        private bool breakChain;
        private object result;
        private Exception error;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets a value indicating whether the action was cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool Cancelled
        {
            get { return cancelled; }
        }

        /// <summary>
        /// Gets a value indicating whether this action chain should be broken.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the action chain should be broken; otherwise, <c>false</c>.
        /// </value>
        public bool BreakChain
        {
            get { return breakChain; }
        }

        /// <summary>
        /// Gets the result of the action.
        /// </summary>
        public object Result
        {
            get { return result; }
        }

        /// <summary>
        /// Gets any exception thrown by the action.
        /// </summary>
        public Exception Error
        {
            get { return error; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginActionResult"/> class.
        /// </summary>
        /// <param name="cancelled">if set to <c>true</c> this action was cancelled.</param>
        /// <param name="breakChain">if set to <c>true</c> this action chain should be broken.</param>
        /// <param name="result">The result.</param>
        /// <param name="error">The error.</param>
        public PluginActionResult(
            bool cancelled = false, 
            bool breakChain = false, 
            object result = null, 
            Exception error = null)
        {
            this.cancelled = cancelled;
            this.breakChain = breakChain;
            this.result = result;
            this.error = error;
        }

        #endregion Constructor

    }
}
