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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Alan McGovern (amcgovern@novell.com)
//

using System;
using System.Collections.Generic;
using System.Xml;

namespace System.IO.Packaging {

	public abstract class Package : IDisposable
	{
		internal const string RelationshipContentType = "application/vnd.openxmlformats-package.relationships+xml";
		internal const string RelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
		internal static readonly Uri RelationshipUri = new Uri ("/_rels/.rels", UriKind.Relative);
		
		PackagePartCollection partsCollection = new PackagePartCollection ();
		PackageRelationshipCollection relationshipsCollection = new PackageRelationshipCollection ();
		
		Dictionary<string, PackageRelationship> relationships;
		
		Dictionary<string, PackageRelationship> Relationships {
			get {
				if (relationships == null) {
					relationships = new Dictionary<string, PackageRelationship> ();
					
					if (PartExists (RelationshipUri))
						using (Stream stream = GetPart (RelationshipUri).GetStream ())
							LoadRelationships (relationships, stream);
				}
				return relationships;
			}
		}
		
		Uri Uri = new Uri ("/", UriKind.Relative);
		
		protected Package (FileAccess fileOpenAccess)
			: this (fileOpenAccess, false)
		{
			
		}

		protected Package (FileAccess fileOpenAccess, bool streaming)
		{
			FileOpenAccess = fileOpenAccess;
			Streaming = streaming;
		}

		void IDisposable.Dispose ()
		{
			Flush ();
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing here needs to be disposed of
		}

		public FileAccess FileOpenAccess {
			get; private set;
		}

		public PackageProperties PackageProperties {
			get; private set;
		}

		private int RelationshipId {
			get; set;
		}

		private bool Streaming {
			get; set;
		}

		public void Close ()
		{
			// FIXME: Ensure that Flush is actually called before dispose
			Flush ();
			Dispose (true);
		}

		public void Flush ()
		{
			// I should have a dirty boolean
			if (FileOpenAccess == FileAccess.ReadWrite || FileOpenAccess == FileAccess.Write)
				FlushCore ();
		}

		protected abstract void FlushCore ();

		public PackagePart CreatePart (Uri partUri, string contentType)
		{
			return CreatePart (partUri, contentType, CompressionOption.NotCompressed);
		}

		public PackagePart CreatePart (Uri partUri, string contentType, CompressionOption compressionOption)
		{
			Check.PartUri (partUri);
			Check.ContentTypeIsValid (contentType);

			if (PartExists (partUri))
				throw new InvalidOperationException ("This partUri is already contained in the package");
			
			PackagePart part = CreatePartCore (partUri, contentType, compressionOption);
			partsCollection.Parts.Add (part);
			return part;
		}

		public void DeletePart (Uri partUri)
		{
			Check.PartUri (partUri);

			PackagePart part = GetPart (partUri);
			if (part != null)
			{
				if (part.Package == null)
					throw new InvalidOperationException ("This part has already been removed");
				
				// FIXME: MS.NET doesn't remove the relationship part
				// Instead it throws an exception if you try to use it
				if (PartExists (part.RelationshipsPartUri))
					GetPart (part.RelationshipsPartUri).Package = null;

				part.Package = null;
				DeletePartCore (partUri);
				partsCollection.Parts.RemoveAll (p => p.Uri == partUri);
			}
		}

		protected abstract PackagePart CreatePartCore (Uri parentUri, string contentType, CompressionOption compressionOption);
		protected abstract void DeletePartCore (Uri partUri);

		public PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType)
		{
			return CreateRelationship (targetUri, targetMode, relationshipType, null);
		}

