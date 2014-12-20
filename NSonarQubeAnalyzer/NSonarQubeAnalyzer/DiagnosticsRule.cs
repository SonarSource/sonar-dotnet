namespace NSonarQubeAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;

    using Microsoft.CodeAnalysis.Diagnostics;

    using ZeroMQ;

    /// <summary>
    /// Base class for Diagnostics Rules
    /// </summary>
    public abstract class DiagnosticsRule : DiagnosticAnalyzer
    {
        public DiagnosticsRule()
        {
            this.SetDefaultSettings();
            this.Status = true;
            this.SubscriberEnabled = true;
            ThreadPool.QueueUserWorkItem(this.StartSubscriber);
        }

        public DiagnosticsRule(bool enableSubscriber)
        {
            this.Status = true;
            this.SubscriberEnabled = enableSubscriber;
            if (enableSubscriber)
            {
                ThreadPool.QueueUserWorkItem(this.StartSubscriber);
            }
        }

        private void StartSubscriber(object state)
        {
            using (var context = ZmqContext.Create())
            {
                using (ZmqSocket subscriber = context.CreateSocket(SocketType.SUB))
                {
                    subscriber.Connect("tcp://localhost:5561");
                    subscriber.Subscribe(Encoding.Unicode.GetBytes(this.RuleId));

                    while (this.SubscriberEnabled)
                    {
                        var data = subscriber.Receive(Encoding.Unicode);
                        try
                        {
                            char[] charSeparators = { ';' };

                            var elems = data.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

                            this.Status = bool.Parse(elems[1]);
                            if (elems.Length > 2)
                            {
                                var parameters = new Dictionary<string, string>();
                                for (int i = 2; i < elems.Length; i++)
                                {
                                    parameters.Add(elems[i].Split('=')[0], elems[i].Split('=')[1]);
                                }

                                this.UpdateParameters(parameters);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }           
        }

        public bool SubscriberEnabled { get; set; }

        public bool Status { get; set; }

        /// <summary>
        /// Rule ID
        /// </summary>
        public abstract string RuleId { get; }

        /// <summary>
        /// Configure the rule from the supplied settings
        /// </summary>
        /// <param name="settings">XML settings</param>
        public virtual void Configure(XDocument settings)
        {
        }

        public virtual void SetDefaultSettings()
        {
        }

        public virtual void UpdateParameters(Dictionary<string, string> parameters)
        {
        }
    }
}