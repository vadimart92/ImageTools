using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using ImageTools.Properties;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media;

namespace ImageTools.ImageRenamer {
	/// <summary>
	/// Логика взаимодействия для ImageRenamer.xaml
	/// </summary>
	public partial class ImageRenamer
	{
		ImageRenamerVM viewModel;
		public ImageRenamer() {
			InitializeComponent();
			InitBindings();
		}

		private void InitBindings() {
			viewModel = new ImageRenamerVM(this);
			DataContext = viewModel;
			lv_files.DataContext = viewModel;
			lv_files.ItemsSource = viewModel.Images;
			orderByComboBox.ItemsSource = viewModel.ExifTags;
		}

		private async void selectFolder_Click(object sender, RoutedEventArgs e) {
			var cont = selectFolder.Content;
			selectFolder.Content = "Loading...";
			await viewModel.SelectFolder();
			selectFolder.Content = cont;
		}

		private void SelectDestFolder_Click(object sender, RoutedEventArgs e) {
			viewModel.SelectDestFolder();
		}

		private void orderByComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			var exifProperty = e.AddedItems.OfType<ExifTagInfo>().FirstOrDefault();
			viewModel.orderByComboBox_SelectionChanged(exifProperty);
		}

		private void lv_files_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			var selectedImage = e.AddedItems.OfType<ImageRenameConfig>().FirstOrDefault();
			viewModel.lv_files_SelectionChanged(selectedImage);
		}

		private void export_Click(object sender, RoutedEventArgs e) {
			viewModel.Export();
		}
		
	}
	public class BoolValueInverter : IValueConverter {

		#region Члены IValueConverter

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool) value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}

		#endregion
	}
}
