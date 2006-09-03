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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//


using System.Xml;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime.Hosting
{
	public class DefaultWorkflowLoaderService : WorkflowLoaderService
	{
		public DefaultWorkflowLoaderService ()
		{

		}

		// Methods
		protected internal override Activity CreateInstance (Type workflowType)
		{
			return (Activity) Activator.CreateInstance (workflowType);
		}

		protected internal override Activity CreateInstance (XmlReader workflowDefinitionReader, XmlReader rulesReader)
		{
			throw new NotImplementedException ();
		}
	}
}

