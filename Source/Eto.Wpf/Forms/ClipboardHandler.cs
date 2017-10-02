using Eto.Drawing;
using Eto.Forms;
using Eto.Wpf.Drawing;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using sw = System.Windows;
using swm = System.Windows.Media;
using swmi = System.Windows.Media.Imaging;

namespace Eto.Wpf.Forms
{
	public class ClipboardHandler : WidgetHandler<sw.DataObject, Clipboard>, Clipboard.IHandler
	{
		public ClipboardHandler()
		{
			Control = new sw.DataObject();
		}

		public string[] Types => sw.Clipboard.GetDataObject()?.GetFormats();

		void Update()
		{
			// internally WPF retries here so no need to retry
			sw.Clipboard.SetDataObject(Control);
		}

		T Retry<T>(Func<T> getValue)
		{
			for (int i = 0; i < 10; i++)
			{
				try
				{
					return getValue();
				}
				catch (COMException ex)
				{
					// cannot open clipboard, so retry 10 times after 100ms
					// WPF sometimes throws this when trying to get a value
					// as it appears to retry when getting the data object, but not when 
					if (ex.HResult != unchecked((int)0x800401D0) || i == 9)
						throw;
				}
				Thread.Sleep(100);
			}
			throw new InvalidOperationException(); // should not get here
		}

		public void SetString(string value, string type)
		{
			if (string.IsNullOrEmpty(type))
				Control.SetText(value);
			else
				Control.SetData(type, value);
			Update();
		}

		public void SetData(byte[] value, string type)
		{
			Control.SetData(type, value);
			Update();
		}

		public string GetString(string type)
		{
			if (string.IsNullOrEmpty(type))
				return Text;
			return Retry(() => sw.Clipboard.ContainsData(type) ? Convert.ToString(sw.Clipboard.GetData(type)) : null);
		}

		public byte[] GetData(string type)
		{
			return Retry(() =>
			{
				if (sw.Clipboard.ContainsData(type))
				{
					var data = sw.Clipboard.GetData(type);
					var bytes = data as byte[];
					if (bytes != null)
						return bytes;
					if (data != null)
					{
						var converter = TypeDescriptor.GetConverter(data.GetType());
						if (converter != null && converter.CanConvertTo(typeof(byte[])))
						{
							return converter.ConvertTo(data, typeof(byte[])) as byte[];
						}
					}
					if (data is string)
					{
						return Encoding.UTF8.GetBytes(data as string);
					}
					if (data is IConvertible)
					{
						return Convert.ChangeType(data, typeof(byte[])) as byte[];
					}
				}
				return null;
			});
		}

		public string Text
		{
			get { return Retry(() => sw.Clipboard.ContainsText() ? sw.Clipboard.GetText() : null); }
			set
			{
				Control.SetText(value);
				Update();
			}
		}

		public string Html
		{
			get { return Retry(() => sw.Clipboard.ContainsText(sw.TextDataFormat.Html) ? sw.Clipboard.GetText(sw.TextDataFormat.Html) : null); }
			set
			{
				Control.SetText(value, sw.TextDataFormat.Html);
				Update();
			}
		}

		public Image Image
		{
			get { return Retry(() => sw.Clipboard.ContainsImage() ? new Bitmap(new BitmapHandler(sw.Clipboard.GetImage())) : null); }
			set
			{
				var dib = (value as Bitmap).ToDIB();
				if (dib != null)
				{
					// write a DIB here, so we can preserve transparency of the image
					Control.SetData(sw.DataFormats.Dib, dib);
					return;
				}

				Control.SetImage(value.ToWpf());
				Update();
			}
		}

		public void Clear()
		{
			sw.Clipboard.Clear();
			Control = new sw.DataObject();
		}
	}
}
