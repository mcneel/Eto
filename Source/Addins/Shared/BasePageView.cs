﻿using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eto.Addin.Shared
{
	public class BasePageView : Panel
	{
		Panel content = new Panel();
		Panel information = new Panel();

		public new Control Content
		{
			get { return content.Content; }
			set { content.Content = value; }
		}

		public Control Information
		{
			get { return information.Content; }
			set { information.Content = value; }
		}

		public BasePageView()
		{
			BackgroundColor = Color.FromArgb(225, 228, 232);

			content.Padding = new Padding(50, 10, 20, 10);

			base.Content = new StackLayout
			{
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				Items =
				{
					new StackLayoutItem(content, VerticalAlignment.Center, expand: true),
					new Panel { BackgroundColor = Colors.White, Size = new Size(280, 200), Content = information, Padding = new Padding(20) }
				}
			};
		}
	}
}
