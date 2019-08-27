using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AddinManager.MVVM
{
    class Messenger : IMessenger
    {
        #region fields
        private static readonly object CreationLock = new object();
        private static IMessenger m_DefaultInstance;
        private readonly object m_RegisterLock = new object();
        private Dictionary<Type, List<WeakActionAndToken>> m_RecipientsOfSubclassesAction;
        private Dictionary<Type, List<WeakActionAndToken>> m_RecipientsStrictAction;
        #endregion

        #region properties
        /// <summary>
        /// 单例.
        /// </summary>
        public static IMessenger Default
        {
            get
            {
                if (m_DefaultInstance == null)
                {
                    lock (CreationLock)
                    {
                        if (m_DefaultInstance == null)
                        {
                            m_DefaultInstance = new Messenger();
                        }
                    }
                }
                return m_DefaultInstance;
            }
        }
        #endregion

        #region methods

        #region IMessenger Members

        public virtual void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register(recipient, null, false, action);
        }

        public virtual void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            Register(recipient, null, receiveDerivedMessagesToo, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            Register(recipient, token, false, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (m_RegisterLock)
            {
                var messageType = typeof(TMessage);

                Dictionary<Type, List<WeakActionAndToken>> recipients;

                if (receiveDerivedMessagesToo)
                {
                    if (m_RecipientsOfSubclassesAction == null)
                    {
                        m_RecipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = m_RecipientsOfSubclassesAction;
                }
                else
                {
                    if (m_RecipientsStrictAction == null)
                    {
                        m_RecipientsStrictAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = m_RecipientsStrictAction;
                }

                lock (recipients)
                {
                    List<WeakActionAndToken> list;

                    if (!recipients.ContainsKey(messageType))
                    {
                        list = new List<WeakActionAndToken>();
                        recipients.Add(messageType, list);
                    }
                    else
                    {
                        list = recipients[messageType];
                    }

                    var weakAction = new WeakAction<TMessage>(recipient, action);

                    var item = new WeakActionAndToken
                    {
                        Action = weakAction,
                        Token = token
                    };

                    list.Add(item);
                }
            }

            RequestCleanup();
        }

        private bool _isCleanupRegistered;

        public virtual void Send<TMessage>(TMessage message)
        {
            SendToTargetOrType(message, null, null);
        }

        public virtual void Send<TMessage, TTarget>(TMessage message)
        {
            SendToTargetOrType(message, typeof(TTarget), null);
        }

        public virtual void Send<TMessage>(TMessage message, object token)
        {
            SendToTargetOrType(message, null, token);
        }

        public virtual void Unregister(object recipient)
        {
            UnregisterFromListsByRecipient(recipient, m_RecipientsOfSubclassesAction);
            UnregisterFromListsByRecipient(recipient, m_RecipientsStrictAction);
        }

        public void Unregister(string token)
        {
            UnregisterFromListsByToken(token, m_RecipientsOfSubclassesAction);
            UnregisterFromListsByToken(token, m_RecipientsStrictAction);
        }

        public virtual void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null, null);
        }

        public virtual void Unregister<TMessage>(object recipient, object token)
        {
            Unregister<TMessage>(recipient, token, null);
        }

        public virtual void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            Unregister(recipient, null, action);
        }

        public virtual void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            UnregisterFromLists(recipient, token, action, m_RecipientsStrictAction);
            UnregisterFromLists(recipient, token, action, m_RecipientsOfSubclassesAction);
            RequestCleanup();
        }

        #endregion

        public static void OverrideDefault(IMessenger newMessenger)
        {
            m_DefaultInstance = newMessenger;
        }

        public static void Reset()
        {
            m_DefaultInstance = null;
        }

        public void ResetAll()
        {
            Reset();
        }

        public void RequestCleanup()
        {
            if (!_isCleanupRegistered)
            {
                Action cleanupAction = Cleanup;

                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(cleanupAction, System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
                _isCleanupRegistered = true;
            }
        }

        public void Cleanup()
        {
            CleanupList(m_RecipientsOfSubclassesAction);
            CleanupList(m_RecipientsStrictAction);
            _isCleanupRegistered = false;
        }
        #endregion

        #region privates 
        private static void CleanupList(IDictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (lists == null)
            {
                return;
            }

            lock (lists)
            {
                var listsToRemove = new List<Type>();
                foreach (var list in lists)
                {
                    var recipientsToRemove = list.Value
                        .Where(item => item.Action == null || !item.Action.IsAlive)
                        .ToList();

                    foreach (var recipient in recipientsToRemove)
                    {
                        list.Value.Remove(recipient);
                    }

                    if (list.Value.Count == 0)
                    {
                        listsToRemove.Add(list.Key);
                    }
                }

                foreach (var key in listsToRemove)
                {
                    lists.Remove(key);
                }
            }
        }

        private static void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens, Type messageTargetType, object token)
        {
            if (weakActionsAndTokens != null)
            {
                // Correction Messaging BL0004.007
                var list = weakActionsAndTokens.ToList();
                var listClone = list.Take(list.Count()).ToList();

                foreach (var item in listClone)
                {
                    var executeAction = item.Action as IExecuteWithObject;

                    if (executeAction != null
                        && item.Action.IsAlive
                        && item.Action.Target != null
                        && (messageTargetType == null
                            || item.Action.Target.GetType() == messageTargetType
                            || messageTargetType.IsAssignableFrom(item.Action.Target.GetType()))
                            && ((item.Token == null && token == null)
                            || item.Token != null && item.Token.Equals(token)))
                    {
                        executeAction.ExecuteWithObject(message);
                    }
                }
            }
        }

        private static void UnregisterFromListsByToken(object token, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (token == null || lists == null || lists.Count == 0)
            {
                return;
            }

            lock (lists)
            {
                foreach (var messageType in lists.Keys)
                {
                    foreach (var item in lists[messageType])
                    {
                        if (item.Action != null && item.Token == token)
                        {
                            item.Action.MarkForDeletion();
                        }
                    }
                }
            }
        }

        private static void UnregisterFromListsByRecipient(object recipient, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (recipient == null || lists == null || lists.Count == 0)
            {
                return;
            }

            lock (lists)
            {
                foreach (var messageType in lists.Keys)
                {
                    foreach (var item in lists[messageType])
                    {
                        var weakAction = (IExecuteWithObject)item.Action;

                        if (weakAction != null
                            && recipient == weakAction.Target)
                        {
                            weakAction.MarkForDeletion();
                        }
                    }
                }
            }
        }

        private static void UnregisterFromLists<TMessage>(object recipient, object token, Action<TMessage> action, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            var messageType = typeof(TMessage);

            if (recipient == null || lists == null || lists.Count == 0 || !lists.ContainsKey(messageType))
            {
                return;
            }

            lock (lists)
            {
                foreach (var item in lists[messageType])
                {
                    if (item.Action is WeakAction<TMessage>)
                    {
                        var weakActionCasted = item.Action as WeakAction<TMessage>;

                        if(recipient == weakActionCasted.Target 
                            && (action == null || action.Method.Name == weakActionCasted.MethodName) 
                            && (token == null || token.Equals(item.Token)))
                        {
                            item.Action.MarkForDeletion();
                        }
                    }
                }
            }
        }


        private void SendToTargetOrType<TMessage>(TMessage message, Type messageTargetType, object token)
        {
            var messageType = typeof(TMessage);

            if (m_RecipientsOfSubclassesAction != null)
            {
                // Correction Messaging BL0008.002
                var listClone = m_RecipientsOfSubclassesAction.Keys.Take(m_RecipientsOfSubclassesAction.Count()).ToList();

                foreach (var type in listClone)
                {
                    List<WeakActionAndToken> list = null;

                    if (messageType == type || messageType.IsSubclassOf(type) || type.IsAssignableFrom(messageType))
                    {
                        lock (m_RecipientsOfSubclassesAction)
                        {
                            list = m_RecipientsOfSubclassesAction[type].Take(m_RecipientsOfSubclassesAction[type].Count()).ToList();
                        }
                    }

                    SendToList(message, list, messageTargetType, token);
                }
            }

            if (m_RecipientsStrictAction != null)
            {
                List<WeakActionAndToken> list = null;

                lock (m_RecipientsStrictAction)
                {
                    if (m_RecipientsStrictAction.ContainsKey(messageType))
                    {
                        list = m_RecipientsStrictAction[messageType]
                            .Take(m_RecipientsStrictAction[messageType].Count())
                            .ToList();
                    }
                }

                if (list != null)
                {
                    SendToList(message, list, messageTargetType, token);
                }
            }

            RequestCleanup();
        }


        private struct WeakActionAndToken
        {
            public WeakAction Action;

            public object Token;
        }
        #endregion
    }
}
