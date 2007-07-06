//
// MsmqTransportBindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MsmqTransportBindingElementTest
	{
		static BindingParameterCollection empty_params =
			new BindingParameterCollection ();

		[Test]
		public void DefaultValues ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			Assert.AreEqual (8, be.MaxPoolSize, "#1");
			Assert.AreEqual (0x80000, be.MaxBufferPoolSize, "#2");
			Assert.AreEqual (QueueTransferProtocol.Native, be.QueueTransferProtocol, "#2");
			Assert.AreEqual ("net.msmq", be.Scheme, "#3");

			Assert.IsNull (be.CustomDeadLetterQueue, "#5");
			Assert.AreEqual (DeadLetterQueue.System, be.DeadLetterQueue, "#6");
			Assert.IsTrue (be.Durable, "#7");
			Assert.IsTrue (be.ExactlyOnce, "#8");
			Assert.AreEqual (0x10000, be.MaxReceivedMessageSize, "#9");
			Assert.AreEqual (2, be.MaxRetryCycles, "#10");
			Assert.AreEqual (ReceiveErrorHandling.Fault, be.ReceiveErrorHandling, "#11");
			Assert.AreEqual (5, be.ReceiveRetryCount, "#12");
			// hmm, it is documented as 10 minutes but ...
			Assert.AreEqual (TimeSpan.FromMinutes (30), be.RetryCycleDelay, "#13");
			Assert.AreEqual (TimeSpan.FromDays (1), be.TimeToLive, "#15");
			Assert.IsFalse (be.UseMsmqTracing, "#16");
			Assert.IsFalse (be.UseSourceJournal, "#17");
		}

/*
		[Test]
		public void CanBuildChannelFactory ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsTrue (be.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplyChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputChannel> (ctx), "#4");
			// seems like it does not support session channels by itself ?
			Assert.IsFalse (be.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelFactory<IInputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelFactory<IReplySessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelFactory<IOutputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			MsmqTransportBindingElement be =
				new MsmqTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (), empty_params);
			Assert.IsTrue (be.CanBuildChannelListener<IReplyChannel> (ctx), "#1");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputChannel> (ctx), "#2");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestChannel> (ctx), "#3");
			Assert.IsFalse (be.CanBuildChannelListener<IInputChannel> (ctx), "#4");
			// seems like it does not support session channels by itself ?
			Assert.IsFalse (be.CanBuildChannelListener<IReplySessionChannel> (ctx), "#5");
			Assert.IsFalse (be.CanBuildChannelListener<IOutputSessionChannel> (ctx), "#6");
			Assert.IsFalse (be.CanBuildChannelListener<IRequestSessionChannel> (ctx), "#7");
			Assert.IsFalse (be.CanBuildChannelListener<IInputSessionChannel> (ctx), "#8");

			// IServiceChannel is not supported
			Assert.IsFalse (be.CanBuildChannelListener<IServiceChannel> (ctx), "#9");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement ()),
				empty_params);
			// returns HttpChannelFactory
			IChannelFactory<IRequestChannel> f =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			f.Open (); // required
			IChannel c = f.CreateChannel (new EndpointAddress (
				"http://www.mono-project.com"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateChannelWithoutOpen ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement ()),
				empty_params);
			// returns HttpChannelFactory
			IChannelFactory<IRequestChannel> f =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			IChannel c = f.CreateChannel (new EndpointAddress (
				"http://www.mono-project.com"));
		}

		[Test]
		public void BuildChannelFactoryTwoHttp ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement (),
					new MsmqTransportBindingElement ()),
				empty_params);
			ctx.BuildInnerChannelFactory<IRequestChannel> ();
		}

		[Test]
		public void BuildChannelFactoryHttpThenMessage ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement (),
					new BinaryMessageEncodingBindingElement ()),
				empty_params);
			IChannelFactory<IRequestChannel> cf =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			cf.Open ();
		}

		[Test]
		// with July CTP it still works ...
		public void BuildChannelFactoryHttpNoMessage ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement ()),
				empty_params);
			IChannelFactory<IRequestChannel> cf =
				ctx.BuildInnerChannelFactory<IRequestChannel> ();
			cf.Open ();
		}

		[Test]
		public void BuildChannelFactoryIgnoresRemaining ()
		{
			BindingContext ctx = new BindingContext (
				new CustomBinding (
					new MsmqTransportBindingElement (),
					new InvalidBindingElement ()),
				empty_params);
			ctx.BuildInnerChannelFactory<IRequestChannel> ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChannelInvalidScheme ()
		{
			IChannelFactory<IRequestChannel> f = new BasicHttpBinding ().BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			f.CreateChannel (new EndpointAddress ("stream:dummy"));
		}

		#region connection test

		string svcret;

		[Test]
		[Ignore ("It somehow fails...")]
		// It is almost identical to http-low-level-binding
		public void LowLevelHttpConnection ()
		{
			MsmqTransportBindingElement lel =
				new MsmqTransportBindingElement ();

			// Service
			BindingContext lbc = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection (),
				new Uri ("http://localhost:37564"),
				String.Empty, ListenUriMode.Explicit);
			listener = lel.BuildChannelListener<IReplyChannel> (lbc);

			try {

			listener.Open ();

			svcret = "";

			Thread svc = new Thread (delegate () {
				try {
					svcret = LowLevelHttpConnection_SetupService ();
				} catch (Exception ex) {
					svcret = ex.ToString ();
				}
			});
			svc.Start ();

			// Client code goes here.

			MsmqTransportBindingElement el =
				new MsmqTransportBindingElement ();
			BindingContext ctx = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
			IChannelFactory<IRequestChannel> factory =
				el.BuildChannelFactory<IRequestChannel> (ctx);

			factory.Open ();

			IRequestChannel request = factory.CreateChannel (
				new EndpointAddress ("http://localhost:37564"));

			request.Open ();

			try {
			try {
				Message reqmsg = Message.CreateMessage (
					MessageVersion.Default, "Echo");
				// sync version does not work here.
				Message msg = request.Request (reqmsg, TimeSpan.FromSeconds (5));

				using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
					msg.WriteMessage (w);
				}

				if (svcret != null)
					Assert.Fail (svcret.Length > 0 ? svcret : "service code did not finish until this test expected.");
			} finally {
				if (request.State == CommunicationState.Opened)
					request.Close ();
			}
			} finally {
				if (factory.State == CommunicationState.Opened)
					factory.Close ();
			}
			} finally {
				if (listener.State == CommunicationState.Opened)
					listener.Close ();
			}
		}

		IChannelListener<IReplyChannel> listener;

		string LowLevelHttpConnection_SetupService ()
		{
			IReplyChannel reply = listener.AcceptChannel ();
			reply.Open ();
			if (!reply.WaitForRequest (TimeSpan.FromSeconds (10)))
				return "No request reached here.";

			svcret = "Receiving request ...";
			RequestContext ctx = reply.ReceiveRequest ();
			if (ctx == null)
				return "No request context returned.";

			svcret = "Starting reply ...";
			ctx.Reply (Message.CreateMessage (MessageVersion.Default, "Ack"));
			return null; // OK
		}

		#endregion
*/
	}
}
