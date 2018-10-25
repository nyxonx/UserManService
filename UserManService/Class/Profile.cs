using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserMan
{
    public class Profile
    {
        public string Name { get; set; }
        public string UpTime { get; set; }
        public string Limit { get; set; }

        public Profile(string name, string uptime, string limit)
        {
            this.Name = name;
            this.UpTime = uptime;
            this.Limit = limit;
        }

        public static List<Profile> GetProfileMT(MK mikrotik)
        {
            List<Profile> response = new List<Profile>();

            try
            {
                mikrotik.Send("/ip/hotspot/user/profile/print", true);

                foreach (var item in mikrotik.Read())
                {
                    string temp = getValue("name=", item);
                    if (temp != "")
                    {
                        string uptime = getValue("session-timeout=", item);
                        string limit = getValue("rate-limit=", item);
                        response.Add(new Profile(temp, uptime, limit));
                    }

                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
            }

            return response;
        }

        private static string getValue(string parametar, string row)
        {
            string temp = "";
            int startValue = 0;

            if (row.IndexOf(parametar) > 0)
            {
                startValue = row.IndexOf(parametar) + parametar.Length;
                temp = row.Substring(startValue);
                temp = temp.Substring(0, temp.IndexOf("="));
            }

            return temp;
        }
    }


}
