using System;
using System.Windows.Forms;

namespace Ember.Plugins.Events
{

    /// <summary>
    /// A delegate to show a form on the UI thread.
    /// </summary>
    /// <param name="sender">The plugin making the call.</param>
    /// <param name="e">The <see cref="ShowFormOnUIThreadEventArgs"/> instance containing the event data.</param>
    public delegate void ShowFormOnUIThreadHandler(object sender, ShowFormOnUIThreadEventArgs e);

    /// <summary>
    /// Event arguments for ShowFormOnUIThreadHandler
    /// </summary>
    public class ShowFormOnUIThreadEventArgs
        : EventArgs
    {

        #region Fields

        private Form form;
        private bool asDialog;

        #endregion Fields


        #region Properties

        /// <summary>
        /// Gets the form to be shown.
        /// </summary>
        public Form Form
        {
            get { return form; }
        }

        /// <summary>
        /// Gets a value indicating whether the form should be show as a dialog.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be shown as a dialog; otherwise, <c>false</c>.
        /// </value>
        public bool AsDialog
        {
            get { return asDialog; }
        }

        #endregion Properties


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowFormOnUIThreadEventArgs"/> class.
        /// </summary>
        /// <param name="form">The form to be shown.</param>
        /// <param name="asDialog">if set to <c>true</c> as dialog.</param>
        public ShowFormOnUIThreadEventArgs(Form form, bool asDialog)
        {
            this.form = form;
            this.asDialog = asDialog;
        }

        #endregion Constructor

    }

}
