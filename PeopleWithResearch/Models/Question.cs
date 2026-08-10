using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Datasync.Client;

namespace PeopleWithResearch
{
    public class Question : DatasyncClientData, INotifyPropertyChanged
    {
        string id;
        string? question;
        string? directions;
        string? type;
        string? questionnaireid;
        bool isactive;
        string? category;
        string? notes;
        int questionorder;
        string? signupcode;
        string? area;
        ObservableCollection<Answers>? answerlist;
        bool type03;
        bool type15;
        bool type110;
        bool typess;
        bool typems;
        bool typeentry;
        bool typessadd;
        bool typess1;
        bool typess2;
        bool typess3;
        bool typeSSNumeric;


        //new question type
        bool typenumber;
        bool typedate;
        bool typedoubledate;
        bool typeentryonly;

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

        string? label8;
        string? value8;

        string? label9;
        string? value9;

        string? answerid;

        Color? buttonborder;
        Color? buttonbackground;

        Color? buttonborder1;
        Color? buttonbackground1;

        Color? buttonborder2;
        Color? buttonbackground2;

        Color? buttonborder3;
        Color? buttonbackground3;

        Color? buttonborder4;
        Color? buttonbackground4;

        string? entryanswer;

        double slidervalue;

        bool checkboxthree;
        bool checkboxfour;
        bool checkboxfive;
        bool checkboxsix;
        bool checkboxseven;
        bool checkboxeight;
        bool checkboxnine;
        bool checkboxten;

        bool ischecked0;
        bool ischecked1;
        bool ischecked2;
        bool ischecked3;
        bool ischecked4;
        bool ischecked5;
        bool ischecked6;
        bool ischecked7;
        bool ischecked8;
        bool ischecked9;

        bool Usingchecked0;
        bool Usingchecked1;
        bool Usingchecked2;
        bool Usingchecked3;
        bool Usingchecked4;
        bool Usingchecked5;
        bool Usingchecked6;
        bool Usingchecked7;
        bool Usingchecked8;
        bool Usingchecked9;

        int valueselectednum;

        string? groupkey;

        bool addvisible;
        bool addvisible2;
        string? addquestion;
        string? addquestion2;

        bool headervis;
        Thickness bottommarginnum;
        string? blanktitle;
        bool ssvis;

        string? aqtype1number;
        string? aqtype1ss;

        string? aqdate;
        string? aqdate2;
        string? aqentry;

        string? aqtype3date1;
        string? aqtype3date2;
        string? aqtype3entry;


        bool UsingcheckedSS0;
        bool UsingcheckedSS1;
        bool UsingcheckedSS2;

        DateTime dateanswer;
        bool entryvis;
        bool entryvisms;

        string? dateday;
        string? datemonth;
        string? dateyear;
        ObservableCollection<Question>? datelist;

        Question? singledateselecteditem;
        double dateop;
        string? datelabel;
        string? datelabel2;

