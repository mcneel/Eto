namespace Eto.Forms;

/// <summary>
/// Helper to turn a property changed event to an EventHandler for binding
/// </summary>
/// <remarks>
/// Use <see cref="Binding.AddPropertyEvent"/> and <see cref="Binding.RemovePropertyEvent(object,string,EventHandler{EventArgs})"/> to access
/// this functionality, or better yet use the <see cref="PropertyBinding{T}"/> class.
/// </remarks>
class PropertyNotifyHelper
{
	public string PropertyName { get; private set; }

	public event EventHandler<EventArgs> Changed;
	
	public PropertyNotifyHelper(INotifyPropertyChanged obj, string propertyName)
	{
		PropertyName = propertyName;
		obj.PropertyChanged += obj_PropertyChanged;
	}

	public void Unregister(object obj)
	{
		var notifyObject = obj as INotifyPropertyChanged;
		if (notifyObject != null)
			notifyObject.PropertyChanged -= obj_PropertyChanged;
	}

	void obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == PropertyName)
		{
			if (Changed != null)
				Changed(sender, EventArgs.Empty);
		}
	}

	public bool IsHookedTo(EventHandler<EventArgs> eh)
	{
		foreach (var invocation in Changed.GetInvocationList())
		{
			if (invocation == (Delegate)eh)
			{
				return true;
			}
		}
		return false;
	}
}