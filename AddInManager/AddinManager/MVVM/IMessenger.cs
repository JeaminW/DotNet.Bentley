using System;
using System.Collections.Generic;
using System.Text;

namespace AddinManager.MVVM
{
   public interface IMessenger
    {
        /// <summary>
        /// 注册一个接收消息者，指定接收的消息类型。
        /// </summary>
        /// <typeparam name="TMessage">消息类型.</typeparam>
        /// <param name="recipient">消息接收者.</param>
        /// <param name="action">消息处理委托.</param>
        void Register<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// 注册一个消息接收者，指定接收消息的类型和消息令牌.
        /// </summary>
        /// <typeparam name="TMessage">消息类型.</typeparam>
        /// <param name="recipient">消息接收者.</param>
        /// <param name="token">消息令牌.</param>
        /// <param name="action">消息处理委托.</param>
        void Register<TMessage>(object recipient, object token, Action<TMessage> action);

        /// <summary>
        /// 注册一个消息接收者，指定接收消息的类型和消息令牌.
        /// </summary>
        /// <typeparam name="TMessage">消息类型.</typeparam>
        /// <param name="recipient">消息接收者.</param>
        /// <param name="token">消息令牌.</param>
        /// <param name="receiveDerivedMessagesToo">如果为true，则注册消息类的派生类也会被指定接收消息.</param>
        /// <param name="action">The action.</param>
        void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action);

        /// <summary>
        /// 注册一个消息接收者，指定接收的消息类型。
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="recipient">The recipient.</param>
        /// <param name="receiveDerivedMessagesToo">if set to <c>true</c> [receive derived messages too].</param>
        /// <param name="action">The action.</param>
        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action);

        /// <summary>
        /// 发送一个消息.并指定消息类型.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        void Send<TMessage>(TMessage message);

        /// <summary>
        /// 发送一个消息，并指定消息接收者的类型.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="message">The message.</param>
        void Send<TMessage, TTarget>(TMessage message);

        /// <summary>
        /// 发送一个消息，并指定一个令牌.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="token">The token.</param>
        void Send<TMessage>(TMessage message, object token);

        /// <summary>
        /// 取消注册一个指定接收者的所有消息.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        void Unregister(object recipient);

        /// <summary>
        /// 取消注册机遇指定令牌的所有消息.
        /// </summary>
        /// <param name="token">The token.</param>
        void Unregister(string token);

        /// <summary>
        /// 取消注册一个接收者指定消息类型的消息.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="recipient">The recipient.</param>
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// 取消注册一个接收者和指定令牌的消息.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="recipient">The recipient.</param>
        /// <param name="token">The token.</param>
        void Unregister<TMessage>(object recipient, object token);

        /// <summary>
        /// Unregisters the specified recipient.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="recipient">The recipient.</param>
        /// <param name="action">The action.</param>
        void Unregister<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// Unregisters the specified recipient.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="recipient">The recipient.</param>
        /// <param name="token">The token.</param>
        /// <param name="action">The action.</param>
        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);
    }
}