        bool headerinfo;
        string? headertext;
        Color? backgroundcolourchange;
        bool groupheadervis;

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }


        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "question")]
        public string? QuestionName
        {
            get { return question; }
            set { question = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "directions")]
        public string? Directions
        {
            get { return directions; }
            set { directions = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string? Type
        {
            get { return type; }
            set { type = value; }
        }


        [JsonProperty(PropertyName = "questionnaireid")]
        public string? Questionnaireid
        {
            get { return questionnaireid; }
            set { questionnaireid = value; }
        }

        [JsonProperty(PropertyName = "isactive")]
        public bool Isactive
        {
            get { return isactive; }
            set { isactive = value; }
        }


        [JsonProperty(PropertyName = "category")]
        public string? Category
        {
            get { return category; }
            set { category = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string? Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        [JsonProperty(PropertyName = "questionorder")]
        public int Questionorder
        {
            get { return questionorder; }
            set { questionorder = value; }
        }

        [JsonProperty(PropertyName = "referralcode")]
        public string? Signupcode
        {
            get { return signupcode; }
            set { signupcode = value; }
        }

        [JsonProperty(PropertyName = "area")]
        public string? Area
        {
            get { return area; }
            set { area = value; }
        }

        [JsonIgnore]
        public ObservableCollection<Answers>? Answerlist
        {
            get { return answerlist; }
            set
            {
                answerlist = value;



            }
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
        public bool TypeSSAdd
        {
            get { return typessadd; }
            set { typessadd = value; }
        }

        [JsonIgnore]
        public bool TypeSS1
        {
            get { return typess1; }
            set { typess1 = value; }
        }

        [JsonIgnore]
        public bool TypeSS2
        {
            get { return typess2; }
            set { typess2 = value; }
        }

        [JsonIgnore]
        public bool TypeSS3
        {
            get { return typess3; }
            set { typess3 = value; }
        }

        

        [JsonIgnore]
        public bool TypeSSNumeric
        {
            get { return typeSSNumeric; }
            set { typeSSNumeric = value; }
        }

        [JsonIgnore]
        public bool TypeNumber
        {
            get { return typenumber; }
            set { typenumber = value; }
        }

        [JsonIgnore]
        public bool TypeDate
        {
            get { return typedate; }
            set { typedate = value; }
        }

        [JsonIgnore]
        public bool TypeDoubleDate
        {
            get { return typedoubledate; }
            set { typedoubledate = value; }
        }

        [JsonIgnore]
        public bool TypeEntryOnly
        {
            get { return typeentryonly; }
            set { typeentryonly = value; }
        }

        //values for question type 0-3

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

        //add the fourth for the type 1-5
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

        //add the fourth for the type 1-5
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

        //add the fourth for the type 1-5
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

        //add the fourth for the type 1-5
        [JsonIgnore]
        public string? Label8
        {
            get { return label8; }
            set { label8 = value; }
        }

        [JsonIgnore]
        public string? Value8
        {
            get { return value8; }
            set { value8 = value; }
        }

        //add the fourth for the type 1-5
        [JsonIgnore]
        public string? Label9
        {
            get { return label9; }
            set { label9 = value; }
        }

        [JsonIgnore]
        public string? Value9
        {
            get { return value9; }
            set { value9 = value; }
        }

        [JsonIgnore]
        public string? Answerid
        {
            get { return answerid; }
            set { answerid = value; }
        }

        [JsonIgnore]
        public string? Aqtype3date1
        {
            get { return aqtype3date1; }
            set { aqtype3date1 = value; }
        }

        [JsonIgnore]
        public string? Aqtype3date2
        {
            get { return aqtype3date2; }
            set { aqtype3date2 = value; }
        }

        [JsonIgnore]
        public string Aqtype3entry
        {
            get { return aqtype3entry; }
            set { aqtype3entry = value; }
        }

        [JsonIgnore]
        public Color Buttonborder
        {
            get { return buttonborder; }
            set
            {
                buttonborder = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonborder)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonbackground
        {
            get { return buttonbackground; }
            set
            {
                buttonbackground = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonbackground)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonborder1
        {
            get { return buttonborder1; }
            set
            {
                buttonborder1 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonborder1)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonbackground1
        {
            get { return buttonbackground1; }
            set
            {
                buttonbackground1 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonbackground1)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonborder2
        {
            get { return buttonborder2; }
            set
            {
                buttonborder2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonborder2)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonbackground2
        {
            get { return buttonbackground2; }
            set
            {
                buttonbackground2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonbackground2)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonborder3
        {
            get { return buttonborder3; }
            set
            {
                buttonborder3 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonborder3)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonbackground3
        {
            get { return buttonbackground3; }
            set
            {
                buttonbackground3 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonbackground3)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonborder4
        {
            get { return buttonborder4; }
            set
            {
                buttonborder4 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonborder4)));
                }


            }
        }

        [JsonIgnore]
        public Color Buttonbackground4
        {
            get { return buttonbackground4; }
            set
            {
                buttonbackground4 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Buttonbackground4)));
                }


            }
        }



        [JsonIgnore]
        public string Entryanswer
        {
            get { return entryanswer; }
            set { entryanswer = value; }
        }

        [JsonIgnore]
        public double Slidervalue
        {
            get { return slidervalue; }
            set
            {
                slidervalue = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Slidervalue)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxthree
        {
            get { return checkboxthree; }
            set
            {
                checkboxthree = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxthree)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxfour
        {
            get { return checkboxfour; }
            set
            {
                checkboxfour = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxfour)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxfive
        {
            get { return checkboxfive; }
            set
            {
                checkboxfive = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxfive)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxsix
        {
            get { return checkboxsix; }
            set
            {
                checkboxsix = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxsix)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxseven
        {
            get { return checkboxseven; }
            set
            {
                checkboxseven = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxseven)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxeight
        {
            get { return checkboxeight; }
            set
            {
                checkboxeight = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxeight)));
                }


            }
        }


        [JsonIgnore]
        public bool Checkboxnine
        {
            get { return checkboxnine; }
            set
            {
                checkboxnine = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxnine)));
                }


            }
        }

        [JsonIgnore]
        public bool Checkboxten
        {
            get { return checkboxten; }
            set
            {
                checkboxten = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Checkboxten)));
                }


            }
        }

        //ischecked variables
        [JsonIgnore]
        public bool Ischecked0
        {
            get { return ischecked0; }
            set
            {
                ischecked0 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked0)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked1
        {
            get { return ischecked1; }
            set
            {
                ischecked1 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked1)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked2
        {
            get { return ischecked2; }
            set
            {
                ischecked2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked2)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked3
        {
            get { return ischecked3; }
            set
            {
                ischecked3 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked3)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked4
        {
            get { return ischecked4; }
            set
            {
                ischecked4 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked4)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked5
        {
            get { return ischecked5; }
            set
            {
                ischecked5 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked5)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked6
        {
            get { return ischecked6; }
            set
            {
                ischecked6 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked6)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked7
        {
            get { return ischecked7; }
            set
            {
                ischecked7 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked7)));
                }


            }
        }


        [JsonIgnore]
        public bool Ischecked8
        {
            get { return ischecked8; }
            set
            {
                ischecked8 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked8)));
                }


            }
        }

        [JsonIgnore]
        public bool Ischecked9
        {
            get { return ischecked9; }
            set
            {
                ischecked9 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Ischecked9)));
                }


            }
        }




        //using the checked options
        [JsonIgnore]
        public bool UsingChecked0
        {
            get { return Usingchecked0; }
            set
            {
                Usingchecked0 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked0)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked1
        {
            get { return Usingchecked1; }
            set
            {
                Usingchecked1 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked1)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked2
        {
            get { return Usingchecked2; }
            set
            {
                Usingchecked2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked2)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked3
        {
            get { return Usingchecked3; }
            set
            {
                Usingchecked3 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked3)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked4
        {
            get { return Usingchecked4; }
            set
            {
                Usingchecked4 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked4)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked5
        {
            get { return Usingchecked5; }
            set
            {
                Usingchecked5 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked5)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked6
        {
            get { return Usingchecked6; }
            set
            {
                Usingchecked6 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked6)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked7
        {
            get { return Usingchecked7; }
            set
            {
                Usingchecked7 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked7)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked8
        {
            get { return Usingchecked8; }
            set
            {
                Usingchecked8 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked8)));
                }


            }
        }

        [JsonIgnore]
        public bool UsingChecked9
        {
            get { return Usingchecked9; }
            set
            {
                Usingchecked9 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(UsingChecked9)));
                }


            }
        }



        [JsonIgnore]
        public int ValueSelectedNum
        {
            get { return valueselectednum; }
            set { valueselectednum = value; }
        }

        [JsonIgnore]
        public string GroupKey
        {
            get { return groupkey; }
            set { groupkey = value; }
        }


        [JsonIgnore]
        public bool AddVisible
        {
            get { return addvisible; }
            set
            {
                addvisible = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AddVisible)));
                }


            }
        }

        [JsonIgnore]
        public bool AddVisible2
        {
            get { return addvisible2; }
            set
            {
                addvisible2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AddVisible2)));
                }


            }
        }

        [JsonIgnore]
        public string Addquestion
        {
            get { return addquestion; }
            set { addquestion = value; }
        }

        [JsonIgnore]
        public string Addquestion2
        {
            get { return addquestion2; }
            set { addquestion2 = value; }
        }


        [JsonIgnore]
        public bool Headervis
        {
            get { return headervis; }
            set { headervis = value; }
        }

        [JsonIgnore]
        public Thickness BottomMarginNum
        {
            get { return bottommarginnum; }
            set { bottommarginnum = value; }
        }

        [JsonIgnore]
        public string BlankTitle
        {
            get { return blanktitle; }
            set { blanktitle = value; }
        }



        [JsonIgnore]
        public bool SSVis
        {
            get { return ssvis; }
            set
            {
                ssvis = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SSVis)));
                }


            }
        }


        [JsonIgnore]
        public string Aqtype1number
        {
            get { return aqtype1number; }
            set { aqtype1number = value; }
        }

        [JsonIgnore]
        public string Aqtype1ss
        {
            get { return aqtype1ss; }
            set { aqtype1ss = value; }
        }

        [JsonIgnore]
        public string Aqdate
        {
            get { return aqdate; }
            set { aqdate = value; }
        }

        [JsonIgnore]
        public string Aqdate2
        {
            get { return aqdate2; }
            set { aqdate2 = value; }
        }

        [JsonIgnore]
        public string Aqentry
        {
            get { return aqentry; }
            set { aqentry = value; }
        }

        [JsonIgnore]
        public bool UsingCheckedss0
        {
            get { return UsingcheckedSS0; }
            set { UsingcheckedSS0 = value; }
        }

        [JsonIgnore]
        public bool UsingCheckedss1
        {
            get { return UsingcheckedSS1; }
            set { UsingcheckedSS1 = value; }
        }

        [JsonIgnore]
        public bool UsingCheckedss2
        {
            get { return UsingcheckedSS2; }
            set { UsingcheckedSS2 = value; }
        }

        [JsonIgnore]
        public DateTime DateAnswer
        {
            get { return dateanswer; }
            set { dateanswer = value; }
        }

        [JsonIgnore]
        public bool Entryvis
        {
            get { return entryvis; }
            set
            {
                entryvis = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Entryvis)));
                }


            }
        }

        [JsonIgnore]
        public bool Entryvisms
        {
            get { return entryvisms; }
            set
            {
                entryvisms = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Entryvisms)));
                }


            }
        }


        [JsonIgnore]
        public string Dateday
        {
            get { return dateday; }
            set { dateday = value; }
        }

        [JsonIgnore]
        public string Datemonth
        {
            get { return datemonth; }
            set { datemonth = value; }
        }

        [JsonIgnore]
        public string Dateyear
        {
            get { return dateyear; }
            set { dateyear = value; }
        }


        public ObservableCollection<Question> Datelist
        {
            get { return datelist; }
            set
            {
                datelist = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(datelist)));
                }


            }
        }

        public Question Singledateselecteditem
        {
            get { return singledateselecteditem; }
            set
            {
                singledateselecteditem = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(singledateselecteditem)));
                }


            }
        }

        public double Dateop
        {
            get { return dateop; }
            set
            {
                dateop = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Dateop)));
                }


            }
        }

        public string Datelabel
        {
            get { return datelabel; }
            set
            {
                datelabel = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Datelabel)));
                }


            }
        }

        public string Datelabel2
        {
            get { return datelabel2; }
            set
            {
                datelabel2 = value;


                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Datelabel2)));
                }


            }
        }

        [JsonIgnore]
        public bool Headerinfo
        {
            get { return headerinfo; }
            set { headerinfo = value; }
        }


        [JsonIgnore]
        public string Headertext
        {
            get { return headertext; }
            set { headertext = value; }
        }

        [JsonIgnore]
        public Color Backgroundcolourchange
        {
            get { return backgroundcolourchange; }
            set { backgroundcolourchange = value; }
        }

        
        [JsonIgnore]
        public bool Groupheadervis
        {
            get { return groupheadervis; }
            set { groupheadervis = value; }
        }


        // event handler for updating the list views
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
