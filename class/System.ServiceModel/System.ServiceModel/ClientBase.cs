//
// generic ClientBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Configuration;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using ConfigurationType = System.Configuration.Configuration;

namespace System.ServiceModel
{
	[MonoTODO ("It somehow rejects classes, but dunno how we can do that besides our code level.")]
	public abstract class ClientBase<TChannel>
		: IDisposable, ICommunicationObject
	{
		static InstanceContext initialContxt = new InstanceContext (null);

		ChannelFactory<TChannel> factory;
		ClientRuntimeChannel inner_channel;
		CommunicationState state;

		protected ClientBase ()
			: this (initialContxt)
		{
		}

		protected ClientBase (string configname)
			: this (initialContxt, configname)
		{
		}

		protected ClientBase (Binding binding, EndpointAddress remoteAddress)
			: this (initialContxt, binding, remoteAddress)
		{
		}

		protected ClientBase (string configname, EndpointAddress remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (string configname, string remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (InstanceContext instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");

			ChannelEndpointElement el = GetEndpointConfig (null);
			Initialize (instance,
				ConfigUtil.CreateBinding (el.Binding, el.BindingConfiguration),
				new EndpointAddress (el.Address));
		}

		protected ClientBase (InstanceContext instance, string configname)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");

			ChannelEndpointElement el = GetEndpointConfig (configname);
			Initialize (instance,
				ConfigUtil.CreateBinding (el.Binding, el.BindingConfiguration),
				new EndpointAddress (el.Address));
		}

		protected ClientBase (InstanceContext instance,
			string configname, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, GetBindingFromConfig (configname), remoteAddress);
		}

		protected ClientBase (InstanceContext instance,
			string configname, string remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (remoteAddress == null)
				throw new ArgumentNullException ("endpointAddress");
			if (configname == null)
				throw new ArgumentNullException ("configurationname");

			Initialize (instance, GetBindingFromConfig (configname), new EndpointAddress (remoteAddress));
		}

		protected ClientBase (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, binding, remoteAddress);
		}

		// FIXME: handle interface inheritance scenarios and add tests.
		private static string GetContractName (Type t)
		{
			Attribute[] attrs = (Attribute[])t.GetCustomAttributes (typeof(ServiceContractAttribute), false);
			if (attrs.Length == 0)
				return String.Empty;

			ServiceContractAttribute attr = (ServiceContractAttribute)attrs [0];
			return attr.ConfigurationName ?? t.FullName;
		}

		// FIXME: This logic should actually be in ChannelFactory<T>.
		static ChannelEndpointElement GetEndpointConfig (string name)
		{
			string contractName = GetContractName (typeof (TChannel));
			ClientSection client = (ClientSection) ConfigurationManager.GetSection ("system.serviceModel/client");
			ChannelEndpointElement res = null;
			foreach (ChannelEndpointElement el in client.Endpoints)
				if (el.Contract == contractName && (el.Name == name || name == null)) {
					if (res != null)
						throw new InvalidOperationException (String.Format ("More then one endpoint matching contract {0} was found.", contractName));
					res = el;
				}
			if (res == null)
				throw new InvalidOperationException (String.Format ("Client endpoint configuration '{0}' was not found in {1} endpoints.", name, client.Endpoints.Count));
			return res;
		}

		static Binding GetBindingFromConfig (string endpointConfigurationName)
		{
			ChannelEndpointElement el = GetEndpointConfig (endpointConfigurationName);
			return ConfigUtil.CreateBinding (el.Binding, el.BindingConfiguration);
		}

		void Initialize (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (binding, remoteAddress);
		}

		public ChannelFactory<TChannel> ChannelFactory {
			get { return factory; }
		}

		public ClientCredentials ClientCredentials {
			get { return ChannelFactory.Credentials; }
		}

		public ServiceEndpoint Endpoint {
			get { return factory.Endpoint; }
		}

		public IClientChannel InnerChannel {
			get {
				if (inner_channel == null)
					inner_channel = (ClientRuntimeChannel) (object) factory.CreateChannel ();
				return inner_channel;
			}
		}

		protected TChannel Channel {
			get { return (TChannel) (object) InnerChannel; }
		}

		public CommunicationState State {
			get { return InnerChannel.State; }
		}

		[MonoTODO]
		public void Abort ()
		{
			InnerChannel.Abort ();
		}

		[MonoTODO]
		public void Close ()
		{
			InnerChannel.Close ();
		}

		[MonoTODO]
		public void DisplayInitializationUI ()
		{
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			Close ();
		}

		protected virtual TChannel CreateChannel ()
		{
			return ChannelFactory.CreateChannel ();
		}

		public void Open ()
		{
			InnerChannel.Open ();
		}

		#region ICommunicationObject implementation

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndOpen (IAsyncResult result)
		{
			InnerChannel.EndOpen (result);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndClose (IAsyncResult result)
		{
			InnerChannel.EndClose (result);
		}

		[MonoTODO]
		void ICommunicationObject.Close (TimeSpan timeout)
		{
			InnerChannel.Close (timeout);
		}

		[MonoTODO]
		void ICommunicationObject.Open (TimeSpan timeout)
		{
			InnerChannel.Open (timeout);
		}

		event EventHandler ICommunicationObject.Opening {
			add { InnerChannel.Opening += value; }
			remove { InnerChannel.Opening -= value; }
		}
		event EventHandler ICommunicationObject.Opened {
			add { InnerChannel.Opened += value; }
			remove { InnerChannel.Opened -= value; }
		}
		event EventHandler ICommunicationObject.Closing {
			add { InnerChannel.Closing += value; }
			remove { InnerChannel.Closing -= value; }
		}
		event EventHandler ICommunicationObject.Closed {
			add { InnerChannel.Closed += value; }
			remove { InnerChannel.Closed -= value; }
		}
		event EventHandler ICommunicationObject.Faulted {
			add { InnerChannel.Faulted += value; }
			remove { InnerChannel.Faulted -= value; }
		}

		#endregion
	}
}
