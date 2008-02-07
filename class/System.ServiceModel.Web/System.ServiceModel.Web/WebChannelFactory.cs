//
// WebChannelFactory.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

// This class does:
// - manual addressing support (with ChannelFactory, client will fail with
//   InvalidOperationException that claims missing manual addressing) in every
//   messages.

namespace System.ServiceModel.Web
{
	public class WebChannelFactory<TChannel> : ChannelFactory<TChannel>
	{
		[MonoTODO]
		public WebChannelFactory ()
			: base ()
		{
		}

		[MonoTODO]
		public WebChannelFactory (string configurationName)
			: base (configurationName)
		{
		}

		[MonoTODO]
		public WebChannelFactory (Binding binding)
			: base (binding)
		{
		}

		[MonoTODO]
		public WebChannelFactory (Binding binding, Uri serviceBaseUri)
			: this (new ServiceEndpoint (ContractDescription.GetContract (typeof (TChannel)), binding, new EndpointAddress (serviceBaseUri)))
		{
		}

		[MonoTODO]
		public WebChannelFactory (ServiceEndpoint endpoint)
			: base (endpoint)
		{
		}

		[MonoTODO]
		protected override void OnOpening ()
		{
		}
	}
}