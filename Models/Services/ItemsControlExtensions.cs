using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.Foundation;
using Microsoft.Toolkit.Uwp.UI;

namespace E621Downloader.Models.Services {
	public static class ItemsControlExtensions {
		public static async void ScrollToCenterOfView(this ItemsControl itemsControl, object item) {
			// Scroll immediately if possible
			if(!itemsControl.TryScrollToCenterOfView(item)) {
				await Task.Delay(100);
				// Otherwise wait until everything is loaded, then scroll
				if(itemsControl is ListViewBase view) {
					try {
						view.ScrollIntoView(item);
						//await view.SmoothScrollIntoViewWithItemAsync(item);
					} catch(Exception) {
						//try {
						//	await view.SmoothScrollIntoViewWithItemAsync(item);
						//} catch(Exception) { }
					}
				} else if(itemsControl is ListBox box) {
					box.ScrollIntoView(item);
				}

				await itemsControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
					itemsControl.TryScrollToCenterOfView(item);
				});
			}
		}



		private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item) {
			// Find the container
			if(itemsControl.ContainerFromItem(item) is not FrameworkElement container) {
				return false;
			}


			if(container.FindParent(typeof(ScrollContentPresenter)) is not ScrollContentPresenter scrollPresenter) {
				return false;
			}

			Size size = container.RenderSize;

			var center = container.TransformToVisual(scrollPresenter).TransformPoint(new Point(size.Width / 2, size.Height / 2));

			center.Y += scrollPresenter.VerticalOffset;
			center.X += scrollPresenter.HorizontalOffset;


			// Scroll the center of the container to the center of the viewport
			if(scrollPresenter.CanVerticallyScroll) {
				scrollPresenter.SetVerticalOffset(
					CenteringOffset(
						center.Y,
						scrollPresenter.ViewportHeight,
						scrollPresenter.ExtentHeight
					)
				);
			}

			if(scrollPresenter.CanHorizontallyScroll) {
				scrollPresenter.SetHorizontalOffset(
					CenteringOffset(
						center.X,
						scrollPresenter.ViewportWidth,
						scrollPresenter.ExtentWidth
					)
				);
			}
			return true;
		}



		public static FrameworkElement FindParent(this FrameworkElement o, Type type) {

			for(var element = VisualTreeHelper.GetParent(o) as FrameworkElement;
					element != null;
					element = VisualTreeHelper.GetParent(element) as FrameworkElement) {

				if(element?.GetType() == type) {
					return element;
				}
			}

			return null;

		}

		private static double CenteringOffset(double center, double viewport, double extent) {
			return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
		}
	}
}
