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
                using (ZmqSocket subscriber = context.CreateSocket(SocketType.REQ))
                {
                    var currentProcess = Process.GetCurrentProcess();
                    var id = currentProcess.Id;
                    if (id < 5000)
                    {
                        id += 5000;
                    }

                    subscriber.Connect("tcp://localhost:" + id);

                    subscriber.Send(Encoding.Unicode.GetBytes(this.RuleId));
                    var message = subscriber.ReceiveMessage();
                    var data = Encoding.Unicode.GetString(message[0].Buffer);

                    char[] charSeparators = { ';' };
                    var elems = data.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
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