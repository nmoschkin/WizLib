using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBulb
{
    public class RecentFile
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();

        public string FileName { get; set; }

        public static bool TryParse(string text, out RecentFile result)
        {
            RecentFile res = Parse(text);

            if (res == null)
            {
                result = null;
                return false;
            }

            result = res;
            return true;
        }

        public static RecentFile Parse(string text)
        {

            try
            {
                string[] p = text.Split('|');

                if (p == null || p.Length == 0)
                {
                    return null;
                }
                else if (p.Length == 2)
                {
                    if (Guid.TryParse(p[1], out Guid res))
                    {
                        return new RecentFile()
                        {
                            FileName = p[0],
                            ProjectId = res
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (p.Length == 1)
                {
                    return new RecentFile() { FileName = text };
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }

        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", FileName, ProjectId);
        }

    }
}
