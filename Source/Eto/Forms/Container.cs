using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eto.Drawing;

namespace Eto.Forms
{
	/// <summary>
	/// Base class for controls that contain children controls
	/// </summary>
	public abstract class Container : Control
	{
		new IHandler Handler { get { return (IHandler)base.Handler; } }

		/// <summary>
		/// Gets or sets the size for the client area of the control
		/// </summary>
		/// <remarks>
		/// The client size differs from the <see cref="Control.Size"/> in that it excludes the decorations of
		/// the container, such as the title bar and border around a <see cref="Window"/>, or the title and line 
		/// around a <see cref="GroupBox"/>.
		/// </remarks>
		/// <value>The size of the client area</value>
		public Size ClientSize
		{
			get { return Handler.ClientSize; }
			set { Handler.ClientSize = value; }
		}

		/// <summary>
		/// Gets an enumeration of controls that are directly contained by this container
		/// </summary>
		/// <value>The contained controls.</value>
		public abstract IEnumerable<Control> Controls { get; }

		/// <summary>
		/// Gets an enumeration of all contained child controls, including controls within child containers
		/// </summary>
		/// <value>The children.</value>
		public IEnumerable<Control> Children
		{
			get
			{
				foreach (var control in Controls)
				{
					yield return control;
					var container = control as Container;
					if (container != null)
					{
						foreach (var child in container.Children)
							yield return child;
					}
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="BindableWidget.DataContextChanged"/> event
		/// </summary>
		/// <remarks>
		/// Implementors may override this to fire this event on child widgets in a heirarchy. 
		/// This allows a control to be bound to its own <see cref="BindableWidget.DataContext"/>, which would be set
		/// on one of the parent control(s).
		/// </remarks>
		/// <param name="e">Event arguments</param>
		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);

            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (var control in Controls)
				{
					control.TriggerDataContextChanged(e);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.PreLoad"/> event, and recurses to this container's children
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnPreLoad(EventArgs e)
		{
			base.OnPreLoad(e);

            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (Control control in Controls)
				{
					control.TriggerPreLoad(e);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.Load"/> event, and recurses to this container's children
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnLoad(EventArgs e)
		{
            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (Control control in Controls)
				{
					control.TriggerLoad(e);
				}
			}
			
			base.OnLoad(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.LoadComplete"/> event, and recurses to this container's children
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);

            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (Control control in Controls)
				{
					control.TriggerLoadComplete(e);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.UnLoad"/> event, and recurses to this container's children
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnUnLoad(EventArgs e)
		{
			if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (Control control in Controls)
				{
					control.TriggerUnLoad(e);
				}
			}
			
			base.OnUnLoad(e);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Eto.Forms.Container"/> class.
		/// </summary>
		protected Container()
		{
		}

		/// <summary>
		/// Initializes a new instance of the Container with the specified handler
		/// </summary>
		/// <param name="handler">Pre-created handler to attach to this instance</param>
		protected Container(IHandler handler)
			: base(handler)
		{
		}

		/// <summary>
		/// Unbinds any bindings in the <see cref="BindableWidget.Bindings"/> collection and removes the bindings, and recurses to this container's children
		/// </summary>
		public override void Unbind()
		{
			base.Unbind();
            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (var control in Controls)
				{
					control.Unbind();
				}
			}
		}

		/// <summary>
		/// Updates all bindings in this widget, and recurses to this container's children
		/// </summary>
		public override void UpdateBindings(BindingUpdateMode mode = BindingUpdateMode.Source)
		{
			base.UpdateBindings(mode);
            if (Handler != null && Handler.RecurseToChildren)
			{
				foreach (var control in Controls)
				{
					control.UpdateBindings(mode);
				}
			}
		}

		/// <summary>
		/// Remove the specified <paramref name="controls"/> from this container
		/// </summary>
		/// <param name="controls">Controls to remove</param>
		public virtual void Remove(IEnumerable<Control> controls)
		{
			foreach (var control in controls)
				Remove(control);
		}

		/// <summary>
		/// Removes all controls from this container
		/// </summary>
		public virtual void RemoveAll()
		{
			Remove(Controls.ToArray());
		}

		/// <summary>
		/// Removes the specified <paramref name="child"/> control
		/// </summary>
		/// <param name="child">Child to remove</param>
		public abstract void Remove(Control child);

		/// <summary>
		/// Removes the specified control from the container.
		/// </summary>
		/// <remarks>
		/// This should only be called on controls that the container owns. Otherwise, call <see cref="Control.Detach"/>
		/// </remarks>
		/// <param name="child">Child to remove from this container</param>
		protected void RemoveParent(Control child)
		{
			if (child != null && !ReferenceEquals(child.Parent, null))
			{
#if DEBUG
				if (!ReferenceEquals(child.Parent, this))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The child control is not a child of this container. Ensure you only remove children that you own."));
#endif
				if (child.Loaded)
				{
					child.TriggerUnLoad(EventArgs.Empty);
				}
				child.Parent = null;
				child.TriggerDataContextChanged(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Sets the parent of the specified <paramref name="child"/> to this container
		/// </summary>
		/// <remarks>
		/// This is used by container authors to set the parent of a child before it is added to the underlying platform control.
		/// 
		/// The <paramref name="assign"/> parameter should call the handler method to add the child to the parent.
		/// </remarks>
		/// <returns><c>true</c>, if parent was set, <c>false</c> otherwise.</returns>
		/// <param name="child">Child to set the parent</param>
		/// <param name="assign">Method to assign the child to the handler</param>
		/// <param name="previousChild">Previous child that the new child is replacing.</param>
		protected void SetParent(Control child, Action assign, Control previousChild = null)
		{
			bool triggerPrevious = false;
			if (previousChild != null && !ReferenceEquals(previousChild.Parent, null) && (!ReferenceEquals(previousChild, child) || !ReferenceEquals(child.Parent, this)))
			{
#if DEBUG
				if (!ReferenceEquals(previousChild.Parent, this))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The child control is not a child of this container. Ensure you only remove children that you own."));
#endif
				if (previousChild.Loaded)
				{
					previousChild.TriggerUnLoad(EventArgs.Empty);
				}
				previousChild.Parent = null;
				triggerPrevious = true;
			}
			if (child != null && !ReferenceEquals(child.Parent, this))
			{
				// Detach so parent can remove from controls collection if necessary.
				// prevents UnLoad from being called more than once when containers think a control is still a child
				// no-op if there is no parent (handled in detach)
				child.Detach();

				child.Parent = this;
				if (Loaded && !child.Loaded)
				{
					using (child.Platform.Context)
					{
						child.TriggerPreLoad(EventArgs.Empty);
						child.TriggerLoad(EventArgs.Empty);
						child.TriggerDataContextChanged(EventArgs.Empty);
						assign();
						child.TriggerLoadComplete(EventArgs.Empty);
					}
					if (triggerPrevious)
						previousChild.TriggerDataContextChanged(EventArgs.Empty);
					return;
				}
			}
			assign();
			if (triggerPrevious)
				previousChild.TriggerDataContextChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Finds a child control in this container or any of its child containers with the specified <paramref name="id"/>
		/// </summary>
		/// <returns>The child control if found, or null if not.</returns>
		/// <param name="id">Optional identifier of the control to find that matches the <see cref="Widget.ID"/>.</param>
		/// <typeparam name="T">The type of control to find.</typeparam>
		public T FindChild<T>(string id = null)
			where T: Control
		{
			if (string.IsNullOrEmpty(id))
				return Children.OfType<T>().FirstOrDefault();
			else
				return Children.OfType<T>().FirstOrDefault(r => r.ID == id);
		}

		/// <summary>
		/// Finds a child control in this container or any of its child containers with the specified <paramref name="type"/>
		/// </summary>
		/// <returns>The child control if found, or null if not.</returns>
		/// <param name="type">The type of control to find.</param>
		/// <param name="id">Optional identifier of the control to find that matches the <see cref="Widget.ID"/>.</param>
		public Control FindChild(Type type, string id = null)
		{
			if (id == null)
				throw new ArgumentNullException("type");
			if (string.IsNullOrEmpty(id))
				return Children.FirstOrDefault(r => type.IsInstanceOfType(r));
			else
				return Children.FirstOrDefault(r => type.IsInstanceOfType(r) && r.ID == id);
		}

		/// <summary>
		/// Finds a child control in this container or any of its child containers with the specified <paramref name="id"/>.
		/// </summary>
		/// <returns>The child control if found, or null if not.</returns>
		/// <param name="id">Identifier of the control to find that matches the <see cref="Widget.ID"/>.</param>
		public Control FindChild(string id)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			return Children.FirstOrDefault(r => r.ID == id);
		}


		/// <summary>
		/// Handler interface for the <see cref="Container"/> control
		/// </summary>
		public new interface IHandler : Control.IHandler
		{
			/// <summary>
			/// Gets or sets the size for the client area of the control
			/// </summary>
			/// <remarks>
			/// The client size differs from the <see cref="P:Eto.Forms.Control.IHandler.Size"/> in that it excludes the decorations of
			/// the container, such as the title bar and border around a <see cref="Window"/>, or the title and line 
			/// around a <see cref="GroupBox"/>.
			/// </remarks>
			/// <value>The size of the client area</value>
			Size ClientSize { get; set; }

			/// <summary>
			/// Gets a value indicating whether PreLoad/Load/LoadComplete/Unload events are propegated to the children controls
			/// </summary>
			/// <remarks>
			/// This is mainly used when you want to use Eto controls in your handler, such as with the <see cref="ThemedContainerHandler{TContainer,TWidget,TCallback}"/>
			/// </remarks>
			/// <value><c>true</c> to recurse events to children; otherwise, <c>false</c>.</value>
			bool RecurseToChildren { get; }
		}
	}
}
