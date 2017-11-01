using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Filters
{
    public class PorterStemmer : IFilter
    {
        public List<string> Filter(List<string> words)
        {
            List<string> result = new List<string>();
            foreach (string word in words)
            {
                result.Add(Stem(word));
            }
            return result;
        }

        public static string Stem(string word)
        {
            char[] b = word.ToCharArray();
            int j = 0;
            int k = 0;

            k = b.Length-1;
            if (k > 1) 
            { 
                Step1(b, ref j, ref k); 
                Step2(b, ref j, ref k); 
                Step3(b, ref j, ref k); 
                Step4(b, ref j, ref k);
                Step5(b, ref j, ref k); 
                Step6(b, ref j, ref k); 
            }

            return new string(b, 0, k+1);
        }

        /// <summary>
        ///  m() measures the number of consonant sequences between 0 and j. if c is
        /// a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
        /// presence,
        /// <c><v>       gives 0
        /// <c>vc<v>     gives 1
        /// <c>vcvc<v>   gives 2
        /// <c>vcvcvc<v> gives 3
        /// ....
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        private static int GetConsonantSeqCount(char[] b, int j)
        {
            int n = 0;
            int i = 0;

            // check one vowel sequence
            while (true)
            {
                if (i > j) return n;
                if (!IsConsonant(b, i)) break;
                i++;
            }

            i++;
            
            while (true)
            {
                // check one constant sequence
                while (true)
                {
                    if (i > j) return n;
                    if (IsConsonant(b, i)) break;
                    i++;
                }
                i++;

                n++; // now one vowel sequence followed by one constant sequence is found, increment by 1

                // check one vowel sequence
                while (true)
                {
                    if (i > j) return n;
                    if (!IsConsonant(b, i)) break;
                    i++;
                }
                i++;
            }
        }

        private static bool IsConsonant(char[] b, int i)
        {
            switch (b[i])
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u': return false;
                case 'y': return (i == 0) ? true : !IsConsonant(b, i - 1);
                default: return true;
            }
        }

        /// <summary>
        /// vowelinstem() is true <=> 0,...j contains a vowel
        /// </summary>
        private static bool ContainsVowel(char[] b, int j)
        {
            int i; for (i = 0; i <= j; i++) if (!IsConsonant(b, i)) return true;
            return false;
        }

        /// <summary>
        /// cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
        /// and also if the second c is not w,x or y. this is used when trying to
        /// restore an e at the end of a short word. e.g.
        /// 
        /// cav(e), lov(e), hop(e), crim(e), but
        /// snow, box, tray.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static bool IsConsonantVowelConsonant(char[] b, int i)
        {
            if (i < 2 || !IsConsonant(b, i) || IsConsonant(b, i - 1) || !IsConsonant(b, i - 2)) return false;
            {
                int ch = b[i];
                if (ch == 'w' || ch == 'x' || ch == 'y') return false;
            }
            return true;
        }

        private static bool EndsWith(char[] b, ref int j, int k, string s)
        {
            int l = s.Length;
            int o = k - l + 1;
            if (o < 0) return false;
            for (int i = 0; i < l; i++) if (b[o + i] != s[i]) return false;
            j = k - l;
            return true;
        }

        private static void Replace(char[] b, int j, ref int k, string s)
        {
            int l = s.Length;
            int o = j + 1;
            for (int i = 0; i < l; i++) b[o + i] = s[i];
            k = j + l;
        }

        /// <summary>
        /// doublec(j) is true <=> j,(j-1) contain a double consonant.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private static bool IsDoubleConsonant(char[] b, int j)
        {
            if (j < 1) return false;
            if (b[j] != b[j - 1]) return false;
            return IsConsonant(b, j);
        }


        /// <summary>
        /// Step1() gets rid of plurals and -ed or -ing. e.g.
        ///
        ///     caresses  ->  caress
        ///     ponies    ->  poni
        ///     ties      ->  ti
        ///     caress    ->  caress
        ///     cats      ->  cat
        ///     feed      ->  feed
        ///     agreed    ->  agree
        ///     disabled  ->  disable
        ///     matting   ->  mat
        ///     mating    ->  mate
        ///     meeting   ->  meet
        ///     milling   ->  mill
        ///     messing   ->  mess
        ///     meetings  ->  meet
        /// </summary>
        /// <param name="b"></param>
        /// <param name="k"></param>
        private static void Step1(char[] b, ref int j, ref int k)
        {
            if (b[k] == 's')
            {
                if (EndsWith(b, ref j, k, "sses"))
                {
                    k -= 2;
                }
                else
                {
                    if (EndsWith(b, ref j, k, "ies"))
                    {
                        Replace(b, j, ref k, "i");
                    }
                    else
                    {
                        if (b[k - 1] != 's') k--;
                    }
                }
            }

            if (EndsWith(b, ref j, k, "eed")) 
            { 
                if (GetConsonantSeqCount(b, j) > 0) k--; 
            }
            else
            {
                if ((EndsWith(b, ref j, k, "ed") || EndsWith(b, ref j, k, "ing")) && ContainsVowel(b, j))
                {
                    k = j;
                    if (EndsWith(b, ref j, k, "at")) Replace(b, j, ref k, "ate");
                    else
                    {
                        if (EndsWith(b, ref j, k, "bl"))
                        {
                            Replace(b, j, ref k, "ble");
                        }
                        else
                        {
                            if (EndsWith(b, ref j, k, "iz")) Replace(b, j, ref k, "ize");
                            else
                            {
                                if (IsDoubleConsonant(b, k))
                                {
                                    k--;
                                    {
                                        int ch = b[k];
                                        if (ch == 'l' || ch == 's' || ch == 'z') k++;
                                    }
                                }
                                else if (GetConsonantSeqCount(b, j) == 1 && IsConsonantVowelConsonant(b, k))
                                {
                                    Replace(b, j, ref k, "e");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Step2() turns terminal y to i when there is another vowel in the stem.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        private static void Step2(char[] b, ref int j, ref int k) 
        {
            if (EndsWith(b, ref j, k, "y") && ContainsVowel(b, j))
            {
                b[k] = 'i';
            }
        }

        private static void r(char[] b, int j, ref int k, string s) 
        {
            if (GetConsonantSeqCount(b, j) > 0)
            {
                Replace(b, j, ref k, s);
            }
        }

        /// <summary>
        /// Step3() maps double suffices to single ones. so -ization ( = -ize plus -ation) maps to -ize etc. note that the string before the suffix must give m() > 0.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        private static void Step3(char[] b, ref int j, ref int k)
        {
            if (k == 0) return; 
            
            switch (b[k - 1])
            {
                case 'a': if (EndsWith(b, ref j, k, "ational")) { r(b, j, ref k, "ate"); break; }
                    if (EndsWith(b, ref j, k, "tional")) { r(b, j, ref k, "tion"); break; }
                    break;
                case 'c': if (EndsWith(b, ref j, k, "enci")) { r(b, j, ref k, "ence"); break; }
                    if (EndsWith(b, ref j, k, "anci")) { r(b, j, ref k, "ance"); break; }
                    break;
                case 'e': if (EndsWith(b, ref j, k, "izer")) { r(b, j, ref k, "ize"); break; }
                    break;
                case 'l': if (EndsWith(b, ref j, k, "bli")) { r(b, j, ref k, "ble"); break; }
                    if (EndsWith(b, ref j, k, "alli")) { r(b, j, ref k, "al"); break; }
                    if (EndsWith(b, ref j, k, "entli")) { r(b, j, ref k, "ent"); break; }
                    if (EndsWith(b, ref j, k, "eli")) { r(b, j, ref k, "e"); break; }
                    if (EndsWith(b, ref j, k, "ousli")) { r(b, j, ref k, "ous"); break; }
                    break;
                case 'o': if (EndsWith(b, ref j, k, "ization")) { r(b, j, ref k, "ize"); break; }
                    if (EndsWith(b, ref j, k, "ation")) { r(b, j, ref k, "ate"); break; }
                    if (EndsWith(b, ref j, k, "ator")) { r(b, j, ref k, "ate"); break; }
                    break;
                case 's': if (EndsWith(b, ref j, k, "alism")) { r(b, j, ref k, "al"); break; }
                    if (EndsWith(b, ref j, k, "iveness")) { r(b, j, ref k, "ive"); break; }
                    if (EndsWith(b, ref j, k, "fulness")) { r(b, j, ref k, "ful"); break; }
                    if (EndsWith(b, ref j, k, "ousness")) { r(b, j, ref k, "ous"); break; }
                    break;
                case 't': if (EndsWith(b, ref j, k, "aliti")) { r(b, j, ref k, "al"); break; }
                    if (EndsWith(b, ref j, k, "iviti")) { r(b, j, ref k, "ive"); break; }
                    if (EndsWith(b, ref j, k, "biliti")) { r(b, j, ref k, "ble"); break; }
                    break;
                case 'g': 
                    if (EndsWith(b, ref j, k, "logi")) { r(b, j, ref k, "log"); break; }
                    break;
            }
        }

        /// <summary>
        /// Step4() deals with -ic-, -full, -ness etc. similar strategy to Step3.
        /// </summary>
        private static void Step4(char[] b, ref int j, ref int k)
        {
            switch (b[k])
            {
                case 'e': if (EndsWith(b, ref j, k, "icate")) { r(b, j, ref k, "ic"); break; }
                    if (EndsWith(b, ref j, k, "ative")) { r(b, j, ref k, ""); break; }
                    if (EndsWith(b, ref j, k, "alize")) { r(b, j, ref k, "al"); break; }
                    break;
                case 'i': if (EndsWith(b, ref j, k, "iciti")) { r(b, j, ref k, "ic"); break; }
                    break;
                case 'l': if (EndsWith(b, ref j, k, "ical")) { r(b, j, ref k, "ic"); break; }
                    if (EndsWith(b, ref j, k, "ful")) { r(b, j, ref k, ""); break; }
                    break;
                case 's': if (EndsWith(b, ref j, k, "ness")) { r(b, j, ref k, ""); break; }
                    break;
            }
        }

        /// <summary>
        /// Step5() takes off -ant, -ence etc., in context <c>vcvc<v>.
        /// </summary>
        private static void Step5(char[] b, ref int j, ref int k)
        {
            if (k == 0) return; /* for Bug 1 */ switch (b[k - 1])
            {
                case 'a': if (EndsWith(b, ref j, k, "al")) break; return;
                case 'c': if (EndsWith(b, ref j, k, "ance")) break;
                    if (EndsWith(b, ref j, k, "ence")) break; return;
                case 'e': if (EndsWith(b, ref j, k, "er")) break; return;
                case 'i': if (EndsWith(b, ref j, k, "ic")) break; return;
                case 'l': if (EndsWith(b, ref j, k, "able")) break;
                    if (EndsWith(b, ref j, k, "ible")) break; return;
                case 'n': if (EndsWith(b, ref j, k, "ant")) break;
                    if (EndsWith(b, ref j, k, "ement")) break;
                    if (EndsWith(b, ref j, k, "ment")) break;
                    /* element etc. not stripped before the m */
                    if (EndsWith(b, ref j, k, "ent")) break; return;
                case 'o': if (EndsWith(b, ref j, k, "ion") && j >= 0 && (b[j] == 's' || b[j] == 't')) break;
                    /* j >= 0 fixes Bug 2 */
                    if (EndsWith(b, ref j, k, "ou")) break; return;
                /* takes care of -ous */
                case 's': if (EndsWith(b, ref j, k, "ism")) break; return;
                case 't': if (EndsWith(b, ref j, k, "ate")) break;
                    if (EndsWith(b, ref j, k, "iti")) break; return;
                case 'u': if (EndsWith(b, ref j, k, "ous")) break; return;
                case 'v': if (EndsWith(b, ref j, k, "ive")) break; return;
                case 'z': if (EndsWith(b, ref j, k, "ize")) break; return;
                default: return;
            }
            if (GetConsonantSeqCount(b, j) > 1) k = j;
        }

        /// <summary>
        /// Step6() removes a final -e if m() > 1.
        /// </summary>
       private static void Step6(char[] b, ref int j, ref int k)
       {
           j = k;
           if (b[k] == 'e')
           {
               int a = GetConsonantSeqCount(b, j);
               if (a > 1 || a == 1 && !IsConsonantVowelConsonant(b, k - 1)) k--;
           }
           if (b[k] == 'l' && IsDoubleConsonant(b, k) && GetConsonantSeqCount(b, j) > 1) k--;
       }

    }


}
