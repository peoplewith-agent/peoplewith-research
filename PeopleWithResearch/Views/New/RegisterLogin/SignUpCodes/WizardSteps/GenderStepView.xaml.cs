using PeopleWithResearch;
using PeopleWithResearch.Models;

namespace PeopleWithResearch.Views.New.RegisterLogin.SignUpCodes;
public partial class GenderStepView : ContentView
{
    public GenderStepView() { InitializeComponent(); }

    private void GenderPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is ImperialViewModel vm && GenderPicker.SelectedItem is OptionDetails opt)
                vm.newuser.gender = opt.Text;
        }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "GenderStepView.GenderPicker_SelectedIndexChanged"); }
    }
}

