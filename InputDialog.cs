using System.Windows;
using System.Windows.Controls;

namespace convertor_application
{
    public static class InputDialog
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Window prompt = new Window()
            {
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = caption,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            StackPanel stackPanel = new StackPanel() { Margin = new Thickness(20) };

            TextBlock textBlock = new TextBlock()
            {
                Text = text,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };

            TextBox textBox = new TextBox()
            {
                Text = defaultValue,
                FontSize = 14,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 0, 15)
            };

            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button okButton = new Button()
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                Background = System.Windows.Media.Brushes.LightBlue
            };

            Button cancelButton = new Button()
            {
                Content = "Cancel",
                Width = 80,
                Height = 30,
                Background = System.Windows.Media.Brushes.LightGray
            };

            bool? dialogResult = null;

            okButton.Click += (sender, e) => { dialogResult = true; prompt.Close(); };
            cancelButton.Click += (sender, e) => { dialogResult = false; prompt.Close(); };

            // Allow Enter key to submit
            textBox.KeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    dialogResult = true;
                    prompt.Close();
                }
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);
            prompt.Content = stackPanel;

            textBox.Focus();
            textBox.SelectAll();

            prompt.ShowDialog();

            return dialogResult == true ? textBox.Text : string.Empty;
        }
    }
}