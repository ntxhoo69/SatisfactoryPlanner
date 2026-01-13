using System.Windows;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Controls;

public partial class SourceConfigDialog : Window
{
    public string ItemName { get; private set; }
    public double ItemRate { get; private set; }
    
    public SourceConfigDialog(Building? building = null)
    {
        InitializeComponent();
        
        // Pre-fill if building already has configuration
        if (building != null && building.IsSource())
        {
            ItemNameTextBox.Text = building.SourceItemName ?? "";
            ItemRateTextBox.Text = building.SourceItemRate.ToString("F1");
        }
    }
    
    private void OK_Click(object sender, RoutedEventArgs e)
    {
        ItemName = ItemNameTextBox.Text.Trim();
        
        if (string.IsNullOrEmpty(ItemName))
        {
            MessageBox.Show("Please enter an item name.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (!double.TryParse(ItemRateTextBox.Text, out double rate) || rate <= 0)
        {
            MessageBox.Show("Please enter a valid positive number for items per minute.", 
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        ItemRate = rate;
        DialogResult = true;
        Close();
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
