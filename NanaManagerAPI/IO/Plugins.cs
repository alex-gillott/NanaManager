using NanaManagerAPI.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NanaManagerAPI.IO
{
    public static class Plugins
    {
		internal static List<MethodInfo> NeedSTA = new List<MethodInfo>();

		public static void LoadPlugins() {
			NeedSTA.Clear();

			Logging.Write( "Getting Plugins", "Plugins", LogLevel.Info );
			Queue<string> toLoad = new Queue<string>();
			Queue<string> toOpen = new Queue<string>();
			toLoad.Enqueue( ContentFile.PluginPath );

			while ( toLoad.Count > 0 ) {
				string cur = toLoad.Dequeue();
				if ( File.Exists( cur ) && Path.GetExtension( cur ) == ".dll" ) {
					toOpen.Enqueue( cur );
					Logging.Write( $"Discovered DLL \"{cur}\"", "Plugin" );
				}
				else if ( Directory.Exists( cur ) ) {
					Logging.Write( $"Scanning \"{cur}\" for plugins", "Plugins" );
					foreach ( string s in Directory.GetFiles( cur ) )
						toLoad.Enqueue( s );
					foreach ( string s in Directory.GetDirectories( cur ) )
						toLoad.Enqueue( s );
				}
				else
					Logging.Write( $"Invalid plugin path \"{cur}\"", "Plugins", LogLevel.Warn );
			}

			Logging.Write( "Loading Plugins", "Plugins", LogLevel.Info );

			while ( toOpen.Count > 0 ) {
				string cur = toOpen.Dequeue();
				Logging.Write( $"Attempting to load \"{cur}\"", "Plugins" );
				try {
					Assembly dll = Assembly.LoadFile( cur );
					Type[] types = dll.GetTypes();
					MethodInfo start = null;
					foreach ( var (t, methods) in from Type t in types where Attribute.IsDefined( t, typeof( EntryPointAttribute ) ) let methods = t.GetMethods() select (t, methods) ) {
						start = methods.FirstOrDefault( m => Attribute.IsDefined( m, typeof( EntryPointAttribute ) ) );
						if ( start != null )
							break;
						else
							Logging.Write( $"Entry point class \"{t.Name}\" in \"{t.Namespace}\" did not contain an entry point method. THIS IS BAD, FIX IT", "Plugins", LogLevel.Error );
					}

					if ( start != null )
						if ( Attribute.IsDefined( start, typeof( STAThreadAttribute ) ) )
							NeedSTA.Add( start );
						else
							start.Invoke( null, null );
				} catch ( ReflectionTypeLoadException ) {
					Logging.Write( $"\"{cur}\" was a blocked assembly. Please unblock the assembly in the file properties", "Plugins", LogLevel.Error);
					if ( Debugger.IsAttached )
						throw;
                } catch ( BadImageFormatException ) {
					Logging.Write( $"\"{cur}\" was not a valid assembly", "Plugins", LogLevel.Error );
					if ( Debugger.IsAttached )
						throw;
				} catch ( FileNotFoundException ) {
					Logging.Write( $"Could not find file \"{cur}\"", "Plugins", LogLevel.Error );
					if ( Debugger.IsAttached )
						throw;
				} catch ( FileLoadException ) {
					Logging.Write( $"Could not load file \"{cur}\"", "Plugins", LogLevel.Error );
					if ( Debugger.IsAttached )
						throw;
				}
			}
		}
	}
}
