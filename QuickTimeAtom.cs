using System;
using System.Collections.Generic;

namespace QuickTimeParser
{
    static class QuickTimeAtom
    {
        private static readonly Dictionary<string, string> atoms = new Dictionary<string, string>
        {
            {"moov", "MOVie container atom"},
            {"trak", "TRAcK container atom"},
            {"clip", "CLIPping container atom"},
            {"matt", "track MATTe container atom"},
            {"edts", "EDiTS container atom"},
            {"tref", "Track REFerence container atom"},
            {"mdia", "MeDIA container atom"},
            {"minf", "Media INFormation container atom"},
            {"dinf", "Data INFormation container atom"},
            {"udta", "User DaTA container atom"},
            {"cmov", "Compressed MOVie container atom"},
            {"rmra", "Reference Movie Record Atom"},
            {"rmda", "Reference Movie Descriptor Atom"},
            {"gmhd", "Generic Media info HeaDer atom (seen on QTVR)"},
            {"stbl", "Sample TaBLe container atom"},
            {"©cpy", ""},
            {"©day", ""},
            {"©dir", ""},
            {"©ed1", ""},
            {"©ed2", ""},
            {"©ed3", ""},
            {"©ed4", ""},
            {"©ed5", ""},
            {"©ed6", ""},
            {"©ed7", ""},
            {"©ed8", ""},
            {"©ed9", ""},
            {"©fmt", ""},
            {"©inf", ""},
            {"©prd", ""},
            {"©prf", ""},
            {"©req", ""},
            {"©src", ""},
            {"©wrt", ""},
            {"©nam", ""},
            {"©cmt", ""},
            {"©wrn", ""},
            {"©hst", ""},
            {"©mak", ""},
            {"©mod", ""},
            {"©PRD", ""},
            {"©swr", ""},
            {"©aut", ""},
            {"©ART", ""},
            {"©trk", ""},
            {"©alb", ""},
            {"©com", ""},
            {"©gen", ""},
            {"©ope", ""},
            {"©url", ""},
            {"©enc", ""},
            {"play", "auto-PLAY atom"},
            {"WLOC", "Window LOCation atom"},
            {"LOOP", "LOOPing atom"},
            {"SelO", "play SELection Only atom"},
            {"AllF", "play ALL Frames atom"},
            {"name", ""},
            {"MCPS", "Media Cleaner PRo"},
            {"@PRM", "adobe PReMiere version"},
            {"@PRQ", "adobe PRemiere Quicktime version"},
            {"cmvd", "Compressed MooV Data atom"},
            {"dcom", "Data COMpression atom"},
            {"rdrf", "Reference movie Data ReFerence atom"},
            {"url ", ""},
            {"alis", ""},
            {"rsrc", ""},
            {"rmqu", "Reference Movie QUality atom"},
            {"rmcs", "Reference Movie Cpu Speed atom"},
            {"rmvc", "Reference Movie Version Check atom"},
            {"rmcd", "Reference Movie Component check atom"},
            {"rmdr", "Reference Movie Data Rate atom"},
            {"rmla", "Reference Movie Language Atom"},
            {"ptv ", "Print To Video - defines a movie's full screen mode"},
            {"stsd", "Sample Table Sample Description atom"},
            {"avc1", ""},
            {"mp4v", ""},
            {"qtvr", ""},
            {"mp4a", ""},
            {"raw ", "PCM"},
            {"alac", "Apple Lossless Audio Codec"},
            {"mp4s", ""},
            {"3ivx", ""},
            {"3iv1", ""},
            {"3iv2", ""},
            {"xvid", ""},
            {"divx", ""},
            {"div1", ""},
            {"div2", ""},
            {"div3", ""},
            {"div4", ""},
            {"div5", ""},
            {"div6", ""},
            {"stts", "Sample Table Time-to-Sample atom"},
            {"stss", "Sample Table Sync Sample (key frames) atom"},
            {"stsc", "Sample Table Sample-to-Chunk atom"},
            {"stsz", "Sample Table SiZe atom"},
            {"stco", "Sample Table Chunk Offset atom"},
            {"co64", "Chunk Offset 64-bit (version of \"stco\" that supports > 2GB files)"},
            {"dref", "Data REFerence atom"},
            {"gmin", "base Media INformation atom"},
            {"smhd", "Sound Media information HeaDer atom"},
            {"vmhd", "Video Media information HeaDer atom"},
            {"hdlr", "HanDLeR reference atom"},
            {"mdhd", "MeDia HeaDer atom"},
            {"pnot", "Preview atom"},
            {"crgn", "Clipping ReGioN atom"},
            {"load", "track LOAD settings atom"},
            {"tmcd", "TiMe CoDe atom"},
            {"chap", "CHAPter list atom"},
            {"sync", "SYNChronization atom"},
            {"scpt", "tranSCriPT atom"},
            {"ssrc", "non-primary SouRCe atom"},
            {"elst", "Edit LiST atom"},
            {"kmat", "compressed MATte atom"},
            {"ctab", "Color TABle atom"},
            {"mvhd", "MoVie HeaDer atom"},
            {"tkhd", "TracK HeaDer atom"},
            {"meta", "METAdata atom"},
            {"ftyp", "FileTYPe (?) atom (for MP4 it seems)"},
            {"mdat", "Media DATa atom"},
            {"free", "FREE space atom"},
            {"skip", "SKIP atom"},
            {"wide", "64-bit expansion placeholder atom"},
            {"nsav", "NoSAVe atom"},
            {"ctyp", "Controller TYPe atom (seen on QTVR)"},
            {"pano", "PANOrama track (seen on QTVR)"},
            {"hint", "HINT track"},
            {"hinf", ""},
            {"hinv", ""},
            {"hnti", ""},
            {"imgt", "IMaGe Track reference (kQTVRImageTrackRefType) (seen on QTVR)"},
            {"FXTC", "Something to do with Adobe After Effects (?)"},
            {"PrmA", ""},
            {"code", ""},
            {"FIEL", ""}
        };

        /// <summary>
        /// Checks if found string is valid QuickTime atom
        /// </summary>
        /// <param name="atomName">Name of the atom</param>
        /// <returns>True if found string is valid QuickTime atom, False otherwise</returns>
        public static bool IsValidAtom(string atomName)
        {
            return atoms.ContainsKey(atomName);
        }

        /// <summary>
        /// Checks if found atom is a container 
        /// </summary>
        /// <param name="atomType">Name of the atom</param>
        /// <returns>True if found atom is a container, False otherwise</returns>
        public static bool IsContainerAtom(string atomType)
        {
            string[] containerAtoms = {
                "moov", "trak", "mdia", "minf", "stbl", "edts", "udta", "meta",
                "ilst", "dinf", "mvex", "moof", "traf", "gmhd", "rmra", "rmda"
            };
            return Array.IndexOf(containerAtoms, atomType) >= 0;
        }

        /// <summary>
        /// Checks if found string is unused space atom
        /// </summary>
        /// <param name="atomType">Name of the atom</param>
        /// <returns>True if found atom is an unused space atom, False otherwise</returns>
        public static bool IsUnusedSpaceAtom(string atomType)
        {
            return atomType == "free" ||    // An atom that designates unused space in the movie data file.
                   atomType == "skip" ||    // An atom that designates unused space in the movie data file.
                   atomType == "wide";      // An atom that designates reserved space in the movie data file.
        }
    }
}
