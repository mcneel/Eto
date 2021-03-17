using Microsoft.VisualStudio.ComponentModelHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Eto.Addin.VisualStudio
{
	static class Services
	{
		public static IComponentModel ComponentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
		public static IOleServiceProvider ServiceProvider = (IOleServiceProvider)Package.GetGlobalService(typeof(IOleServiceProvider));
		static ServiceProvider vsServiceProvider;

		public static ServiceProvider VsServiceProvider => vsServiceProvider ?? (vsServiceProvider = new ServiceProvider(ServiceProvider));

		public static T GetComponentService<T>()
			where T : class
		{
			return ComponentModel.GetService<T>();
		}

		public static T GetService<T>()
			where T : class
		{
			return VsServiceProvider.GetService(typeof(T)) as T;
		}
		public static TInterface GetService<T, TInterface>()
			where T : class
		{
			return (TInterface)VsServiceProvider.GetService(typeof(T));
		}
	}
}
