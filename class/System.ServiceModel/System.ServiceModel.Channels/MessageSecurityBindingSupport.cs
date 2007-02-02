//
// MessageSecurityBindingSupport.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels
{
	internal class SupportingTokenInfo
	{
		public SupportingTokenInfo (SecurityToken token,
			SecurityTokenAttachmentMode mode,
			bool isOptional)
		{
			Token = token;
			Mode = mode;
			IsOptional = isOptional;
		}

		public SecurityToken Token;
		public SecurityTokenAttachmentMode Mode;
		public bool IsOptional;
	}

	internal class SupportingTokenInfoCollection : Collection<SupportingTokenInfo>
	{
		protected override void InsertItem (int index, SupportingTokenInfo item)
		{
			foreach (SupportingTokenInfo i in this)
				if (i.Token.GetType () == item.Token.GetType ())
					throw new ArgumentException (String.Format ("Supporting tokens do not allow multiple SecurityTokens of the same type: {0}", i.Token.GetType ()));
			base.InsertItem (index, item);
		}
	}

	internal abstract class MessageSecurityBindingSupport
	{
		SecurityTokenManager manager;
		ChannelProtectionRequirements requirements;
		SecurityTokenSerializer serializer;
		SecurityBindingElementSupport element_support;

		// only filled at prepared state.
		SecurityToken encryption_token;
		SecurityToken signing_token;
		SecurityTokenAuthenticator authenticator;
		SecurityTokenResolver auth_token_resolver;

		protected MessageSecurityBindingSupport (
			SecurityBindingElementSupport elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
		{
			element_support = elementSupport;
			Initialize (manager, requirements);
		}

		public void Initialize (SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
		{
			this.manager = manager;
			if (requirements == null)
				requirements = new ChannelProtectionRequirements ();
			this.requirements = requirements;
		}

		public abstract IDefaultCommunicationTimeouts Timeouts { get; }

		public ChannelProtectionRequirements ChannelRequirements {
			get { return requirements; }
		}

		public SecurityTokenManager SecurityTokenManager {
			get { return manager; }
		}

		public SecurityTokenSerializer TokenSerializer {
			get {
				if (serializer == null)
					serializer = manager.CreateSecurityTokenSerializer (Element.MessageSecurityVersion.SecurityTokenVersion);
				return serializer;
			}
		}

		public SecurityTokenAuthenticator TokenAuthenticator {
			get { return authenticator; }
		}

		public SecurityTokenResolver OutOfBandTokenResolver {
			get { return auth_token_resolver; }
		}

		public SecurityToken EncryptionToken {
			get { return encryption_token; }
			set { encryption_token = value; }
		}

		public SecurityToken SigningToken {
			get { return signing_token; }
			set { signing_token = value; }
		}

		#region element_support

		public SecurityBindingElement Element {
			get { return element_support.Element; }
		}

		public bool AllowSerializedSigningTokenOnReply {
			get { return element_support.AllowSerializedSigningTokenOnReply; }
		}

		public MessageProtectionOrder MessageProtectionOrder { 
			get { return element_support.MessageProtectionOrder; }
		}

		public SecurityTokenParameters InitiatorParameters { 
			get { return element_support.InitiatorParameters; }
		}

		public SecurityTokenParameters RecipientParameters { 
			get { return element_support.RecipientParameters; }
		}

		public bool RequireSignatureConfirmation {
			get { return element_support.RequireSignatureConfirmation; }
		}

		public string DefaultSignatureAlgorithm {
			get { return element_support.DefaultSignatureAlgorithm; }
		}

		#endregion

		public SecurityTokenProvider CreateTokenProvider (SecurityTokenRequirement requirement)
		{
			return manager.CreateSecurityTokenProvider (requirement);
		}

		public void CreateTokenAuthenticator (SecurityTokenRequirement requirement)
		{
			authenticator = manager.CreateSecurityTokenAuthenticator (requirement, out auth_token_resolver);
		}

		public void Release ()
		{
			ReleaseCore ();

			authenticator = null;

			IDisposable disposable = signing_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			signing_token = null;

			disposable = encryption_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			encryption_token = null;
		}

		protected abstract void ReleaseCore ();

		public SupportingTokenInfoCollection CollectSupportingTokens (string action)
		{
			SupportingTokenInfoCollection tokens =
				new SupportingTokenInfoCollection ();

			SupportingTokenParameters supp;

			CollectSupportingTokensCore (tokens, Element.EndpointSupportingTokenParameters, true);
			if (Element.OperationSupportingTokenParameters.TryGetValue (action, out supp))
				CollectSupportingTokensCore (tokens, supp, true);
			CollectSupportingTokensCore (tokens, Element.OptionalEndpointSupportingTokenParameters, false);
			if (Element.OptionalOperationSupportingTokenParameters.TryGetValue (action, out supp))
				CollectSupportingTokensCore (tokens, supp, false);

			return tokens;
		}

		void CollectSupportingTokensCore (
			SupportingTokenInfoCollection l,
			SupportingTokenParameters s,
			bool required)
		{
			foreach (SecurityTokenParameters p in s.Signed)
				l.Add (new SupportingTokenInfo (GetExchangeToken (p), SecurityTokenAttachmentMode.Signed, required));
			foreach (SecurityTokenParameters p in s.Endorsing)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.Endorsing, required));
			foreach (SecurityTokenParameters p in s.SignedEndorsing)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.SignedEndorsing, required));
			foreach (SecurityTokenParameters p in s.SignedEncrypted)
				l.Add (new SupportingTokenInfo (GetExchangeToken (p), SecurityTokenAttachmentMode.SignedEncrypted, required));
		}

		SecurityToken GetSigningToken (SecurityTokenParameters p)
		{
			return GetToken (CreateRequirement (), p, SecurityKeyUsage.Signature);
		}

		SecurityToken GetExchangeToken (SecurityTokenParameters p)
		{
			return GetToken (CreateRequirement (), p, SecurityKeyUsage.Exchange);
		}

		public SecurityToken GetToken (SecurityTokenRequirement requirement, SecurityTokenParameters targetParams, SecurityKeyUsage usage)
		{
			requirement.KeyUsage = usage;
			requirement.Properties [ReqType.SecurityBindingElementProperty] = Element;
			requirement.Properties [ReqType.MessageSecurityVersionProperty] =
				Element.MessageSecurityVersion.SecurityTokenVersion;

			targetParams.CallInitializeSecurityTokenRequirement (requirement);

			SecurityTokenProvider provider =
				CreateTokenProvider (requirement);
			ICommunicationObject obj = provider as ICommunicationObject;
			try {
				if (obj != null)
					obj.Open (Timeouts.OpenTimeout);
				return provider.GetToken (Timeouts.SendTimeout);
			} finally {
				if (obj != null && obj.State == CommunicationState.Opened)
					obj.Close ();
			}
		}
		
		public abstract SecurityTokenRequirement CreateRequirement ();
	}

	internal class InitiatorMessageSecurityBindingSupport : MessageSecurityBindingSupport
	{
		ChannelFactoryBase factory;
		EndpointAddress message_to;

		public InitiatorMessageSecurityBindingSupport (
			SecurityBindingElementSupport elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
			: base (elementSupport, manager, requirements)
		{
		}

		public override IDefaultCommunicationTimeouts Timeouts {
			get { return factory; }
		}

		public void Prepare (ChannelFactoryBase factory, EndpointAddress address)
		{
			this.factory = factory;
			this.message_to = address;

			SecurityTokenRequirement r = CreateRequirement ();
			RecipientParameters.CallInitializeSecurityTokenRequirement (r);
			CreateTokenAuthenticator (r); // FIXME: is it correct??
			EncryptionToken = GetToken (r, RecipientParameters, SecurityKeyUsage.Exchange);
			r = CreateRequirement ();
			InitiatorParameters.CallInitializeSecurityTokenRequirement (r);
			SigningToken = GetToken (r, InitiatorParameters, SecurityKeyUsage.Signature);
		}

		protected override void ReleaseCore ()
		{
			this.factory = null;
			this.message_to = null;
		}

		public override SecurityTokenRequirement CreateRequirement ()
		{
			SecurityTokenRequirement r = new InitiatorServiceModelSecurityTokenRequirement ();
//			r.Properties [ReqType.IssuerAddressProperty] = message_to;
			r.Properties [ReqType.TargetAddressProperty] = message_to;
			return r;
		}
	}

	class RecipientMessageSecurityBindingSupport : MessageSecurityBindingSupport
	{
		ChannelListenerBase listener;

		public RecipientMessageSecurityBindingSupport (
			SecurityBindingElementSupport elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
			: base (elementSupport, manager, requirements)
		{
		}

		public override IDefaultCommunicationTimeouts Timeouts {
			get { return listener; }
		}

		public void Prepare (ChannelListenerBase listener)
		{
			this.listener = listener;

			SecurityTokenRequirement r = CreateRequirement ();
			RecipientParameters.CallInitializeSecurityTokenRequirement (r);
			CreateTokenAuthenticator (r);
			SigningToken = GetToken (r, RecipientParameters, SecurityKeyUsage.Signature);

			r = CreateRequirement ();
			InitiatorParameters.CallInitializeSecurityTokenRequirement (r);
			EncryptionToken = GetToken (r, InitiatorParameters, SecurityKeyUsage.Exchange);
		}

		protected override void ReleaseCore ()
		{
			this.listener = null;
		}

		public override SecurityTokenRequirement CreateRequirement ()
		{
			SecurityTokenRequirement requirement =
				new RecipientServiceModelSecurityTokenRequirement ();
			requirement.Properties [ReqType.ListenUriProperty] = listener.Uri;
			return requirement;
		}
	}

	internal abstract class SecurityBindingElementSupport
	{
		public abstract SecurityBindingElement Element { get; }

		public abstract bool AllowSerializedSigningTokenOnReply { get; }

		public abstract MessageProtectionOrder MessageProtectionOrder { get; }

		public abstract SecurityTokenParameters InitiatorParameters { get; }

		public abstract SecurityTokenParameters RecipientParameters { get; }

		public abstract bool RequireSignatureConfirmation { get; }

		public abstract string DefaultSignatureAlgorithm { get; }
	}

	internal class SymmetricSecurityBindingElementSupport : SecurityBindingElementSupport
	{
		SymmetricSecurityBindingElement element;

		public SymmetricSecurityBindingElementSupport (
			SymmetricSecurityBindingElement element)
		{
			this.element = element;
		}

		public override SecurityBindingElement Element {
			get { return element; }
		}

		// FIXME: const true or false
		public override bool AllowSerializedSigningTokenOnReply {
			get { throw new NotImplementedException (); }
		}

		public override MessageProtectionOrder MessageProtectionOrder {
			get { return element.MessageProtectionOrder; }
		}

		public override SecurityTokenParameters InitiatorParameters {
			get { return element.ProtectionTokenParameters; }
		}

		public override SecurityTokenParameters RecipientParameters {
			get { return element.ProtectionTokenParameters; }
		}

		public override bool RequireSignatureConfirmation {
			get { return element.RequireSignatureConfirmation; }
		}

		public override string DefaultSignatureAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultSymmetricSignatureAlgorithm; }
		}
	}

	internal class AsymmetricSecurityBindingElementSupport : SecurityBindingElementSupport
	{
		AsymmetricSecurityBindingElement element;

		public AsymmetricSecurityBindingElementSupport (
			AsymmetricSecurityBindingElement element)
		{
			this.element = element;
		}

		public override bool AllowSerializedSigningTokenOnReply {
			get { return element.AllowSerializedSigningTokenOnReply; }
		}

		public override SecurityBindingElement Element {
			get { return element; }
		}

		public override MessageProtectionOrder MessageProtectionOrder {
			get { return element.MessageProtectionOrder; }
		}

		public override SecurityTokenParameters InitiatorParameters {
			get { return element.InitiatorTokenParameters; }
		}

		public override SecurityTokenParameters RecipientParameters {
			get { return element.RecipientTokenParameters; }
		}

		public override bool RequireSignatureConfirmation {
			get { return element.RequireSignatureConfirmation; }
		}

		public override string DefaultSignatureAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultAsymmetricSignatureAlgorithm; }
		}
	}
}
