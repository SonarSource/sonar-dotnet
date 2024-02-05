using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor_Linear
{
    class Program
    {
        private object obj = new object();
        private object other = new object();

        public object PublicObject = new object();

        public void Method1(string arg)
        {
            Monitor.Enter(obj); // FN, because arg.Length can throw NullReferenceException
            Console.WriteLine(arg.Length);
            Monitor.Exit(obj);
        }

        public void Method1_SafeOperation(string arg)
        {
            Monitor.Enter(obj); // Compliant
            arg = null;
            Monitor.Exit(obj);
        }

        public void Method2(string arg)
        {
            Monitor.Enter(obj); // Compliant
            Console.WriteLine(arg.Length);
            Monitor.Exit(other);
        }

        public void Method3()
        {
            Monitor.Enter(obj); // Compliant
            var a = new Action(() =>
            {
                Monitor.Exit(obj);
            });
        }

        public void Method4(string arg)
        {
            var localObj = obj;
            Monitor.Enter(localObj); // Compliant
            arg = null;
            Monitor.Exit(localObj);
        }

        public void Method5(string arg)
        {
            var localObj = obj;
            Monitor.Enter(obj); // Compliant
            Console.WriteLine(arg.Length);
            Monitor.Exit(localObj);
        }

        public void Method6(string arg, object paramObj)
        {
            paramObj = obj;
            Monitor.Enter(paramObj); // Compliant
            arg = null;
            Monitor.Exit(paramObj);
        }

        public void Method7(string arg)
        {
            Monitor.Enter(obj); // Compliant
            Console.WriteLine(arg.Length);
            var localObj = obj;
            Monitor.Exit(localObj);
        }

        public void Method7(string arg, object paramObj)
        {
            Monitor.Enter(paramObj); // Compliant
            arg = null;
            Monitor.Exit(paramObj);
        }

        public void Method8(string arg, Program p1)
        {
            Monitor.Enter(p1.PublicObject); // Compliant
            Console.WriteLine(arg.Length);
            Monitor.Exit(p1.PublicObject);
        }

        public void Method9()
        {
            var a = new Action(() =>
            {
                Monitor.Enter(obj); // Compliant
        });

            Monitor.Exit(obj);
        }

        public void Method10()
        {
            var getObj = new Func<object>(() =>
            {
                return obj;
            });

            Monitor.Enter(getObj());
            Monitor.Exit(getObj());
        }

        public void Method11()
        {
            var getObj = new Func<object>(() =>
            {
                return obj;
            });

            Monitor.Enter(obj);
            Monitor.Exit(getObj());
        }

        public void Method12()
        {
            Monitor.Enter(obj); // Compliant
            var a = new Action(() =>
            {
                Monitor.Exit(obj);
            });

            a();
        }

        public void Method14(string arg)
        {
            Monitor.Exit(obj);
            arg = null;
            Monitor.Enter(obj); // Compliant
            arg = null;
            Monitor.Exit(obj);
        }

        public void Method15(Program first, Program second)
        {
            Monitor.Enter(first.obj); // Compliant
            Monitor.Exit(second.obj);
        }


        public void Method16()
        {
            void LocalFunc()
            {
                Monitor.Enter(obj); // Compliant
            }

            Monitor.Exit(obj);
        }

        public void Method17()
        {
            object LocalFunc()
            {
                return obj;
            }

            Monitor.Enter(LocalFunc());
            Monitor.Exit(LocalFunc());
        }

        public void Method18()
        {
            object LocalFunc()
            {
                return obj;
            }

            Monitor.Enter(obj);
            Monitor.Exit(LocalFunc());
        }

        public void Method19()
        {
            Monitor.Enter(obj); // Compliant
            LocalFunc();

            void LocalFunc()
            {
                Monitor.Exit(obj); // Compliant
            }
        }

        public void WrongCallNoArgs(string arg)
        {
            Monitor.Exit(obj);
            Console.WriteLine(arg.Length);
            Monitor.Enter(); // Error [CS1501] No overload for method 'Enter' takes 0 arguments
        }

        public void DifferentFields(Program first, Program second)
        {
            Monitor.Exit(second.obj);
            Monitor.Enter(first.obj);
        }

        public void FirstReleasedThanAcquired() // Issues from Peach
        {
            Monitor.Exit(obj);
            try
            {
                Console.WriteLine();
            }
            finally
            {
                Monitor.Enter(obj); // Compliant, source of FPs on Peach
            }
        }

        static int Property
        {
            get // Adds coverage for handling FlowCaptureReference operations.
            {
                var lockObject = new object();
                lock (lockObject)
                {
                    return 1;
                }
            }
        }

        public abstract class Base
        {
            protected abstract List<Task> GetScheduledTasks();
        }

        public class Derived : Base
        {
            private readonly List<Task> _tasks = new List<Task>();
            protected override List<Task> GetScheduledTasks() // Adds coverage for handling Conversion operations
            {
                var lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);

                    if (lockTaken) return _tasks;
                    else throw new NotSupportedException();
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(_tasks);
                }
            }
        }

        public class PropertyReference
        {
            public Context PreMovieInfoScraperAction(Context context) // adds coverage for PropertyReference operations
            {
                if (string.IsNullOrEmpty(context.Movie.Year))
                {
                    context.Movie.Year = "2022";
                }

                return context;
            }

            public class Context
            {
                public Movie Movie { get; }
            }

            public class Movie
            {
                public string Year { get; set; }
            }
        }
    }
}
