﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLibrary
{

    public class OpenLibrarySearch
    {
        public int start { get; set; }
        public int num_found { get; set; }
        public int numFound { get; set; }
        public Doc[] docs { get; set; }
    }

    public class Doc
    {
        public string title_suggest { get; set; }
        public string[] edition_key { get; set; }
        public int? cover_i { get; set; }
        public string[] isbn { get; set; }
        public bool has_fulltext { get; set; }
        public string[] id_depósito_legal { get; set; }
        public string[] text { get; set; }
        public string[] author_name { get; set; }
        public string[] contributor { get; set; }
        public string[] ia_loaded_id { get; set; }
        public string[] seed { get; set; }
        public string[] oclc { get; set; }
        public string[] id_google { get; set; }
        public string[] ia { get; set; }
        public string[] author_key { get; set; }
        public string[] subject { get; set; }
        public string title { get; set; }
        public string lending_identifier_s { get; set; }
        public string ia_collection_s { get; set; }
        public int? first_publish_year { get; set; }
        public string type { get; set; }
        public int ebook_count_i { get; set; }
        public string[] publish_place { get; set; }
        public string[] ia_box_id { get; set; }
        public int edition_count { get; set; }
        public string key { get; set; }
        public string[] id_alibris_id { get; set; }
        public string[] id_goodreads { get; set; }
        public string[] author_alternative_name { get; set; }
        public bool public_scan_b { get; set; }
        public string[] id_overdrive { get; set; }
        public string[] publisher { get; set; }
        public string[] id_amazon { get; set; }
        public string[] id_paperback_swap { get; set; }
        public string[] id_canadian_national_library_archive { get; set; }
        public string[] language { get; set; }
        public string[] lccn { get; set; }
        public int last_modified_i { get; set; }
        public string lending_edition_s { get; set; }
        public string[] id_librarything { get; set; }
        public string cover_edition_key { get; set; }
        public string[] person { get; set; }
        public int[] publish_year { get; set; }
        public string printdisabled_s { get; set; }
        public string[] place { get; set; }
        public string[] time { get; set; }
        public string[] publish_date { get; set; }
        public string[] id_wikidata { get; set; }
        public string[] first_sentence { get; set; }
        public string subtitle { get; set; }
        public string[] id_bcid { get; set; }
        public string[] top_subjects { get; set; }
        public string name { get; set; }
        public string death_date { get; set; }
        public string[] alternate_names { get; set; }
        public string birth_date { get; set; }
        public string top_work { get; set; }
        public int work_count { get; set; }
    }

}
