using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace wpf_app;

internal static class UiHelper
{
    internal static bool ValidateMandatory(ObservableCollection<string> statusItems, DataGrid dataGrid, string errorMessage)
    {
        if (dataGrid.SelectedItem == null)
        {
            statusItems.Add($"[VALIDATION ERROR] {errorMessage}");
            return false;
        }

        return true;
    }

    internal static bool ValidateMandatory(ObservableCollection<string> statusItems, string text, string errorMessage)
    {
        if (text.Length == 0)
        {
            statusItems.Add($"[VALIDATION ERROR] {errorMessage}");
            return false;
        }

        return true;
    }

    internal static bool ValidateMandatory(ObservableCollection<string> statusItems, TextBox textBox, string errorMessage)
    {
        return ValidateMandatory(statusItems, textBox.Text, errorMessage);
    }
}