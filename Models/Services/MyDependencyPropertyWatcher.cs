using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace E621Downloader.Models.Services {
	public class MyDependencyPropertyWatcher<T>: DependencyObject, IDisposable {
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				"Value",
				typeof(object),
				typeof(MyDependencyPropertyWatcher<T>),
				new PropertyMetadata(null, OnPropertyChanged));

		public event DependencyPropertyChangedEventHandler PropertyChanged;

		public MyDependencyPropertyWatcher(DependencyObject target, string propertyPath) {
			this.Target = target;
			BindingOperations.SetBinding(
				this,
				ValueProperty,
				new Binding() { Source = target, Path = new PropertyPath(propertyPath), Mode = BindingMode.OneWay });
		}

		public DependencyObject Target { get; private set; }

		public T Value {
			get { return (T)this.GetValue(ValueProperty); }
		}

		public static void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs args) {
			var source = (MyDependencyPropertyWatcher<T>)sender;
			source.PropertyChanged?.Invoke(source.Target, args);
		}

		public void Dispose() {
			this.ClearValue(ValueProperty);
		}
	}
}
