
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Media.Animation {


public class DiscreteRectKeyFrame : RectKeyFrame
{
	Rect value;
	KeyTime keyTime;

	public DiscreteRectKeyFrame ()
	{
	}

	public DiscreteRectKeyFrame (Rect value)
	{
		this.value = value;
		// XXX keytime?
	}

	public DiscreteRectKeyFrame (Rect value, KeyTime keyTime)
	{
		this.value = value;
		this.keyTime = keyTime;
	}

	protected override Freezable CreateInstanceCore ()
	{
		return new DiscreteRectKeyFrame ();
	}

	protected override Rect InterpolateValueCore (Rect baseValue, double keyFrameProgress)
	{
		return keyFrameProgress == 1.0 ? value : baseValue;
	}
}


}
