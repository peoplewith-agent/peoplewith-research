using PeopleWithResearch;
using PeopleWithResearch.Models;

namespace PeopleWithResearch.Views.New.RegisterLogin.SignUpCodes;
public partial class EthnicityStepView : ContentView
{
    public EthnicityStepView() { InitializeComponent(); }

    private void EthnicityPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is ImperialViewModel vm && EthnicityPicker.SelectedItem is OptionDetails opt)
                vm.newuser.ethnicity = opt.Text;
        }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "EthnicityStepView.EthnicityPicker_SelectedIndexChanged"); }
    }
}

