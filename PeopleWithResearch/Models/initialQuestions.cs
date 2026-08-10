using System;
using System.ComponentModel;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PeopleWithResearch
{
    public class initialQuestions : DatasyncClientData, INotifyPropertyChanged
    { 
        string id;
        string? question;
        string? directions;
        string? type;
        bool active;
        int questionorder;
        string? advertid;

        bool type03;
        bool type15;
        bool type110;
        bool typess;
        bool typems;
        bool typeentry;
        bool typenumberentry;

        //values for question type 0-3
        string? label0;
        string? value0;

        string? label1;
        string? value1;

        string? label2;
        string? value2;

        string? label3;
        string? value3;

        string? label4;
        string? value4;

        string? label5;
        string? value5;

        string? label6;
        string? value6;

        string? label7;
        string? value7;


        string? value8;
        string? value9;
        string? value10;
        string? value11;
        string? value12;
        string? value13;
        string? value14;
     

        string? entryanswer;
        string? answerid;


        //hide the single selection values if there not needed

        //starts at three because one and two are always needed

        bool hide3;
        bool hide4;
        bool hide5;
        bool hide6;
        bool hide7;
        bool hide8;
        bool hide9;
        bool hide10;
        bool hide11;
        bool hide12;
        bool hide13;
        bool hide14;
        bool hide15;

        string? rownum;

        string? questionnum;

        double activeop;
        bool isquestionactive;

        bool showadditionalgenderdropdown;

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "question")]
        public string? Question
        {
            get { return question; }
            set { question = value; }
        }

        [JsonProperty(PropertyName = "directions")]
        public string? Directions
        {
            get { return directions; }
            set { directions = value; }
        }

        [JsonProperty(PropertyName = "type")]
        public string? Type
        {
            get { return type; }
            set { type = value; }
        }

        [JsonProperty(PropertyName = "active")]
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }



        [JsonProperty(PropertyName = "questionorder")]
        public int Questionorder
        {
            get { return questionorder; }
            set { questionorder = value; }
        }

        [JsonProperty(PropertyName = "advertid")]
        public string? Advertid
        {
            get { return advertid; }
            set { advertid = value; }
        }


        [JsonIgnore]
        public bool Type03
        {
            get { return type03; }
            set { type03 = value; }
        }

        [JsonIgnore]
        public bool Type15
        {
            get { return type15; }
            set { type15 = value; }
        }

        [JsonIgnore]
        public bool Type110
        {
            get { return type110; }
            set { type110 = value; }
        }

        [JsonIgnore]
        public bool Typess
        {
            get { return typess; }
            set { typess = value; }
        }

        [JsonIgnore]
        public bool Typems
        {
            get { return typems; }
            set { typems = value; }
        }

        [JsonIgnore]
        public bool TypeEntry
        {
            get { return typeentry; }
            set { typeentry = value; }
        }

        [JsonIgnore]
        public bool TypeNumberEntry
        {
            get { return typenumberentry; }
            set { typenumberentry = value; }
        }



        //<!-- values for ss -->
        [JsonIgnore]
        public string? Label0
        {
            get { return label0; }
            set { label0 = value; }
        }

        [JsonIgnore]
        public string? Value0
        {
            get { return value0; }
            set { value0 = value; }
        }

        [JsonIgnore]
        public string? Label1
        {
            get { return label1; }
            set { label1 = value; }
        }

        [JsonIgnore]
        public string? Value1
        {
            get { return value1; }
            set { value1 = value; }
        }

        [JsonIgnore]
        public string? Label2
        {
            get { return label2; }
            set { label2 = value; }
        }

        [JsonIgnore]
        public string? Value2
        {
            get { return value2; }
            set { value2 = value; }
        }

        [JsonIgnore]
        public string? Label3
        {
            get { return label3; }
            set { label3 = value; }
        }

        [JsonIgnore]
        public string? Value3
        {
            get { return value3; }
            set { value3 = value; }
        }

        //add the fourth for the type 1-5
        [JsonIgnore]
        public string? Label4
        {
            get { return label4; }
            set { label4 = value; }
        }

        [JsonIgnore]
        public string? Value4
        {
            get { return value4; }
            set { value4 = value; }
        }

        [JsonIgnore]
        public string? Label5
        {
            get { return label5; }
            set { label5 = value; }
        }

        [JsonIgnore]
        public string? Value5
        {
            get { return value5; }
            set { value5 = value; }
        }

        [JsonIgnore]
        public string? Label6
        {
            get { return label6; }
            set { label6 = value; }
        }

        [JsonIgnore]
        public string? Value6
        {
            get { return value6; }
            set { value6 = value; }
        }

        [JsonIgnore]
        public string? Label7
        {
            get { return label7; }
            set { label7 = value; }
        }

        [JsonIgnore]
        public string? Value7
        {
            get { return value7; }
            set { value7 = value; }
        }

        [JsonIgnore]
        public string? Value8
        {
            get { return value8; }
            set { value8 = value; }
        }

        [JsonIgnore]
        public string? Value9
        {
            get { return value9; }
            set { value9 = value; }
        }

        [JsonIgnore]
        public string? Value10
        {
            get { return value10; }
            set { value10 = value; }
        }

        [JsonIgnore]
        public string? Value11
        {
            get { return value11; }
            set { value11 = value; }
        }

        [JsonIgnore]
        public string? Value12
        {
            get { return value12; }
            set { value12 = value; }
        }

        [JsonIgnore]
        public string? Value13
        {
            get { return value13; }
            set { value13 = value; }
        }

        [JsonIgnore]
        public string? Value14
        {
            get { return value14; }
            set { value14 = value; }
        }

        [JsonIgnore]
        public bool Hide3
        {
            get { return hide3; }
            set { hide3 = value; }
        }



        [JsonIgnore]
        public bool Hide4
        {
            get { return hide4; }
            set { hide4 = value; }
        }


        [JsonIgnore]
        public bool Hide5
        {
            get { return hide5; }
            set { hide5 = value; }
        }


        [JsonIgnore]
        public bool Hide6
        {
            get { return hide6; }
            set { hide6 = value; }
        }


        [JsonIgnore]
        public bool Hide7
        {
            get { return hide7; }
            set { hide7 = value; }
        }


        [JsonIgnore]
        public bool Hide8
        {
            get { return hide8; }
            set { hide8 = value; }
        }

        [JsonIgnore]
        public bool Hide9
        {
            get { return hide9; }
            set { hide9 = value; }
        }

        [JsonIgnore]
        public bool Hide10
        {
            get { return hide10; }
            set { hide10 = value; }
        }

        [JsonIgnore]
        public bool Hide11
        {
            get { return hide11; }
            set { hide11 = value; }
        }

        [JsonIgnore]
        public bool Hide12
        {
            get { return hide12; }
            set { hide12 = value; }
        }

        [JsonIgnore]
        public bool Hide13
        {
            get { return hide13; }
            set { hide13 = value; }
        }

        [JsonIgnore]
        public bool Hide14
        {
            get { return hide14; }
            set { hide14 = value; }
        }

        [JsonIgnore]
        public bool Hide15
        {
            get { return hide15; }
            set { hide15 = value; }
        }

        [JsonIgnore]
        public string? Entryanswer
        {
            get { return entryanswer; }
            set { entryanswer = value; }
        }

        [JsonIgnore]
        public string? Answerid
        {
            get { return answerid; }
            set { answerid = value; }
        }

        [JsonIgnore]
        public string? Rownum
        {
            get { return rownum; }
            set { rownum = value; }
        }


        [JsonIgnore]
        public string? Questionnum
        {
            get { return questionnum; }
            set { questionnum = value; }
        }

        [JsonIgnore]
        public double Activeop
        {
            get { return activeop; }
            set
            {
                activeop = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Activeop)));
                }


            }
        }

        [JsonIgnore]
        public bool Isquestionactive
        {
            get { return isquestionactive; }
            set
            {
                isquestionactive = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Isquestionactive)));
                }


            }
        }

        [JsonIgnore]
        public bool Showadditionalgenderdropdown
        {
            get { return showadditionalgenderdropdown; }
            set
            {
                showadditionalgenderdropdown = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Showadditionalgenderdropdown)));
                }


            }
        }

        


        // event handler for updating the list views
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



    }
}