		public PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType, string id)
		{
			return CreateRelationship (targetUri, targetMode, relationshipType, id, false);
		}

		public PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType, string id, bool loading)
		{
			Check.TargetUri (targetUri);
			
			Check.RelationshipTypeIsValid (relationshipType);
			Check.IdIsValid (id);

			if (id == null)
				id = NextId ();

			PackageRelationship r = new PackageRelationship (id, this, relationshipType, Uri, targetMode, targetUri);

			if (!PartExists (RelationshipUri))
				CreatePart (RelationshipUri, RelationshipContentType).IsRelationship = true;
			
			Relationships.Add (r.Id, r);
			relationshipsCollection.Relationships.Add (r);

			if (!loading) {
				using (Stream s = GetPart (RelationshipUri).GetStream ())
					WriteRelationships (relationships, s);
			}
			
			return r;
		}

		public void DeleteRelationship (string id)
		{
			Relationships.Remove (id);

			relationshipsCollection.Relationships.RemoveAll (r => r.Id == id);
			if (Relationships.Count > 0)
				using (Stream s = GetPart (RelationshipUri).GetStream ())
					WriteRelationships (relationships, s);
			else
				DeletePart (RelationshipUri);
		}

		public PackagePart GetPart (Uri partUri)
		{
			Check.PartUri (partUri);
			return GetPartCore (partUri);
		}

		protected abstract PackagePart GetPartCore (Uri partUri);

		public PackagePartCollection GetParts ()
		{
			partsCollection.Parts.Clear ();
			partsCollection.Parts.AddRange (GetPartsCore());
			return partsCollection;
		}

		protected abstract PackagePart [] GetPartsCore ();


		public virtual bool PartExists (Uri partUri)
		{
			return GetPart (partUri) != null;
		}

		public PackageRelationship GetRelationship (string id)
		{
			return Relationships [id];
		}

		public PackageRelationshipCollection GetRelationships ()
		{
			relationshipsCollection.Relationships.Clear ();
			relationshipsCollection.Relationships.AddRange (Relationships.Values);
			return relationshipsCollection;
		}

		public PackageRelationshipCollection GetRelationshipsByType (string relationshipType)
		{
			PackageRelationshipCollection collection = new PackageRelationshipCollection ();
			foreach (PackageRelationship r in Relationships.Values)
				if (r.RelationshipType == relationshipType)
					collection.Relationships.Add (r);
			
			return collection;
		}

		public bool RelationshipExists (string id)
		{
			return Relationships.ContainsKey (id);
		}

		void LoadRelationships (Dictionary<string, PackageRelationship> relationships, Stream stream)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (stream);
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("rel", RelationshipNamespace);

			foreach (XmlNode node in doc.SelectNodes ("/rel:Relationships/*", manager))
			{
				
				TargetMode mode = TargetMode.Internal;
				if (node.Attributes["TargetMode"] != null)
					mode = (TargetMode) Enum.Parse (typeof(TargetMode), node.Attributes ["TargetMode"].Value);
				
				CreateRelationship (new Uri (node.Attributes ["Target"].Value.ToString(), UriKind.Relative),
				                    mode,
				                    node.Attributes["Type"].Value.ToString (),
				                    node.Attributes["Id"].Value.ToString (),
				                    true);
			}
		}
		
		private string NextId ()
		{
			while (true)
			{
				string s = RelationshipId.ToString ();
				if (!Relationships.ContainsKey (s))
					return s;
				
				RelationshipId++;
			}
		}

		public static Package Open (Stream stream)
		{
			return Open (stream, FileMode.Open);
		}

		public static Package Open (string path)
		{
			return Open (path, FileMode.OpenOrCreate);
		}

		public static Package Open (Stream stream, FileMode packageMode)
		{
			FileAccess access = packageMode == FileMode.Open ? FileAccess.Read : FileAccess.ReadWrite;
			return Open (stream, packageMode, access);
		}

		public static Package Open (string path, FileMode packageMode)
		{
			return Open (path, packageMode, FileAccess.ReadWrite);
		}

		public static Package Open (Stream stream, FileMode packageMode, FileAccess packageAccess)
		{
			return OpenCore (stream, packageMode, packageAccess);
		}

		public static Package Open (string path, FileMode packageMode, FileAccess packageAccess)
		{
			return Open (path, packageMode, packageAccess, FileShare.None);
		}

		public static Package Open (string path, FileMode packageMode, FileAccess packageAccess, FileShare packageShare)
		{
			if (packageShare != FileShare.Read && packageShare != FileShare.None)
				throw new NotSupportedException ("FileShare.Read and FileShare.None are the only supported options");

			FileInfo info = new FileInfo (path);
			
			// Bug - MS.NET appears to test for FileAccess.ReadWrite, not FileAccess.Write
			if (packageAccess != FileAccess.ReadWrite && !info.Exists)
				throw new ArgumentException ("packageAccess", "Cannot create stream with FileAccess.Read");

			
			if (info.Exists && packageMode == FileMode.OpenOrCreate && info.Length == 0)
				throw new FileFormatException ("Stream length cannot be zero with FileMode.Open");

			Stream s = File.Open (path, packageMode, packageAccess, packageShare);
			return Open (s, packageMode, packageAccess);
		}

		private static Package OpenCore (Stream stream, FileMode packageMode, FileAccess packageAccess)
		{
			if ((packageAccess & FileAccess.Read) == FileAccess.Read && !stream.CanRead)
				throw new IOException ("Stream does not support reading");

			if ((packageAccess & FileAccess.Write) == FileAccess.Write && !stream.CanWrite)
				throw new IOException ("Stream does not support reading");
			
			if (!stream.CanSeek)
				throw new ArgumentException ("stream", "Stream must support seeking");
			
			if (packageMode == FileMode.Open && stream.Length == 0)
				throw new FileFormatException("Stream length cannot be zero with FileMode.Open");

			if (packageMode == FileMode.Append || packageMode == FileMode.Truncate)
			{
				if (stream.CanWrite)
					throw new NotSupportedException (string.Format("PackageMode.{0} is not supported", packageMode));
				else
					throw new IOException (string.Format("PackageMode.{0} is not supported", packageMode));
			}

			// FIXME: MS docs say that a ZipPackage is returned by default.
			// It looks like if you create a custom package, you cannot use Package.Open.
			ZipPackage package = new ZipPackage (packageAccess);
			package.PackageStream = stream;
			return package;
		}

		internal static void WriteRelationships (Dictionary <string, PackageRelationship> relationships, Stream stream)
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("rel", RelationshipNamespace);

			doc.AppendChild (doc.CreateNode (XmlNodeType.XmlDeclaration, "", ""));
			
			XmlNode root = doc.CreateNode (XmlNodeType.Element, "Relationships", RelationshipNamespace);
			doc.AppendChild (root);
			
			foreach (PackageRelationship relationship in relationships.Values)
			{
				XmlNode node = doc.CreateNode (XmlNodeType.Element, "Relationship", RelationshipNamespace);
				
				XmlAttribute idAtt = doc.CreateAttribute ("Id");
				idAtt.Value = relationship.Id;
				node.Attributes.Append(idAtt);
				
				XmlAttribute targetAtt = doc.CreateAttribute ("Target");
				targetAtt.Value = relationship.TargetUri.ToString ();
				node.Attributes.Append(targetAtt);
				
				XmlAttribute typeAtt = doc.CreateAttribute ("Type");
				typeAtt.Value = relationship.RelationshipType;
				node.Attributes.Append(typeAtt);
				
				root.AppendChild (node);
			}

			using (XmlTextWriter writer = new XmlTextWriter (stream, System.Text.Encoding.UTF8))
				doc.WriteTo (writer);
		}
	}
}

