
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using NUnit.Framework;

namespace MonoTests.System.Windows.Media.Animation {


[TestFixture]
public class SingleKeyFrameTest
{
	[Test]
	public void Properties ()
	{
		Assert.AreEqual ("KeyTime", SingleKeyFrame.KeyTimeProperty.Name, "1");
		Assert.AreEqual (typeof (SingleKeyFrame), SingleKeyFrame.KeyTimeProperty.OwnerType, "2");
		Assert.AreEqual (typeof (KeyTime), SingleKeyFrame.KeyTimeProperty.PropertyType, "3");

		Assert.AreEqual ("Value", SingleKeyFrame.ValueProperty.Name, "4");
		Assert.AreEqual (typeof (SingleKeyFrame), SingleKeyFrame.ValueProperty.OwnerType, "5");
		Assert.AreEqual (typeof (float), SingleKeyFrame.ValueProperty.PropertyType, "6");
	}
}


}