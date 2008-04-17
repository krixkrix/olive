//
// HttpChannelListener.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal class HttpChannelListener<TChannel> : ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		HttpTransportBindingElement source;
		BindingContext context;
		Uri listen_uri;
		List<IChannel> channels = new List<IChannel> ();
		MessageEncoder encoder;
		HttpChannelManager<TChannel> httpChannelManager;

		public HttpChannelListener (HttpTransportBindingElement source,
			BindingContext context)
			: base (context.Binding)
		{
			// FIXME: consider ListenUriMode
			// FIXME: there should be some way to post-provide Uri in case of null listenerUri in context.
			listen_uri = context.ListenUriBaseAddress != null ?
				new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress) : null;
			foreach (BindingElement be in context.RemainingBindingElements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					encoder = mbe.CreateMessageEncoderFactory ().Encoder;
					break;
				}
			}
			if (encoder == null)
				encoder = new TextMessageEncoder (MessageVersion.Default, Encoding.UTF8);
		}

		public HttpListener Http {
			get {  return httpChannelManager.HttpListener; }
		}

		public MessageEncoder MessageEncoder {
			get { return encoder; }
		}

		public override Uri Uri {
			get { return listen_uri; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			TChannel ch = PopulateChannel (timeout);
			channels.Add (ch);
			return ch;
		}

		TChannel PopulateChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IReplyChannel)) {
				if (ServiceHostingEnvironment.InAspNet)
					return (TChannel) (object) new AspNetReplyChannel (
						(HttpChannelListener<IReplyChannel>) (object) this, timeout);
				else
					return (TChannel) (object) new HttpReplyChannel (
						(HttpChannelListener<IReplyChannel>) (object) this, timeout);
			}

			// FIXME: implement more

			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback,
			object asyncState)
		{
			throw new NotImplementedException ();
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		void StartListening (TimeSpan timeout)
		{
			httpChannelManager = new HttpChannelManager<TChannel> (this);
			httpChannelManager.Open (timeout);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (ServiceHostingEnvironment.InAspNet)
				return;

			StartListening (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (ServiceHostingEnvironment.InAspNet)
				return;

			httpChannelManager.Stop ();
			// FIXME: somewhere to use timeout?
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("find out what to do here.")]
		protected override void OnAbort ()
		{
		}
	}
}
