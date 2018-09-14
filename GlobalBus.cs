using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Shared
{
    public delegate object ObserverMessage(object sender, string message, object arguments);
    class GlobalBusObserver
    {
        public ObserverMessage observerMessage;
        public string Filter = null;
        public Regex reg = null;

        public bool validateMessage(string message)
        {
            if (reg != null)
                return reg.IsMatch(message);

            return message.ToLower().IndexOf(Filter.ToLower()) == 0;

        }
    }
    public class GlobalBus
    {
        

        private static List<GlobalBusObserver> observers = new List<GlobalBusObserver>();
        private static Dictionary<string, string> vars = new Dictionary<string, string>();
        public delegate void ReturnAction(List<Object> results);
        public static void SetVar(string name, string value)
        {
            GlobalBus.vars[name] = value;
            GlobalBus.Message(null, "VarSet." + name, value);
        }

        public static string GetVar(string name)
        {
            if (GlobalBus.ContainsVar(name))
                return GlobalBus.vars[name];
            else
                return null;
        }

        public static bool ContainsVar(string name)
        {
            return GlobalBus.vars.ContainsKey(name);
        }

        public static void Message(object sender, string message, object arguments, ReturnAction OnEnd = null)
        {
            LogHelper.cat("GlobalBus", "Message: " + message);
            List<Object> results = new List<object>();
            Thread th = new Thread(delegate ()
            {
                for (int cont = 0; cont < GlobalBus.observers.Count; cont++)
                {
                    if (((GlobalBus.observers[cont].Filter == null) && (GlobalBus.observers[cont].reg == null)) || (GlobalBus.observers[cont].validateMessage(message)))
                        try {
                            object ret = GlobalBus.observers[cont].observerMessage.Invoke(sender, message, arguments);
                            if ((OnEnd != null) && (ret != null))
                            {
                                if (ret is IEnumerable<object>)
                                    foreach (object cObject in (IEnumerable<object>)ret)
                                        results.Add(cObject);
                                else
                                    results.Add(ret);
                            }
                        } catch { }
                }

                if (OnEnd != null)
                    OnEnd(results);
            });
            th.Start();
        }
        
        public static void Observate(ObserverMessage observer)
        {
            GlobalBus.observers.Add(new GlobalBusObserver
            {
                observerMessage = observer,
                Filter = null,
                reg = null
            });
        }

        public static void Observate(ObserverMessage observer, string PrefixFilter)
        {
            GlobalBus.observers.Add(new GlobalBusObserver
            {
                observerMessage = observer,
                Filter = PrefixFilter,
                reg = null
            });
        }

        public static void Observate(ObserverMessage observer, Regex PrefixFilter)
        {
            GlobalBus.observers.Add(new GlobalBusObserver
            {
                observerMessage = observer,
                Filter = null,
                reg = PrefixFilter
            });
        }
    }

    public class GB : GlobalBus { }
}
