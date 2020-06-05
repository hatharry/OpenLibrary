using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLibrary
{

    public class OpenLibraryResp
    {
        public Classifications classifications { get; set; }
        public string title { get; set; }
        public Cover cover { get; set; }
        public Subject[] subjects { get; set; }
        public Author[] authors { get; set; }
        public string publish_date { get; set; }
    }

    public class Classifications
    {
        public string[] dewey_decimal_class { get; set; }
        public string[] lc_classifications { get; set; }
    }

    public class Cover
    {
        public string small { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
    }

    public class Subject
    {
        public string url { get; set; }
        public string name { get; set; }
    }

    public class Author
    {
        public string url { get; set; }
        public string name { get; set; }
    }


}
