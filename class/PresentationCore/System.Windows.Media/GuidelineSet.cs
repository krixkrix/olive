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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

using System.Windows;
using System.Windows.Media.Animation;

namespace System.Windows.Media {

	public class GuidelineSet : Animatable {
		public GuidelineSet (double[] guidelinesX, double[] guidelinesY)
		{
			GuidelinesX = new DoubleCollection (guidelinesX);
			GuidelinesY = new DoubleCollection (guidelinesY);
		}

		public GuidelineSet ()
		{
		}

		public new GuidelineSet Clone ()
		{
			throw new NotImplementedException ();
		}

		public new GuidelineSet CloneCurrentValue ()
		{
			throw new NotImplementedException ();
		}

		protected override Freezable CreateInstanceCore ()
		{
			return new GuidelineSet ();
		}

		public static readonly DependencyProperty GuidelinesXProperty;
		public DoubleCollection GuidelinesX {
		    get { return (DoubleCollection)GetValue (GuidelinesXProperty); }
		    set { SetValue (GuidelinesXProperty, value); }
		}

		public static readonly DependencyProperty GuidelinesYProperty;
		public DoubleCollection GuidelinesY {
		    get { return (DoubleCollection)GetValue (GuidelinesYProperty); }
		    set { SetValue (GuidelinesYProperty, value); }
		}
		
	}

}
