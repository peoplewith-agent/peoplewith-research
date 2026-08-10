using PeopleWithResearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class ConfirmTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate LogoutTemplate { get; set; }


        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var Select = item as confirmationmessage;

            return Select?.action switch
            {
                "image-upload" => ImageTemplate,
                "complete" => CompleteTemplate,
                "logout" => LogoutTemplate,
                _ => CompleteTemplate
            };
        }
    }
}
